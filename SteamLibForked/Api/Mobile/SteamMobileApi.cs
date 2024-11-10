using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SteamLib.Core;
using SteamLib.Exceptions;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Exceptions;
using SteamLib.ProtoCore.Services;
using System.Net;
using SteamLib.Core.Models;

namespace SteamLib.Api.Mobile;


[PublicAPI]
public static class SteamMobileApi
{

    private const string GENERATE_ACCESS_TOKEN =
        SteamConstants.STEAM_API + "IAuthenticationService/GenerateAccessTokenForApp/v1";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="refreshToken"></param>
    /// <param name="steamId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Refreshed AccessToken</returns>
    /// <exception cref="SessionInvalidException"></exception>
    public static async Task<string> RefreshJwt(HttpClient client, string refreshToken, SteamId steamId, CancellationToken cancellationToken = default)
    {
        var req = new GenerateAccessTokenForApp_Request
        {
            RefreshToken = refreshToken,
            SteamId = steamId.Steam64,
            TokenRenewalType = true
        };

        try
        {
            var resp = await client.PostProto<GenerateAccessTokenForApp_Response>(GENERATE_ACCESS_TOKEN, req, cancellationToken: cancellationToken);
            return resp.AccessToken;
        }
        catch (EResultException ex)
            when (ex.Result == EResult.AccessDenied)
        {
            throw new SessionPermanentlyExpiredException("RefreshToken is not accepted by Steam. You must login again and use new token");
        }
    }

    public static async Task<bool> HasPhoneAttached(HttpClient client, string sessionId, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, string>
        {
            {"op", "has_phone"},
            {"arg", "null"},
            {"sessionid", sessionId}

        };
        var content = new FormUrlEncodedContent(data);
        var resp = await client.PostAsync(SteamConstants.STEAM_COMMUNITY + "steamguard/phoneajax", content, cancellationToken);
        var respContent = await resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken);

        return SteamLibErrorMonitor.HandleResponse(respContent, () =>
        {
            var json = JObject.Parse(respContent);
            return json["has_phone"]!.Value<bool>();
        });
    }


    public static async Task<RemoveAuthenticator_Response> RemoveAuthenticator(HttpClient client, string accessToken, string rCode, CancellationToken cancellationToken = default)
    {
        var req = SteamConstants.STEAM_API + "ITwoFactorService/RemoveAuthenticator/v1?access_token=" + accessToken;
        var reqData = new RemoveAuthenticator_Request
        {
            RevocationCode = rCode,
            RevocationReason = 1,
            SteamGuardScheme = 1
        };
        try
        {
            return await client.PostProto<RemoveAuthenticator_Response>(req, reqData, cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode is HttpStatusCode.Unauthorized)
        {
            throw new SessionInvalidException(SessionInvalidException.GOT_401_MSG);
        }
    }



}