//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#pragma warning disable 1591

// Option: light framework (CF/Silverlight) enabled
    
// Generated from: gcsystemmsgs.proto
namespace Dota2.GC.Internal
{
    [global::ProtoBuf.ProtoContract(Name=@"EGCSystemMsg")]
    public enum EGCSystemMsg
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgInvalid")]
      k_EGCMsgInvalid = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMulti")]
      k_EGCMsgMulti = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGenericReply")]
      k_EGCMsgGenericReply = 10,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSystemBase")]
      k_EGCMsgSystemBase = 50,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgAchievementAwarded")]
      k_EGCMsgAchievementAwarded = 51,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgConCommand")]
      k_EGCMsgConCommand = 52,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgStartPlaying")]
      k_EGCMsgStartPlaying = 53,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgStopPlaying")]
      k_EGCMsgStopPlaying = 54,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgStartGameserver")]
      k_EGCMsgStartGameserver = 55,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgStopGameserver")]
      k_EGCMsgStopGameserver = 56,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgWGRequest")]
      k_EGCMsgWGRequest = 57,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgWGResponse")]
      k_EGCMsgWGResponse = 58,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetUserGameStatsSchema")]
      k_EGCMsgGetUserGameStatsSchema = 59,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetUserGameStatsSchemaResponse")]
      k_EGCMsgGetUserGameStatsSchemaResponse = 60,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetUserStatsDEPRECATED")]
      k_EGCMsgGetUserStatsDEPRECATED = 61,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetUserStatsResponse")]
      k_EGCMsgGetUserStatsResponse = 62,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgAppInfoUpdated")]
      k_EGCMsgAppInfoUpdated = 63,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgValidateSession")]
      k_EGCMsgValidateSession = 64,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgValidateSessionResponse")]
      k_EGCMsgValidateSessionResponse = 65,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgLookupAccountFromInput")]
      k_EGCMsgLookupAccountFromInput = 66,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSendHTTPRequest")]
      k_EGCMsgSendHTTPRequest = 67,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSendHTTPRequestResponse")]
      k_EGCMsgSendHTTPRequestResponse = 68,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgPreTestSetup")]
      k_EGCMsgPreTestSetup = 69,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgRecordSupportAction")]
      k_EGCMsgRecordSupportAction = 70,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetAccountDetails_DEPRECATED")]
      k_EGCMsgGetAccountDetails_DEPRECATED = 71,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgReceiveInterAppMessage")]
      k_EGCMsgReceiveInterAppMessage = 73,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgFindAccounts")]
      k_EGCMsgFindAccounts = 74,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgPostAlert")]
      k_EGCMsgPostAlert = 75,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetLicenses")]
      k_EGCMsgGetLicenses = 76,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetUserStats")]
      k_EGCMsgGetUserStats = 77,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetCommands")]
      k_EGCMsgGetCommands = 78,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetCommandsResponse")]
      k_EGCMsgGetCommandsResponse = 79,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgAddFreeLicense")]
      k_EGCMsgAddFreeLicense = 80,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgAddFreeLicenseResponse")]
      k_EGCMsgAddFreeLicenseResponse = 81,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetIPLocation")]
      k_EGCMsgGetIPLocation = 82,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetIPLocationResponse")]
      k_EGCMsgGetIPLocationResponse = 83,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSystemStatsSchema")]
      k_EGCMsgSystemStatsSchema = 84,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetSystemStats")]
      k_EGCMsgGetSystemStats = 85,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetSystemStatsResponse")]
      k_EGCMsgGetSystemStatsResponse = 86,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSendEmail")]
      k_EGCMsgSendEmail = 87,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSendEmailResponse")]
      k_EGCMsgSendEmailResponse = 88,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetEmailTemplate")]
      k_EGCMsgGetEmailTemplate = 89,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetEmailTemplateResponse")]
      k_EGCMsgGetEmailTemplateResponse = 90,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGrantGuestPass")]
      k_EGCMsgGrantGuestPass = 91,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGrantGuestPassResponse")]
      k_EGCMsgGrantGuestPassResponse = 92,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetAccountDetails")]
      k_EGCMsgGetAccountDetails = 93,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetAccountDetailsResponse")]
      k_EGCMsgGetAccountDetailsResponse = 94,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetPersonaNames")]
      k_EGCMsgGetPersonaNames = 95,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetPersonaNamesResponse")]
      k_EGCMsgGetPersonaNamesResponse = 96,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMultiplexMsg")]
      k_EGCMsgMultiplexMsg = 97,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgWebAPIRegisterInterfaces")]
      k_EGCMsgWebAPIRegisterInterfaces = 101,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgWebAPIJobRequest")]
      k_EGCMsgWebAPIJobRequest = 102,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgWebAPIJobRequestHttpResponse")]
      k_EGCMsgWebAPIJobRequestHttpResponse = 104,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgWebAPIJobRequestForwardResponse")]
      k_EGCMsgWebAPIJobRequestForwardResponse = 105,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMemCachedGet")]
      k_EGCMsgMemCachedGet = 200,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMemCachedGetResponse")]
      k_EGCMsgMemCachedGetResponse = 201,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMemCachedSet")]
      k_EGCMsgMemCachedSet = 202,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMemCachedDelete")]
      k_EGCMsgMemCachedDelete = 203,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMemCachedStats")]
      k_EGCMsgMemCachedStats = 204,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMemCachedStatsResponse")]
      k_EGCMsgMemCachedStatsResponse = 205,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSQLStats")]
      k_EGCMsgSQLStats = 210,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSQLStatsResponse")]
      k_EGCMsgSQLStatsResponse = 211,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMasterSetDirectory")]
      k_EGCMsgMasterSetDirectory = 220,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMasterSetDirectoryResponse")]
      k_EGCMsgMasterSetDirectoryResponse = 221,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMasterSetWebAPIRouting")]
      k_EGCMsgMasterSetWebAPIRouting = 222,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMasterSetWebAPIRoutingResponse")]
      k_EGCMsgMasterSetWebAPIRoutingResponse = 223,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMasterSetClientMsgRouting")]
      k_EGCMsgMasterSetClientMsgRouting = 224,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgMasterSetClientMsgRoutingResponse")]
      k_EGCMsgMasterSetClientMsgRoutingResponse = 225,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSetOptions")]
      k_EGCMsgSetOptions = 226,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSetOptionsResponse")]
      k_EGCMsgSetOptionsResponse = 227,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgSystemBase2")]
      k_EGCMsgSystemBase2 = 500,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetPurchaseTrustStatus")]
      k_EGCMsgGetPurchaseTrustStatus = 501,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetPurchaseTrustStatusResponse")]
      k_EGCMsgGetPurchaseTrustStatusResponse = 502,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgUpdateSession")]
      k_EGCMsgUpdateSession = 503,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGCAccountVacStatusChange")]
      k_EGCMsgGCAccountVacStatusChange = 504,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgCheckFriendship")]
      k_EGCMsgCheckFriendship = 505,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgCheckFriendshipResponse")]
      k_EGCMsgCheckFriendshipResponse = 506,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetPartnerAccountLink")]
      k_EGCMsgGetPartnerAccountLink = 507,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetPartnerAccountLinkResponse")]
      k_EGCMsgGetPartnerAccountLinkResponse = 508,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgVSReportedSuspiciousActivity")]
      k_EGCMsgVSReportedSuspiciousActivity = 509,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgDPPartnerMicroTxns")]
      k_EGCMsgDPPartnerMicroTxns = 512,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgDPPartnerMicroTxnsResponse")]
      k_EGCMsgDPPartnerMicroTxnsResponse = 513,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetIPASN")]
      k_EGCMsgGetIPASN = 514,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetIPASNResponse")]
      k_EGCMsgGetIPASNResponse = 515,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetAppFriendsList")]
      k_EGCMsgGetAppFriendsList = 516,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgGetAppFriendsListResponse")]
      k_EGCMsgGetAppFriendsListResponse = 517,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgVacVerificationChange")]
      k_EGCMsgVacVerificationChange = 518,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCMsgAccountPhoneNumberChange")]
      k_EGCMsgAccountPhoneNumberChange = 519
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"ESOMsg")]
    public enum ESOMsg
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_Create")]
      k_ESOMsg_Create = 21,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_Update")]
      k_ESOMsg_Update = 22,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_Destroy")]
      k_ESOMsg_Destroy = 23,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_CacheSubscribed")]
      k_ESOMsg_CacheSubscribed = 24,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_CacheUnsubscribed")]
      k_ESOMsg_CacheUnsubscribed = 25,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_UpdateMultiple")]
      k_ESOMsg_UpdateMultiple = 26,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_CacheSubscriptionRefresh")]
      k_ESOMsg_CacheSubscriptionRefresh = 28,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_ESOMsg_CacheSubscribedUpToDate")]
      k_ESOMsg_CacheSubscribedUpToDate = 29
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"EGCBaseClientMsg")]
    public enum EGCBaseClientMsg
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCPingRequest")]
      k_EMsgGCPingRequest = 3001,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCPingResponse")]
      k_EMsgGCPingResponse = 3002,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToClientPollConvarRequest")]
      k_EMsgGCToClientPollConvarRequest = 3003,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToClientPollConvarResponse")]
      k_EMsgGCToClientPollConvarResponse = 3004,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCClientWelcome")]
      k_EMsgGCClientWelcome = 4004,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCServerWelcome")]
      k_EMsgGCServerWelcome = 4005,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCClientHello")]
      k_EMsgGCClientHello = 4006,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCServerHello")]
      k_EMsgGCServerHello = 4007,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCClientConnectionStatus")]
      k_EMsgGCClientConnectionStatus = 4009,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCServerConnectionStatus")]
      k_EMsgGCServerConnectionStatus = 4010
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"EGCToGCMsg")]
    public enum EGCToGCMsg
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCToGCMsgMasterAck")]
      k_EGCToGCMsgMasterAck = 150,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCToGCMsgMasterAckResponse")]
      k_EGCToGCMsgMasterAckResponse = 151,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCToGCMsgRouted")]
      k_EGCToGCMsgRouted = 152,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCToGCMsgRoutedReply")]
      k_EGCToGCMsgRoutedReply = 153,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCUpdateSubGCSessionInfo")]
      k_EMsgGCUpdateSubGCSessionInfo = 154,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCRequestSubGCSessionInfo")]
      k_EMsgGCRequestSubGCSessionInfo = 155,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCRequestSubGCSessionInfoResponse")]
      k_EMsgGCRequestSubGCSessionInfoResponse = 156,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EGCToGCMsgMasterStartupComplete")]
      k_EGCToGCMsgMasterStartupComplete = 157,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCSOCacheSubscribe")]
      k_EMsgGCToGCSOCacheSubscribe = 158,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCSOCacheUnsubscribe")]
      k_EMsgGCToGCSOCacheUnsubscribe = 159,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCLoadSessionSOCache")]
      k_EMsgGCToGCLoadSessionSOCache = 160,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCLoadSessionSOCacheResponse")]
      k_EMsgGCToGCLoadSessionSOCacheResponse = 161,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCUpdateSessionStats")]
      k_EMsgGCToGCUpdateSessionStats = 162,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCUniverseStartup")]
      k_EMsgGCToGCUniverseStartup = 163,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCUniverseStartupResponse")]
      k_EMsgGCToGCUniverseStartupResponse = 164,
            
      [global::ProtoBuf.ProtoEnum(Name=@"k_EMsgGCToGCForwardAccountDetails")]
      k_EMsgGCToGCForwardAccountDetails = 165
    }
  
}
#pragma warning restore 1591
