using HtmlAgilityPack;
using JetBrains.Annotations;
using SteamLib.Exceptions.General;
using SteamLib.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SteamLib.Web.Scrappers.HTML;

//TODO: Use this as universal <script> data parser, every html page contains this globals
public static class ScriptHeaderScrapper
{
    private const string XPATH = "//div[@class='responsive_page_content']/script";

    [RegexPattern]
    [SuppressMessage("ReSharper", "UseRawString")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static readonly string _regexTip =
        @"g_sessionID = ""(?<g_sessionID>.*)"";"
        + @"\s*g_steamID = (?<g_steamID>.*);";

    private static readonly Regex Regex = new(_regexTip, RegexOptions.Compiled);


    public static ScriptHeaderModel Scrap(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return Scrap(document);
    }

    public static ScriptHeaderModel Scrap(HtmlDocument document)
    {
        var scriptNode = document.DocumentNode.SelectSingleNode(XPATH) ??
                         throw new NullReferenceException("Script Node was null");
        var script = scriptNode.InnerText;
        return ConvertScript(script);
    }

    public static ScriptHeaderModel ConvertScript(string script)
    {
        var match = Regex.Match(script);
        if (!match.Success)
            throw new UnsupportedResponseException(script, "Page contains script but regex was unsuccessful");

        var steamIdStr = match.Groups["g_steamID"].Value;
        var sessionId = match.Groups["g_sessionID"].Value;
        SteamId? steamId;
        if (steamIdStr == "false") // notLoggedIn
        {
            steamId = null;
        }
        else
        {
            steamId = SteamId.Parse(steamIdStr);
        }

        return new ScriptHeaderModel
        {
            SteamId = steamId,
            SessionId = sessionId
        };
    }
}