﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dota2.Base.Data;
using Dota2.GC.Dota.Internal;
using Dota2.GC.Internal;
using Dota2.Utils;
using ProtoBuf;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.Internal;
using KeyValue = Dota2.Backports.KeyValue;

namespace Dota2.GC
{
    /// <summary>
    ///     This handler handles all Dota 2 GC lobbies interaction.
    /// </summary>
    public sealed partial class DotaGCHandler : ClientMsgHandler
    {
        /// <summary>
        ///     Rich presence type.
        /// </summary>
        public enum RPType
        {
            /// <summary>
            ///     Initial state.
            /// </summary>
            Init,

            /// <summary>
            ///     Currently playing a game.
            /// </summary>
            Play,

            /// <summary>
            ///     Queuing for a match.
            /// </summary>
            Queue,

            /// <summary>
            ///     No state? Menu probably.
            /// </summary>
            None,

            /// <summary>
            ///     Automatically decide state.
            /// </summary>
            Auto
        }

        private readonly Timer _gcConnectTimer;
        private bool _running;

        /// <summary>
        ///     Internally create an instance of the GC handler.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="appId"></param>
        /// <param name="_engine"></param>
        internal DotaGCHandler(SteamClient client, Games appId, ESourceEngine _engine)
        {
            GameId = appId;
            Engine = _engine;
            SteamClient = client;
            // Usually we'd have around 200-600 items.
            EconItems = new Dictionary<ulong, CSOEconItem>(300);
            // Reasonably a bot wouldn't have very many of these.
            LeagueViewPasses = new Dictionary<ulong, CSOEconItemLeagueViewPass>(5);
            // Generally this seems to be 2
            MapLocationStates = new Dictionary<int, CSODOTAMapLocationState>(2);
            _gcConnectTimer = new Timer(
                new TimerCallback((stateInfo) => {
                    if (!_running)
                    {
                        _gcConnectTimer.Change(0, Timeout.Infinite);
                        return;
                    }
                    SayHello();
                }),
                null,
                0,
                Timeout.Infinite
            );
        }

        /// <summary>
        ///     The Game ID the handler will use. Defaults to Main Client.
        /// </summary>
        public Games GameId { get; }

        /// <summary>
        ///     The engine to use.
        /// </summary>
        public ESourceEngine Engine { get; }

        /// <summary>
        ///     Is the GC ready?
        /// </summary>
        public bool Ready { get; private set; }

        /// <summary>
        ///     The current up to date lobby
        /// </summary>
        /// <value>The lobby.</value>
        public CSODOTALobby Lobby { get; private set; }

        /// <summary>
        ///     The current up to date party.
        /// </summary>
        public CSODOTAParty Party { get; private set; }

        /// <summary>
        ///     The active invite to the party.
        /// </summary>
        public CSODOTAPartyInvite PartyInvite { get; private set; }

        /// <summary>
        ///     The active incoming invite to the lobby.
        /// </summary>
        public CSODOTALobbyInvite LobbyInvite { get; private set; }

        /// <summary>
        ///     Last invitation to the game.
        /// </summary>
        public CMsgClientInviteToGame Invitation { get; private set; }

        /// <summary>
        ///     The underlying SteamClient.
        /// </summary>
        public SteamClient SteamClient { get; private set; }

        /// <summary>
        ///     Econ items.
        /// </summary>
        public Dictionary<ulong, CSOEconItem> EconItems { get; }

        /// <summary>
        ///     League view passes.
        /// </summary>
        public Dictionary<ulong, CSOEconItemLeagueViewPass> LeagueViewPasses { get; }

        /// <summary>
        ///     Ping map view states.
        /// </summary>
        public Dictionary<int, CSODOTAMapLocationState> MapLocationStates { get; }

        /// <summary>
        ///     Contains various information about our player.
        /// </summary>
        public CSOEconGameAccountClient GameAccountClient { get; set; }

        /// <summary>
        ///     Setup the DOTA 2 GC handler on an existing client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="appId">Optional, specify the GC to communicate with.</param>
        /// <param name="engine">Optional, _engine to connect to. Default source2.</param>
        public static void Bootstrap(SteamClient client, Games appId = Games.DOTA2,
            ESourceEngine engine = ESourceEngine.k_ESE_Source2)
        {
            client.AddHandler(new DotaGCHandler(client, appId, engine));
        }

        /// <summary>
        ///     Sends a game coordinator message.
        /// </summary>
        /// <param name="msg">The GC message to send.</param>
        public void Send(IClientGCMsg msg)
        {
            var clientMsg = new ClientMsgProtobuf<CMsgGCClient>(EMsg.ClientToGC);

            clientMsg.Body.msgtype = MsgUtil.MakeGCMsg(msg.MsgType, msg.IsProto);
            clientMsg.Body.appid = (uint) GameId;

            clientMsg.Body.payload = msg.Serialize();

            Client.Send(clientMsg);
        }

        /// <summary>
        ///     Old method of starting the DOTA client.
        /// </summary>
        [Obsolete("LaunchDota is deprecated, use Start/Stop instead.")]
        public void LaunchDota()
        {
            Start();
        }

