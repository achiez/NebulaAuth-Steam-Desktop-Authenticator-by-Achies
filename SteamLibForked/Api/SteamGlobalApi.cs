using AchiesUtilities.Models;
using AchiesUtilities.Newtonsoft.JSON.Converters.Special;
using Newtonsoft.Json;
using SteamLib.Authentication;
using SteamLib.Core;
using SteamLib.Core.Enums;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions;
using SteamLib.Exceptions.General;

namespace SteamLib.Api;

public static class SteamGlobalApi
{
    /// <summary>
    /// Raw AccessToken value
    /// </summary>
    /// <param name="client"></param>
    /// <param name="domain"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="SessionInvalidException"></exception>
    public static async Task<string> RefreshJwt(HttpClient client, SteamDomain domain, CancellationToken cancellationToken = default)
    {
        var domainUri = SteamDomains.GetDomain(domain);
        var data = new Dictionary<string, string>
        {
            {"redir", domainUri}
        };
        var cont = new FormUrlEncodedContent(data);
        var resp = await client.PostAsync("https://login.steampowered.com/jwt/ajaxrefresh", cont, cancellationToken);
        var respStr = await resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken);

        JwtRefreshJson? jwtRefresh;
        try
        {
            jwtRefresh = JsonConvert.DeserializeObject<JwtRefreshJson>(respStr);
        }
        catch (Exception ex)
        {
            throw new UnsupportedResponseException(respStr, ex);
        }

        if (jwtRefresh?.Success != true)
        {
            Exception? inner = null;
            if (jwtRefresh?.Error != null)
            {
                var errorCode = jwtRefresh.Error.Value;
                var translated = SteamStatusCode.Translate<SteamStatusCode>(errorCode, out _);
                inner = new SteamStatusCodeException(translated, respStr);
            }
            throw new SessionInvalidException("AjaxRefresh was not successful. 'steamRefresh_steam' cookie is missing or refresh token is invalid", inner);

        }

        data = new Dictionary<string, string>()
        {
            {"steamID", jwtRefresh.SteamId.ToString()},
            {"nonce", jwtRefresh.Nonce},
            {"redir", jwtRefresh.Redir},
            {"auth", jwtRefresh.Auth},
        };

        cont = new FormUrlEncodedContent(data);
        var update = await client.PostAsync(jwtRefresh.LoginUrl, cont, cancellationToken);
        var updateResp = await update.Content.ReadAsStringAsync(cancellationToken);
        JwtUpdateJson result;
        try
        {
            result = JsonConvert.DeserializeObject<JwtUpdateJson>(updateResp) ?? throw new NullReferenceException();
        }
        catch (Exception ex)
        {
            throw new UnsupportedResponseException(updateResp, ex);
        }

        var resultStatus = SteamStatusCode.Translate<SteamStatusCode>(result.Result, out _);
        if (resultStatus.Equals(SteamStatusCode.Ok) == false)
        {
            throw new SessionInvalidException(
                "AjaxRefresh (set-token) response was not successful. Response string stored in Exception.Data")
            {
                Data = { { "Response", updateResp } }
            };
        }

        return SteamTokenHelper.ExtractJwtFromSetCookiesHeader(update.Headers);
    }


    private class JwtRefreshJson
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("login_url")] public string LoginUrl { get; set; } = string.Empty;
        [JsonProperty("steamID")] public long SteamId { get; set; }
        [JsonProperty("nonce")] public string Nonce { get; set; } = string.Empty;
        [JsonProperty("redir")] public string Redir { get; set; } = string.Empty;
        [JsonProperty("auth")] public string Auth { get; set; } = string.Empty;
        [JsonProperty("error")] public int? Error { get; set; }
    }

    private class JwtUpdateJson
    {
        [JsonProperty("result")]
        public int Result { get; set; }

        [JsonProperty("rtExpiry")]
        [JsonConverter(typeof(UnixTimeStampConverter))]
        public UnixTimeStamp RtExpiry { get; set; }
    }
}

