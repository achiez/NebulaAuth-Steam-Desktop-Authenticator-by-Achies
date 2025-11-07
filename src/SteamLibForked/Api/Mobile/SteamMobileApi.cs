using System.Net;
using Newtonsoft.Json.Linq;
using SteamLib.Core;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Authorization;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Services;

namespace SteamLib.Api.Mobile;

public static class SteamMobileApi
{
    /// <summary>
    /// </summary>
    /// <param name="client"></param>
    /// <param name="refreshToken"></param>
    /// <param name="steamId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Refreshed AccessToken</returns>
    /// <exception cref="SessionInvalidException"></exception>
    public static async Task<string> RefreshJwt(HttpClient client, string refreshToken, SteamId steamId,
        CancellationToken cancellationToken = default)
    {
        var req = new GenerateAccessTokenForApp_Request
        {
            RefreshToken = refreshToken,
            SteamId = steamId.Steam64,
            TokenRenewalType = true
        };

        try
        {
            var resp = await client.PostProto<GenerateAccessTokenForApp_Response>(Routes.GENERATE_ACCESS_TOKEN, req,
                cancellationToken);
            return resp.AccessToken;
        }
        catch (SteamStatusCodeException ex)
            when (ex.StatusCode == SteamStatusCode.AccessDenied)
        {
            throw new SessionPermanentlyExpiredException(
                "RefreshToken is not accepted by Steam. You must login again and use new token");
        }
    }

    public static async Task<bool> HasPhoneAttached(HttpClient client, string sessionId,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, string>
        {
            {"op", "has_phone"},
            {"arg", "null"},
            {"sessionid", sessionId}
        };
        var content = new FormUrlEncodedContent(data);
        var resp = await client.PostAsync(Routes.PHONE_AJAX, content,
            cancellationToken);
        var respContent = await resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken);

        return SteamLibErrorMonitor.HandleResponse(respContent, () =>
        {
            var json = JObject.Parse(respContent);
            return json["has_phone"]!.Value<bool>();
        });
    }


    public static async Task<RemoveAuthenticator_Response> RemoveAuthenticator(HttpClient client, string accessToken,
        string rCode, CancellationToken cancellationToken = default)
    {
        var req = Routes.REMOVE_AUTHENTICATOR + $"?access_token={accessToken}";
        var reqData = new RemoveAuthenticator_Request
        {
            RevocationCode = rCode,
            RevocationReason = 1,
            SteamGuardScheme = 1
        };
        try
        {
            return await client.PostProto<RemoveAuthenticator_Response>(req, reqData, cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode is HttpStatusCode.Unauthorized)
        {
            throw new SessionInvalidException(SessionInvalidException.GOT_401_MSG);
        }
    }

    public static class Routes
    {
        public const string GENERATE_ACCESS_TOKEN =
            SteamConstants.STEAM_API + "/IAuthenticationService/GenerateAccessTokenForApp/v1";

        public const string PHONE_AJAX = SteamConstants.STEAM_COMMUNITY + "/steamguard/phoneajax";

        public const string REMOVE_AUTHENTICATOR =
            SteamConstants.STEAM_API + "/ITwoFactorService/RemoveAuthenticator/v1";
    }
}