        /// <summary>
        ///     Start playing DOTA 2 and automatically request a GC session.
        /// </summary>
        public void Start()
        {
            _running = true;

            var launchEvent = new ClientMsg<MsgClientAppUsageEvent>();
            launchEvent.Body.AppUsageEvent = EAppUsageEvent.GameLaunch;
            launchEvent.Body.GameID = new GameID {AppID = (uint) GameId, AppType = SteamKit2.GameID.GameType.App};
            Client.Send(launchEvent);

            UploadRichPresence(RPType.Init);

            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayedWithDataBlob);
            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = (ulong) GameId,
                game_extra_info = "Dota 2",
                game_data_blob = null,
                streaming_provider_id = 0,
                game_flags = (uint) Engine,
                owner_id = Client.SteamID.AccountID
            });
            playGame.Body.client_os_type = 16;

            Client.Send(playGame);

            SayHello();
            _gcConnectTimer.Change(0, 5000);
        }

        /// <summary>
        ///     Uploads rich presence for a variety of situations.
        ///     <param name="inp">Type of RP event to upload.</param>
        /// </summary>
        public void UploadRichPresence(RPType inp = RPType.Auto)
        {
            if (inp == RPType.Auto)
            {
                if (!Ready) inp = RPType.None;
                else if (Lobby != null && Lobby.state == CSODOTALobby.State.RUN) inp = RPType.Play;
                else inp = RPType.Init;
            }

            // Update rich presence
            var kv = new KeyValue("RP");
            switch (inp)
            {
                case RPType.None:
                    break;
                // todo: implement playing rich presence
                case RPType.Play:
                case RPType.Queue:
                case RPType.Init:
                    kv.Children.Add(new KeyValue("status", "#DOTA_RP_INIT"));
                    kv.Children.Add(new KeyValue("num_params", "0"));
                    kv.Children.Add(new KeyValue("_engine", ((uint) Engine).ToString()));
                    if (Lobby != null)
                        kv.Children.Add(new KeyValue("lobby",
                            "lobbyId: " + Lobby.lobby_id + " lobby_state: " + Lobby.state.ToString("G") + " password: " +
                            (Lobby.pass_key.Length == 0 ? "false" : "true") + " game_mode: " +
                            ((DOTA_GameMode) Lobby.game_mode).ToString("G") + " member_count: " + Lobby.members.Count +
                            " max_member_count: " + (Lobby.custom_max_players == 0 ? 10 : Lobby.custom_max_players) +
                            " name: \"" + Lobby.game_name + "\""));
                    break;
            }
            var rpup = new ClientMsgProtobuf<CMsgClientRichPresenceUpload>(EMsg.ClientRichPresenceUpload);
            using (var ms = new MemoryStream())
            {
                kv.SaveToStream(ms, true);
                rpup.Body.rich_presence_kv = ms.ToArray();
            }
            var friends = Client.GetHandler<SteamFriends>();
            for (var i = 0; i < friends.GetFriendCount(); i++)
            {
                var sid = friends.GetFriendByIndex(i);
                var relationship = friends.GetFriendRelationship(sid);
                if (relationship == EFriendRelationship.Friend &&
                    friends.GetFriendGamePlayed(sid) == new GameID((uint) GameId))
                    rpup.Body.steamid_broadcast.Add(sid.ConvertToUInt64());
            }
            Client.Send(rpup);
        }

        /// <summary>
        ///     Send the hello message requesting a GC session. Do not call this manually!
        /// </summary>
        public void SayHello()
        {
            if (!_running) return;
            var clientHello = new ClientGCMsgProtobuf<Internal.CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello);
            clientHello.Body.client_launcher = PartnerAccountType.PARTNER_NONE;
            clientHello.Body.engine = Engine;
            clientHello.Body.secret_key = "";
            clientHello.Body.client_session_need = 104;
            Send(clientHello);
        }

        /// <summary>
        ///     Stop playing DOTA 2.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _gcConnectTimer.Change(0, Timeout.Infinite);

            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            // playGame.Body.games_played left empty
            Client.Send(playGame);

            UploadRichPresence(RPType.None);
        }

        /// <summary>
        ///     Abandon the current game
        /// </summary>
        public void AbandonGame()
        {
            var abandon = new ClientGCMsgProtobuf<CMsgAbandonCurrentGame>((uint) EDOTAGCMsg.k_EMsgGCAbandonCurrentGame);
            Send(abandon);
        }

        /// <summary>
        ///     Cancel the queue for a match
        /// </summary>
        public void StopQueue()
        {
            var queue = new ClientGCMsgProtobuf<CMsgStopFindingMatch>((uint) EDOTAGCMsg.k_EMsgGCStopFindingMatch);
            Send(queue);
        }

        /// <summary>
        ///     Respond to a party invite
        /// </summary>
        /// <param name="party_id"></param>
        /// <param name="accept"></param>
        public void RespondPartyInvite(ulong party_id, bool accept = true)
        {
            var invite = new ClientGCMsgProtobuf<CMsgPartyInviteResponse>((uint) EGCBaseMsg.k_EMsgGCPartyInviteResponse);
            invite.Body.party_id = party_id;
            invite.Body.accept = accept;
            Send(invite);
        }

        /// <summary>
        ///     Respond to a lobby invite
        /// </summary>
        /// <param name="lobbyId">Lobby ID</param>
        /// <param name="accept">accept lobby invite, true/false</param>
        /// <param name="customGameCrc">If responding to a custom game invite, the crc of te game</param>
        /// <param name="customGameTimestamp">If responding to a custom game invite, the timestamp of the download.</param>
        public void RespondLobbyInvite(ulong lobbyId, bool accept = true, ulong customGameCrc = 0,
            uint customGameTimestamp = 0)
        {
            var invite = new ClientGCMsgProtobuf<CMsgLobbyInviteResponse>((uint) EGCBaseMsg.k_EMsgGCLobbyInviteResponse);
            invite.Body.lobby_id = lobbyId;
            invite.Body.accept = accept;
            if (customGameCrc != 0)
                invite.Body.custom_game_crc = customGameCrc;
            if (customGameTimestamp != 0)
                invite.Body.custom_game_timestamp = customGameTimestamp;
            Send(invite);
        }

        /// <summary>
        ///     Join a lobby given a lobby ID
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <param name="passKey"></param>
        public void JoinLobby(ulong lobbyId, string passKey = null)
        {
            var joinLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoin>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoin);
            joinLobby.Body.lobby_id = lobbyId;
            joinLobby.Body.pass_key = passKey;
            Send(joinLobby);
        }

        /// <summary>
        ///     Attempt to leave a lobby
        /// </summary>
        public void LeaveLobby()
        {
            var leaveLobby =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
            Send(leaveLobby);
        }

        /// <summary>
        ///     Attempt to leave a party.
        /// </summary>
        public void LeaveParty()
        {
            var leaveParty = new ClientGCMsgProtobuf<CMsgLeaveParty>((uint) EGCBaseMsg.k_EMsgGCLeaveParty);
            Send(leaveParty);
        }

        /// <summary>
        ///     Respond to a ping()
        /// </summary>
        public void Pong()
        {
            var pingResponse = new ClientGCMsgProtobuf<CMsgGCClientPing>((uint) EGCBaseClientMsg.k_EMsgGCPingResponse);
            Send(pingResponse);
        }

        /// <summary>
        ///     Joins a broadcast channel in the lobby
        /// </summary>
        /// <param name="channel">The channel slot to join. Valid channel values range from 0 to 5.</param>
        public void JoinBroadcastChannel(uint channel = 0)
        {
            var joinChannel =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinBroadcastChannel>(
                    (uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinBroadcastChannel);
            joinChannel.Body.channel = channel;
            Send(joinChannel);
        }

        /// <summary>
        ///     Join a team
        /// </summary>
        /// <param name="team">The team to join.</param>
        public void JoinCoachSlot(DOTA_GC_TEAM team = DOTA_GC_TEAM.DOTA_GC_TEAM_GOOD_GUYS)
        {
            var joinChannel =
                new ClientGCMsgProtobuf<CMsgPracticeLobbySetCoach>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbySetCoach)
                {
                    Body = {team = team}
                };
            Send(joinChannel);
        }

        /// <summary>
        ///     Requests a subscription refresh for a specific cache ID.
        /// </summary>
        /// <param name="type">the type of the cache</param>
        /// <param name="id">the cache soid</param>
        public void RequestSubscriptionRefresh(uint type, ulong id)
        {
            var refresh =
                new ClientGCMsgProtobuf<CMsgSOCacheSubscriptionRefresh>((uint) ESOMsg.k_ESOMsg_CacheSubscriptionRefresh);
            refresh.Body.owner_soid = new CMsgSOIDOwner
            {
                id = id,
                type = type
            };
            Send(refresh);
        }

        /// <summary>
        ///     Send a request for player information.
        /// </summary>
        /// <param name="ids">DOTA 2 profile ids.</param>
        public void RequestPlayerInfo(IEnumerable<uint> ids)
        {
            var req = new ClientGCMsgProtobuf<CMsgGCPlayerInfoRequest>((uint) EDOTAGCMsg.k_EMsgGCPlayerInfoRequest);
            req.Body.player_infos.AddRange(ids.Select(m => new CMsgGCPlayerInfoRequest.PlayerInfo() {account_id = m}));
            Send(req);
        }

        /// <summary>
        ///     Requests the entire pro team list.
        /// </summary>
        public void RequestProTeamList()
        {
            var req = new ClientGCMsgProtobuf<CMsgDOTAProTeamListRequest>((uint) EDOTAGCMsg.k_EMsgGCProTeamListRequest);
            Send(req);
        }

        /// <summary>
        ///     Switches team in a GC Lobby.
        /// </summary>
        /// <param name="team">target team</param>
        /// <param name="slot">slot on the team</param>
        /// <param name="botDifficulty">set bot difficulty for slot</param>
        public void JoinTeam(DOTA_GC_TEAM team, uint slot = 1, DOTABotDifficulty botDifficulty = DOTABotDifficulty.BOT_DIFFICULTY_EXTRA3)
        {
            var joinSlot =
                new ClientGCMsgProtobuf<CMsgPracticeLobbySetTeamSlot>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbySetTeamSlot);
            joinSlot.Body.team = team;
            joinSlot.Body.slot = slot;
            if (botDifficulty != DOTABotDifficulty.BOT_DIFFICULTY_EXTRA3)
                joinSlot.Body.bot_difficulty = botDifficulty;
            Send(joinSlot);
        }

        /// <summary>
        /// Adds or removes a bot from a slot. Alias for JoinTeam with difficulty param.
        /// </summary>
        /// <param name="team">team</param>
        /// <param name="slot">slot</param>
        /// <param name="botDifficulty">difficulty</param>
        public void SetBotSlotDifficulty(DOTA_GC_TEAM team, uint slot = 1,
            DOTABotDifficulty botDifficulty = DOTABotDifficulty.BOT_DIFFICULTY_PASSIVE)
        {
            JoinTeam(team, slot, botDifficulty);
        }

        /// <summary>
        ///     Start the game
        /// </summary>
        public void LaunchLobby()
        {
            Send(new ClientGCMsgProtobuf<CMsgPracticeLobbyLaunch>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLaunch));
        }

        /// <summary>
        ///     Create a practice or tournament or custom lobby.
        /// </summary>
        /// <param name="passKey">Password for the lobby.</param>
        /// <param name="details">Lobby options.</param>
        public void CreateLobby(string passKey, CMsgPracticeLobbySetDetails details)
        {
            var create = new ClientGCMsgProtobuf<CMsgPracticeLobbyCreate>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyCreate);
            create.Body.pass_key = passKey;
            create.Body.lobby_details = details;
            create.Body.lobby_details.pass_key = passKey;
            create.Body.lobby_details.visibility = DOTALobbyVisibility.DOTALobbyVisibility_Public;
            if (string.IsNullOrWhiteSpace(create.Body.search_key))
                create.Body.search_key = "";
            Send(create);
        }

        /// <summary>
        /// Change the details of an existing lobby.
        /// </summary>
        /// <param name="details">Lobby details overrides.</param>
        public void SetLobbyDetails(CMsgPracticeLobbySetDetails details)
        {
            var update = new ClientGCMsgProtobuf<CMsgPracticeLobbySetDetails>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbySetDetails);
            // there's no way to pass a pre-allocated body, so copy the params
            PropertyCopier.CopyProperties(update.Body, details);
            Send(update);
        }

        /// <summary>
        ///     Invite someone to the party.
        /// </summary>
        /// <param name="steam_id">Steam ID</param>
        public void InviteToParty(ulong steam_id)
        {
            {
                var invite = new ClientGCMsgProtobuf<CMsgInviteToParty>((uint) EGCBaseMsg.k_EMsgGCInviteToParty);
                invite.Body.steam_id = steam_id;
                Send(invite);
            }
            if (Party != null)
            {
                var invite = new ClientMsgProtobuf<CMsgClientInviteToGame>(EMsg.ClientUDSInviteToGame);
                invite.Body.connect_string = "+invite " + Party.party_id;
                if (Engine == ESourceEngine.k_ESE_Source2) invite.Body.connect_string += " -launchsource2";
                invite.Body.steam_id_dest = steam_id;
                invite.Body.steam_id_src = 0;
                Client.Send(invite);
            }
        }

        /// <summary>
        ///     Invite someone to the existing lobby.
        ///     <remarks>You need an existing lobby for this to work.</remarks>
        /// </summary>
        /// <param name="steam_id">steam ID to invite</param>
        public void InviteToLobby(ulong steam_id)
        {
            {
                var invite = new ClientGCMsgProtobuf<CMsgInviteToLobby>((uint) EGCBaseMsg.k_EMsgGCInviteToLobby);
                invite.Body.steam_id = steam_id;
                Send(invite);
            }
            if (Lobby != null)
            {
                var invite = new ClientMsgProtobuf<CMsgClientInviteToGame>(EMsg.ClientInviteToGame);
                invite.Body.steam_id_dest = steam_id;
                invite.Body.connect_string = "+invite " + Lobby.lobby_id;
                if (Engine == ESourceEngine.k_ESE_Source2) invite.Body.connect_string += " -launchsource2";
                Client.Send(invite);
            }
        }

        /// <summary>
        ///     Sets the team details on the team the bot is sitting on.
        /// </summary>
        /// <param name="teamid"></param>
        public void ApplyTeamToLobby(uint teamid)
        {
            var apply =
                new ClientGCMsgProtobuf<CMsgApplyTeamToPracticeLobby>((uint) EDOTAGCMsg.k_EMsgGCApplyTeamToPracticeLobby);
            apply.Body.team_id = teamid;
            Send(apply);
        }

        /// <summary>
        ///     Set coach slot in party
        /// </summary>
        /// <param name="coach"></param>
        public void SetPartyCoach(bool coach = false)
        {
            var slot =
                new ClientGCMsgProtobuf<CMsgDOTAPartyMemberSetCoach>((uint) EDOTAGCMsg.k_EMsgGCPartyMemberSetCoach);
            slot.Body.wants_coach = coach;
            Send(slot);
        }

        /// <summary>
        ///     Kick a player from the party
        /// </summary>
        /// <param name="steamId">Steam ID of player to kick</param>
        public void KickPlayerFromParty(ulong steamId)
        {
            var kick = new ClientGCMsgProtobuf<CMsgKickFromParty>((uint) EGCBaseMsg.k_EMsgGCKickFromParty);
            kick.Body.steam_id = steamId;
            Send(kick);
        }

        /// <summary>
        ///     Kick a player from the lobby
        /// </summary>
        /// <param name="accountId">Account ID of player to kick</param>
        public void KickPlayerFromLobby(uint accountId)
        {
            var kick = new ClientGCMsgProtobuf<CMsgPracticeLobbyKick>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyKick);
            kick.Body.account_id = accountId;
            Send(kick);
        }

        /// <summary>
        ///     Kick a player from the lobby team they're in.
        /// </summary>
        /// <param name="accountId">Account ID of player to kick</param>
        public void KickPlayerFromLobbyTeam(uint accountId)
        {
            var kick =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyKickFromTeam>(
                    (uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyKickFromTeam);
            kick.Body.account_id = accountId;
            Send(kick);
        }

        /// <summary>
        ///     Joins a chat channel. Note that limited Steam accounts cannot join chat channels.
        /// </summary>
        /// <param name="name">Name of the chat channel</param>
        /// <param name="type">Type of the chat channel</param>
        public void JoinChatChannel(string name,
            DOTAChatChannelType_t type = DOTAChatChannelType_t.DOTAChannelType_Custom)
        {
            var joinChannel = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannel>((uint) EDOTAGCMsg.k_EMsgGCJoinChatChannel);
            joinChannel.Body.channel_name = name;
            joinChannel.Body.channel_type = type;
            Send(joinChannel);
        }

        /// <summary>
        ///     Request a list of public chat channels from the GC.
        /// </summary>
        public void RequestChatChannelList()
        {
            Send(
                new ClientGCMsgProtobuf<CMsgDOTARequestChatChannelList>((uint) EDOTAGCMsg.k_EMsgGCRequestChatChannelList));
        }

        /// <summary>
        ///     Request a match result
        /// </summary>
        /// <param name="matchId">Match id</param>
        public void RequestMatchResult(ulong matchId)
        {
            var requestMatch =
                new ClientGCMsgProtobuf<CMsgGCMatchDetailsRequest>((uint) EDOTAGCMsg.k_EMsgGCMatchDetailsRequest);
            requestMatch.Body.match_id = matchId;

            Send(requestMatch);
        }

        /// <summary>
        ///     Sends a message in a chat channel.
        /// </summary>
        /// <param name="channelid">Id of channel to join.</param>
        /// <param name="message">Message to send.</param>
        public void SendChannelMessage(ulong channelid, string message)
        {
            var chatMsg = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>((uint) EDOTAGCMsg.k_EMsgGCChatMessage);
            chatMsg.Body.channel_id = channelid;
            chatMsg.Body.text = message;
            Send(chatMsg);
        }

        /// <summary>
        ///     Leaves chat channel
        /// </summary>
        /// <param name="channelid">id of channel to leave</param>
        public void LeaveChatChannel(ulong channelid)
        {
            var leaveChannel =
                new ClientGCMsgProtobuf<CMsgDOTALeaveChatChannel>((uint) EDOTAGCMsg.k_EMsgGCLeaveChatChannel);
            leaveChannel.Body.channel_id = channelid;
            Send(leaveChannel);
        }

        /// <summary>
        ///     Requests a lobby list with an optional password
        /// </summary>
        /// <param name="passKey">Pass key.</param>
        /// <param name="tournament"> Tournament games? </param>
        public void PracticeLobbyList(string passKey = null, bool tournament = false)
        {
            var list = new ClientGCMsgProtobuf<CMsgPracticeLobbyList>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyList);
            list.Body.pass_key = passKey;
            list.Body.tournament_games = tournament;
            Send(list);
        }

        /// <summary>
        ///     Shuffle the current lobby
        /// </summary>
        public void PracticeLobbyShuffle()
        {
            var shuffle =
                new ClientGCMsgProtobuf<CMsgBalancedShuffleLobby>((uint) EDOTAGCMsg.k_EMsgGCBalancedShuffleLobby);
            Send(shuffle);
        }

        /// <summary>
        ///     Flip the teams in the current lobby
        /// </summary>
        public void PracticeLobbyFlip()
        {
            var flip = new ClientGCMsgProtobuf<CMsgFlipLobbyTeams>((uint) EDOTAGCMsg.k_EMsgGCFlipLobbyTeams);
            Send(flip);
        }

        /// <summary>
        ///     Request a player's Dota 2 game profile
        /// </summary>
        public void RequestPlayerProfile(SteamID id)
        {
            var request = new ClientGCMsgProtobuf<CMsgDOTAProfileRequest>((uint) EDOTAGCMsg.k_EMsgGCProfileRequest);
            request.Body.account_id = id.AccountID;
            Send(request);
        }

        /// <summary>
        ///     Set someones role in a guild.
        ///     Roles are: 0 (Kick), 1 (Guild Leader), 2 (Guild Officer), 3 (Regular Member)
        /// </summary>
        public void SetAccountGuildRole(uint guildId, uint accountId, uint targetRole)
        {
            var request =
                new ClientGCMsgProtobuf<CMsgDOTAGuildSetAccountRoleRequest>(
                    (uint) EDOTAGCMsg.k_EMsgGCGuildSetAccountRoleRequest);
            request.Body.guild_id = guildId;
            request.Body.target_account_id = accountId;
            request.Body.target_role = targetRole;
            Send(request);
        }

        /// <summary>
        ///     Invites accountId to a guild.
        /// </summary>
        public void InviteToGuild(uint guildId, uint accountId)
        {
            var request =
                new ClientGCMsgProtobuf<CMsgDOTAGuildInviteAccountRequest>(
                    (uint) EDOTAGCMsg.k_EMsgGCGuildInviteAccountRequest);
            request.Body.guild_id = guildId;
            request.Body.target_account_id = accountId;
            Send(request);
        }

        /// <summary>
        ///     Cancels a pending guild invite
        /// </summary>
        public void CancelGuildInvite(uint guildId, uint accountId)
        {
            var request =
                new ClientGCMsgProtobuf<CMsgDOTAGuildCancelInviteRequest>(
                    (uint) EDOTAGCMsg.k_EMsgGCGuildCancelInviteRequest);
            request.Body.guild_id = guildId;
            request.Body.target_account_id = accountId;
            Send(request);
        }

        /// <summary>
        ///     Requests information about all current guilds the client is in
        /// </summary>
        public void RequestGuildData()
        {
            var request = new ClientGCMsgProtobuf<CMsgDOTARequestGuildData>((uint) EDOTAGCMsg.k_EMsgGCRequestGuildData);
            Send(request);
        }

        /// <summary>
        ///     Requests someone's profile cards
        /// </summary>
        public void RequestProfileCards(uint account_id)
        {
            var request =
                new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>((uint) EDOTAGCMsg.k_EMsgClientToGCGetProfileCard);
            request.Body.account_id = account_id;
            Send(request);
        }

        /// <summary>
        ///     Packet GC message.
        /// </summary>
        /// <param name="eMsg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IPacketGCMsg GetPacketGCMsg(uint eMsg, byte[] data)
        {
            // strip off the protobuf flag
            var realEMsg = MsgUtil.GetGCMsg(eMsg);

            if (MsgUtil.IsProtoBuf(eMsg))
            {
                return new PacketClientGCMsgProtobuf(realEMsg, data);
            }
            return new PacketClientGCMsg(realEMsg, data);
        }

        /// <summary>
        ///     Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg.MsgType == EMsg.ClientFromGC)
            {
                var msg = new ClientMsgProtobuf<CMsgGCClient>(packetMsg);
                if (msg.Body.appid == (uint) GameId)
                {
                    var gcmsg = GetPacketGCMsg(msg.Body.msgtype, msg.Body.payload);
                    var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
                    {
                        {(uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, HandleWelcome},
                        {(uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinResponse, HandlePracticeLobbyJoinResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyListResponse, HandlePracticeLobbyListResponse},
                        {(uint) ESOMsg.k_ESOMsg_UpdateMultiple, HandleUpdateMultiple},
                        {(uint) ESOMsg.k_ESOMsg_CacheSubscribed, HandleCacheSubscribed},
                        {(uint) ESOMsg.k_ESOMsg_CacheUnsubscribed, HandleCacheUnsubscribed},
                        {(uint) ESOMsg.k_ESOMsg_Destroy, HandleCacheDestroy},
                        {(uint) EGCBaseClientMsg.k_EMsgGCPingRequest, HandlePingRequest},
                        {(uint) EDOTAGCMsg.k_EMsgGCJoinChatChannelResponse, HandleJoinChatChannelResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCRequestChatChannelListResponse, HandleChatChannelListResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCChatMessage, HandleChatMessage},
                        {(uint) EDOTAGCMsg.k_EMsgGCOtherJoinedChannel, HandleOtherJoinedChannel},
                        {(uint) EDOTAGCMsg.k_EMsgGCOtherLeftChannel, HandleOtherLeftChannel},
                        {(uint) EDOTAGCMsg.k_EMsgGCPopup, HandlePopup},
                        {(uint) EDOTAGCMsg.k_EMsgDOTALiveLeagueGameUpdate, HandleLiveLeageGameUpdate},
                        {(uint) EGCBaseMsg.k_EMsgGCInvitationCreated, HandleInvitationCreated},
                        {(uint) EDOTAGCMsg.k_EMsgGCMatchDetailsResponse, HandleMatchDetailsResponse},
                        {(uint) EGCBaseClientMsg.k_EMsgGCClientConnectionStatus, HandleConnectionStatus},
                        {(uint) EDOTAGCMsg.k_EMsgGCProTeamListResponse, HandleProTeamList},
                        {(uint) EDOTAGCMsg.k_EMsgGCFantasyLeagueInfo, HandleFantasyLeagueInfo},
                        {(uint) EDOTAGCMsg.k_EMsgGCPlayerInfo, HandlePlayerInfo},
                        {(uint) EDOTAGCMsg.k_EMsgGCProfileResponse, HandleProfileResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCGuildSetAccountRoleResponse, HandleGuildAccountRoleResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCGuildInviteAccountResponse, HandleGuildInviteAccountResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCGuildCancelInviteResponse, HandleGuildCancelInviteResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCGuildData, HandleGuildData},
                        {(uint) EDOTAGCMsg.k_EMsgClientToGCGetProfileCardResponse, HandleProfileCardResponse}
                    };
                    Action<IPacketGCMsg> func;
                    if (!messageMap.TryGetValue(gcmsg.MsgType, out func))
                    {
                        Client.PostCallback(new UnhandledDotaGCCallback(gcmsg));
                        return;
                    }

                    func(gcmsg);
                }
            }
            else
            {
                if (packetMsg.IsProto && packetMsg.MsgType == EMsg.ClientUDSInviteToGame)
                {
                    var msg = new ClientMsgProtobuf<CMsgClientInviteToGame>(packetMsg);
                    Invitation = msg.Body;
                    Client.PostCallback(new SteamPartyInvite(Invitation));
                }
                else
                    switch (packetMsg.MsgType)
                    {
                        case EMsg.ClientAuthListAck:
                        {
                            var msg = new ClientMsgProtobuf<CMsgClientAuthListAck>(packetMsg);
                            Client.PostCallback(new AuthListAck(msg.Body));
                        }
                            break;
                        case EMsg.ClientOGSBeginSessionResponse:
                        {
                            var msg = new ClientMsg<MsgClientOGSBeginSessionResponse>(packetMsg);
                            Client.PostCallback(new BeginSessionResponse(msg.Body));
                        }
                            break;
                        case EMsg.ClientRichPresenceInfo:
                        {
                            var msg = new ClientMsgProtobuf<CMsgClientRichPresenceInfo>(packetMsg);
                            Client.PostCallback(new RichPresenceUpdate(msg.Body));
                        }
                            break;
                    }
            }
        }

        private void HandleInvitationCreated(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgInvitationCreated>(obj);
            Client.PostCallback(new InvitationCreated(msg.Body));
        }

        private void HandleCacheSubscribed(IPacketGCMsg obj)
        {
            var sub = new ClientGCMsgProtobuf<CMsgSOCacheSubscribed>(obj);
            foreach (var cache in sub.Body.objects)
            {
                HandleSubscribedType(cache);
            }
        }

        /// <summary>
        ///     Handle various cache subscription types.
        /// </summary>
        /// <param name="cache"></param>
        private void HandleSubscribedType(CMsgSOCacheSubscribed.SubscribedType cache)
        {
            switch ((CSOTypes) cache.type_id)
            {
                case CSOTypes.ECON_ITEM:
                    HandleEconItemsSnapshot(cache.object_data);
                    break;
                case CSOTypes.ECON_GAME_ACCOUNT_CLIENT:
                    HandleGameAccountClientSnapshot(cache.object_data[0]);
                    break;
                case CSOTypes.LEAGUE_VIEW_PASS:
                    HandleLeaguePassesSnapshot(cache.object_data);
                    break;
                case CSOTypes.MAP_LOCATION_STATE:
                    HandleMapLocationsSnapshot(cache.object_data);
                    break;
                case CSOTypes.LOBBY:
                    HandleLobbySnapshot(cache.object_data[0]);
                    break;
                case CSOTypes.PARTY:
                    HandlePartySnapshot(cache.object_data[0]);
                    break;
                case CSOTypes.PARTYINVITE:
                    HandlePartyInviteSnapshot(cache.object_data[0]);
                    break;
                case CSOTypes.LOBBYINVITE:
                    HandleLobbyInviteSnapshot(cache.object_data[0]);
                    break;
            }
        }

        /// <summary>
        ///     Handle when a cache is destroyed.
        /// </summary>
        /// <param name="obj">Message</param>
        public void HandleCacheDestroy(IPacketGCMsg obj)
        {
            var dest = new ClientGCMsgProtobuf<CMsgSOSingleObject>(obj);
            if (PartyInvite != null && dest.Body.type_id == (int) CSOTypes.PARTYINVITE)
            {
                PartyInvite = null;
                Client.PostCallback(new PartyInviteLeave(null));
            }
            else if (Lobby != null && dest.Body.type_id == (int) CSOTypes.LOBBY)
            {
                Lobby = null;
                Client.PostCallback(new PracticeLobbyLeave(null));
            }
            else if (Party != null && dest.Body.type_id == (int) CSOTypes.PARTY)
            {
                Party = null;
                Client.PostCallback(new PartyLeave(null));
            }
            else if (LobbyInvite != null && dest.Body.type_id == (int) CSOTypes.LOBBYINVITE)
            {
                LobbyInvite = null;
                Client.PostCallback(new LobbyInviteLeave(null));
            }
        }

        private void HandleCacheUnsubscribed(IPacketGCMsg obj)
        {
            var unSub = new ClientGCMsgProtobuf<CMsgSOCacheUnsubscribed>(obj);
            if (Lobby != null && unSub.Body.owner_soid.id == Lobby.lobby_id)
            {
                Lobby = null;
                Client.PostCallback(new PracticeLobbyLeave(unSub.Body));
            }
            else if (Party != null && unSub.Body.owner_soid.id == Party.party_id)
            {
                Party = null;
                Client.PostCallback(new PartyLeave(unSub.Body));
            }
            else if (PartyInvite != null && unSub.Body.owner_soid.id == PartyInvite.group_id)
            {
                PartyInvite = null;
                Client.PostCallback(new PartyInviteLeave(unSub.Body));
            }
            else
                Client.PostCallback(new CacheUnsubscribed(unSub.Body));
        }

        private void HandleLeaguePassesSnapshot(IEnumerable<byte[]> items)
        {
            foreach (var bitem in items)
            {
                using (var stream = new MemoryStream(bitem))
                {
                    var item = Serializer.Deserialize<CSOEconItemLeagueViewPass>(stream);
                    LeagueViewPasses[item.league_id] = item;
                }
            }
            Client.PostCallback(new LeagueViewPassesSnapshot(LeagueViewPasses.Values));
        }

        private void HandleMapLocationsSnapshot(IEnumerable<byte[]> items)
        {
            foreach (var bitem in items)
            {
                using (var stream = new MemoryStream(bitem))
                {
                    var item = Serializer.Deserialize<CSODOTAMapLocationState>(stream);
                    MapLocationStates[item.location_id] = item;
                }
            }
        }

        private void HandleEconItemsSnapshot(IEnumerable<byte[]> items)
        {
            foreach (var bitem in items)
            {
                using (var stream = new MemoryStream(bitem))
                {
                    var item = Serializer.Deserialize<CSOEconItem>(stream);
                    EconItems[item.id] = item;
                }
            }
        }

        private void HandleLobbySnapshot(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var lob = Serializer.Deserialize<CSODOTALobby>(stream);
                var oldLob = Lobby;
                Lobby = lob;
                Client.PostCallback(new PracticeLobbySnapshot(lob, oldLob));
            }
            UploadRichPresence();
        }

        private void HandleGameAccountClientSnapshot(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                GameAccountClient = Serializer.Deserialize<CSOEconGameAccountClient>(stream);
                Client.PostCallback(new GameAccountClientSnapshot(GameAccountClient));
            }
        }

        private void HandlePartySnapshot(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var party = Serializer.Deserialize<CSODOTAParty>(stream);
                var oldParty = Party;
                Party = party;
                Client.PostCallback(new PartySnapshot(party, oldParty));
            }
            UploadRichPresence();
        }

        private void HandlePartyInviteSnapshot(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var party = Serializer.Deserialize<CSODOTAPartyInvite>(stream);
                var oldParty = PartyInvite;
                PartyInvite = party;
                Client.PostCallback(new PartyInviteSnapshot(party, oldParty));
            }
        }

        private void HandleLobbyInviteSnapshot(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var lobby = Serializer.Deserialize<CSODOTALobbyInvite>(stream);
                var oldLobby = LobbyInvite;
                LobbyInvite = lobby;
                Client.PostCallback(new LobbyInviteSnapshot(lobby, oldLobby));
            }
        }

        private void HandleFantasyLeagueInfo(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAFantasyLeagueInfo>(obj);
            Client.PostCallback(new FantasyLeagueInfo(resp.Body));
        }

        private void HandlePlayerInfo(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgGCPlayerInfo>(obj);
            Client.PostCallback(new PlayerInfo(resp.Body.player_infos));
        }

        private void HandlePracticeLobbyListResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgPracticeLobbyListResponse>(obj);
            Client.PostCallback(new PracticeLobbyListResponse(resp.Body));
        }

        private void HandlePracticeLobbyJoinResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinResponse>(obj);
            Client.PostCallback(new PracticeLobbyJoinResponse(resp.Body));
        }

        private void HandlePingRequest(IPacketGCMsg obj)
        {
            var req = new ClientGCMsgProtobuf<CMsgGCClientPing>(obj);
            Pong();
            Client.PostCallback(new PingRequest(req.Body));
        }

        private void HandleJoinChatChannelResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannelResponse>(obj);
            Client.PostCallback(new JoinChatChannelResponse(resp.Body));
        }

        private void HandleChatChannelListResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTARequestChatChannelListResponse>(obj);
            Client.PostCallback(new ChatChannelListResponse(resp.Body));
        }

        private void HandleChatMessage(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>(obj);
            Client.PostCallback(new ChatMessage(resp.Body));
        }

        private void HandleMatchDetailsResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgGCMatchDetailsResponse>(obj);
            Client.PostCallback(new MatchResultResponse(resp.Body));
        }

        private void HandleConnectionStatus(IPacketGCMsg obj)
        {
            if (!_running)
            {
                // Stahhp
                Stop();
                return;
            }

            var resp = new ClientGCMsgProtobuf<CMsgConnectionStatus>(obj);
            Client.PostCallback(new ConnectionStatus(resp.Body));

            if (resp.Body.status != GCConnectionStatus.GCConnectionStatus_HAVE_SESSION) _gcConnectTimer.Change(0, 5000);

            Ready = resp.Body.status == GCConnectionStatus.GCConnectionStatus_HAVE_SESSION;
        }

        private void HandleProTeamList(IPacketGCMsg msg)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAProTeamListResponse>(msg);
            Client.PostCallback(new ProTeamListResponse(resp.Body));
        }

        private void HandleOtherJoinedChannel(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAOtherJoinedChatChannel>(obj);
            Client.PostCallback(new OtherJoinedChannel(resp.Body));
        }

        private void HandleOtherLeftChannel(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAOtherLeftChatChannel>(obj);
            Client.PostCallback(new OtherLeftChannel(resp.Body));
        }

        private void HandleUpdateMultiple(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgSOMultipleObjects>(obj);
            var handled = true;
            foreach (var mObj in resp.Body.objects_modified)
            {
                if (mObj.type_id == (int) CSOTypes.LOBBY)
                {
                    HandleLobbySnapshot(mObj.object_data);
                }
                else if (mObj.type_id == (int) CSOTypes.PARTY)
                {
                    HandlePartySnapshot(mObj.object_data);
                }
                else if (mObj.type_id == (int) CSOTypes.PARTYINVITE)
                {
                    HandlePartyInviteSnapshot(mObj.object_data);
                }
                else
                {
                    handled = false;
                }
            }
            if (!handled)
            {
                Client.PostCallback(new UnhandledDotaGCCallback(obj));
            }
        }

        private void HandlePopup(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAPopup>(obj);
            Client.PostCallback(new Popup(resp.Body));
            switch (resp.Body.id)
            {
                case CMsgDOTAPopup.PopupID.KICKED_FROM_LOBBY:
                    Client.PostCallback(new KickedFromLobby(resp.Body));
                    break;
                case CMsgDOTAPopup.PopupID.KICKED_FROM_PARTY:
                    Client.PostCallback(new KickedFromParty(resp.Body));
                    break;
                case CMsgDOTAPopup.PopupID.KICKED_FROM_TEAM:
                    Client.PostCallback(new KickedFromTeam(resp.Body));
                    break;
            }
        }

        /// <summary>
        ///     GC tells us if there are tournaments _running.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleLiveLeageGameUpdate(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTALiveLeagueGameUpdate>(obj);
            Client.PostCallback(new LiveLeagueGameUpdate(resp.Body));
        }

        //Initial message sent when connected to the GC
        private void HandleWelcome(IPacketGCMsg msg)
        {
            _gcConnectTimer.Change(0, Timeout.Infinite);

            Ready = true;

            // Clear these. They will be updated in the subscriptions if they exist still.
            Lobby = null;
            Party = null;
            PartyInvite = null;

            var wel = new ClientGCMsgProtobuf<CMsgClientWelcome>(msg);
            Client.PostCallback(new GCWelcomeCallback(wel.Body));

            //Handle any cache subscriptions
            foreach (var cache in wel.Body.outofdate_subscribed_caches)
                foreach (var obj in cache.objects)
                    HandleSubscribedType(obj);

            UploadRichPresence();
        }

        private void HandleProfileResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAProfileResponse>(obj);
            Client.PostCallback(new ProfileResponse(resp.Body));
        }

        private void HandleGuildAccountRoleResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAGuildSetAccountRoleResponse>(obj);
            Client.PostCallback(new GuildSetRoleResponse(resp.Body));
        }

        private void HandleGuildInviteAccountResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAGuildInviteAccountResponse>(obj);
            Client.PostCallback(new GuildInviteResponse(resp.Body));
        }

        private void HandleGuildCancelInviteResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAGuildCancelInviteResponse>(obj);
            Client.PostCallback(new GuildCancelInviteResponse(resp.Body));
        }

        private void HandleGuildData(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAGuildSDO>(obj);
            Client.PostCallback(new GuildDataResponse(resp.Body));
        }

        private void HandleProfileCardResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAProfileCard>(obj);
            Client.PostCallback(new ProfileCardResponse(resp.Body));
        }
    }
}
