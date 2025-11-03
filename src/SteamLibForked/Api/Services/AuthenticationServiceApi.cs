using SteamLib.Core;
using SteamLib.Exceptions.Authorization;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Services;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SteamLib.Api.Services;

public static class AuthenticationServiceApi
{
    // ReSharper disable InconsistentNaming
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public static class Routes
    {
        public const string SERVICE = "IAuthenticationService";
        private const string BASE = SteamConstants.STEAM_API + "/" + SERVICE;

        public const string GetAuthSessionsForAccount = BASE + "/GetAuthSessionsForAccount/v1";
        public const string GetAuthSessionInfo = BASE + "/GetAuthSessionInfo/v1";

        public const string UpdateAuthSessionWithMobileConfirmation =
            BASE + "/UpdateAuthSessionWithMobileConfirmation/v1";

        public const string GetPasswordRSAPublicKey = BASE + "/GetPasswordRSAPublicKey/v1";
        public const string BeginAuthSessionWithCredentials = BASE + "/BeginAuthSessionViaCredentials/v1";
        public const string PollAuthSessionStatus = BASE + "/PollAuthSessionStatus/v1";
        public const string UpdateAuthSessionWithSteamGuardCode = BASE + "/UpdateAuthSessionWithSteamGuardCode/v1";
    }


    /// <summary>
    ///     Auth not required
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<GetPasswordRSAPublicKey_Response> GetPasswordRSAPublicKey(HttpClient client,
        GetPasswordRSAPublicKey_Request request, CancellationToken cancellationToken = default)
    {
        return client.GetProto<GetPasswordRSAPublicKey_Response>(Routes.GetPasswordRSAPublicKey, request,
            cancellationToken);
    }

    public static Task<BeginAuthSessionViaCredentials_Response> BeginAuthSessionViaCredentials(HttpClient client,
        BeginAuthSessionViaCredentials_Request request, CancellationToken cancellationToken = default)
    {
        return client.PostProto<BeginAuthSessionViaCredentials_Response>(Routes.BeginAuthSessionWithCredentials,
            request, cancellationToken);
    }

    public static Task<PollAuthSessionStatus_Response> PollAuthSessionStatus(HttpClient client,
        PollAuthSessionStatus_Request request, CancellationToken cancellationToken = default)
    {
        return client.PostProto<PollAuthSessionStatus_Response>(Routes.PollAuthSessionStatus, request,
            cancellationToken);
    }


    public static async Task<GetAuthSessionsForAccount_Response> GetAuthSessionsForAccount(HttpClient client,
        string accessToken, CancellationToken cancellationToken = default)
    {
        var req = Routes.GetAuthSessionsForAccount.AddAccessTokenQuery(accessToken);
        try
        {
            return await client.GetProto<GetAuthSessionsForAccount_Response>(req, EmptyMessage.Instance,
                cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SessionInvalidException(SessionInvalidException.GOT_401_MSG, ex);
        }
    }

    public static async Task<GetAuthSessionInfo_Response> GetAuthSessionInfo(HttpClient client, string accessToken,
        ulong clientId, CancellationToken cancellationToken = default)
    {
        var req = Routes.GetAuthSessionInfo.AddAccessTokenQuery(accessToken);
        var reqData = new GetAuthSessionInfo_Request
        {
            ClientId = clientId
        };
        try
        {
            return await client.PostProto<GetAuthSessionInfo_Response>(req, reqData, cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SessionInvalidException(SessionInvalidException.GOT_401_MSG, ex);
        }
    }

    public static Task UpdateAuthSessionWithSteamGuardCode(HttpClient client,
        UpdateAuthSessionWithSteamGuardCode_Request request, CancellationToken cancellationToken = default)
    {
        return client.PostProtoEnsureSuccess(Routes.UpdateAuthSessionWithSteamGuardCode, request, cancellationToken);
    }

    public static Task UpdateAuthSessionWithMobileConfirmation(HttpClient client, string accessToken,
        UpdateAuthSessionWithMobileConfirmation_Request request, CancellationToken cancellationToken = default)
    {
        var req = Routes.UpdateAuthSessionWithMobileConfirmation.AddAccessTokenQuery(accessToken);
        return client.PostProtoEnsureSuccess(req, request, cancellationToken);
    }

    private static string AddAccessTokenQuery(this string route, string accessToken)
    {
        return route + "?access_token=" + accessToken;
    }
}