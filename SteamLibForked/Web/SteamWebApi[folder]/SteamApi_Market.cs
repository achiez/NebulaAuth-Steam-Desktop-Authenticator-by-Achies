using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using SteamLib.Core;
using SteamLib.Exceptions.General;
using SteamLib.Web.Models.GlobalMarketInfo;
using SteamLib.Web.Scrappers.HTML;
using System.Threading;
using JetBrains.Annotations;
using HtmlAgilityPack;
using SteamLib.Core.Models;

namespace SteamLib.Web;

public static class SteamWebApi
{
    /// <summary>
    ///     Login is not required
    /// </summary>
    /// <param name="client"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<GlobalInfoModel> GetMarketGlobalInfo(HttpClient client,
        CancellationToken cancellationToken = default)
    {
        var resp = await client.GetStringAsync(SteamConstants.STEAM_MARKET, cancellationToken);
        try
        {
            return MarketGlobalInfoScrapper.Scrap(resp);
        }
        catch (Exception ex)
        {
            throw new UnsupportedResponseException(resp, ex);
        }
    }


    [RegexPattern]
    [SuppressMessage("ReSharper", "UseRawString")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static readonly string _regexTip =
        @"g_sessionID = ""(?<g_sessionID>.*)"";"
        + @"\s*g_steamID = (?<g_steamID>.*);";

    private static readonly Regex Regex = new(_regexTip, RegexOptions.Compiled);
    private const string XPATH = "//div[@class='responsive_page_content']/script";

    public static async Task<string> GetLoginSessionId(HttpClient client, CancellationToken cancellationToken = default)
    {
        var resp = await client.GetStringAsync("https://steamcommunity.com/login/home", cancellationToken);

        var document = new HtmlDocument();
        document.LoadHtml(resp);
        var scriptNode = document.DocumentNode.SelectSingleNode(XPATH) ?? throw new NullReferenceException("Script Node was null");
        var script = scriptNode.InnerText;
        var match = Regex.Match(script);
        if (!match.Success)
            throw new UnsupportedResponseException(script, "Page contains script but regex was unsuccessful");
        return match.Groups["g_sessionID"].Value;
    }
}