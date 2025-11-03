using SteamLib.Core;
using SteamLib.Models;
using SteamLib.Web.Scrappers.HTML;
using System.Net;
using System.Web;

namespace SteamLib.Api;

public static class SteamGlobalApi
{
    //FIXME: refactor
    public static async Task<ScriptHeaderModel> GetSessionIdFromLoginPage(HttpClient client,
        CancellationToken cancellationToken = default)
    {
        var resp = await client.GetStringAsync("https://steamcommunity.com/login/home", cancellationToken);
        return SteamLibErrorMonitor.HandleResponse(resp, ScriptHeaderScrapper.Scrap);
    }

    public static async Task<ScriptHeaderModel> GetSessionIdFromPage(HttpClient client, string url,
        CancellationToken cancellationToken = default)
    {
        var resp = await client.GetStringAsync(url, cancellationToken);
        return SteamLibErrorMonitor.HandleResponse(resp, ScriptHeaderScrapper.Scrap);
    }


    [Obsolete(
        "Not recommended. Creates HttpClientHandler's copy with AllowAutoRedirect = false. Rapid usage of this method can exhaust socket pool")]
    public static async Task<string> EligibilityCheck(HttpClientHandler handler,
        CancellationToken cancellationToken = default)
    {
        var oneTimeHandler = new HttpClientHandler
        {
            CookieContainer = handler.CookieContainer,
            AllowAutoRedirect = false,
            AutomaticDecompression = handler.AutomaticDecompression
        };
        var client = new HttpClient(oneTimeHandler, true);
        var req = new HttpRequestMessage(HttpMethod.Get, SteamConstants.STEAM_COMMUNITY + "/market/eligibilitycheck/");
        var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var setCookieHeaders =
            resp.Headers.FirstOrDefault(h => h.Key == "Set-Cookie").Value;

        if (setCookieHeaders == null)
        {
            throw new NullReferenceException("Can't find 'Set-Cookie' header in EligibilityCheck response");
        }

        var wedTradeEligibility = setCookieHeaders.FirstOrDefault(s => s.StartsWith("webTradeEligibility"));
        if (wedTradeEligibility == null)
        {
            throw new NullReferenceException(
                "Can't find wedTradeEligibility header in JwtRefresh (set-token) response");
        }

        var eligibilityStr = wedTradeEligibility
            .ToCharArray()
            .SkipWhile(ch => !ch.Equals('='))
            .TakeWhile(ch => !ch.Equals(';'))
            .ToArray();

        var result = new string(eligibilityStr[1..]);
        client.Dispose();
        return HttpUtility.UrlDecode(result);
    }

    [Obsolete(
        "Not recommended. Creates one-time usage HttpClientHandler with AllowAutoRedirect = false. Rapid usage of this method can exhaust socket pool")]
    public static Task<string> EligibilityCheck(CookieContainer container,
        CancellationToken cancellationToken = default)
    {
        var oneTimeHandler = new HttpClientHandler
        {
            CookieContainer = container,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        };
        return EligibilityCheck(oneTimeHandler, cancellationToken);
    }

    public static Task SetEligibilityCheckCookies(HttpClient client, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, SteamConstants.STEAM_COMMUNITY + "/market/eligibilitycheck/");
        return client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}