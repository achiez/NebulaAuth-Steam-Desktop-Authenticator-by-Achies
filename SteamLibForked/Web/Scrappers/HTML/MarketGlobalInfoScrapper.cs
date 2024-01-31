using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using SteamLib.Web.Models.GlobalMarketInfo;

namespace SteamLib.Web.Scrappers.HTML;

public static class MarketGlobalInfoScrapper
{
    private static readonly Regex LoggedInRegex = new("var g_bLoggedIn = (.+);", RegexOptions.Compiled);
    private static readonly Regex ReqBillingInfoRegex = new("var g_bRequiresBillingInfo = (.+);", RegexOptions.Compiled);
    private static readonly Regex CountryCodeRegex = new("var g_strCountryCode = \"(.+)\";", RegexOptions.Compiled);
    private static readonly Regex HasBillingStates = new("var g_bHasBillingStates = (.+);", RegexOptions.Compiled);
    private static readonly Regex LanguageRegex = new("var g_strLanguage = \"(.+)\";", RegexOptions.Compiled);
    private static readonly Regex WalletInfoRegex = new("var g_rgWalletInfo = (.+);", RegexOptions.Compiled);

    private static readonly Regex SessionIdRegex = new("g_sessionID = \"(.+)\";", RegexOptions.Compiled);
    private static readonly Regex SteamIdRegex = new("g_steamID = \"(.+)\";", RegexOptions.Compiled);

    public static GlobalInfoModel Scrap(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var scriptNodes = document.DocumentNode.SelectNodes("//*[@id=\"responsive_page_template_content\"]/script");
        var index = scriptNodes.Count > 2 ? 1 : 0; //If account is limited or market unavailable elements will be displaced

        var scriptNode = scriptNodes[index];
        var script = scriptNode.InnerText!;

        var logged = GetBool(script, LoggedInRegex);
        var hasBillingStates = GetBool(script, HasBillingStates);
        var reqBillingInfo = GetBool(script, ReqBillingInfoRegex);

        var country = CountryCodeRegex.Match(html).Groups[1].Value;
        var language = LanguageRegex.Match(html).Groups[1].Value;

        
        var walletInfoStr = WalletInfoRegex.Match(html).Groups[1].Value;
        MarketWalletSchema? wallet = null;

        var sessionScriptNode = document.DocumentNode.SelectSingleNode($"//div[@class='responsive_page_content']/script");
        var sessionScript = sessionScriptNode.InnerText!;

        var sessionId = SessionIdRegex.Match(sessionScript).Groups[1].Value;
        long? steamId = null;



        if (logged)
        {
            wallet = JsonConvert.DeserializeObject<MarketWalletSchema>(walletInfoStr);
            var steamIdStr = SteamIdRegex.Match(sessionScript).Groups[1].Value;
            steamId = long.Parse(new string(steamIdStr.Where(char.IsDigit).ToArray()));
        }

       

        return new GlobalInfoModel
        {
            CountryCode = country,
            HasBillingStates = hasBillingStates,
            IsLoggedIn = logged,
            Language = language,
            RequiresBillingInfo = reqBillingInfo,
            WalletInfo = wallet,
            SessionId = sessionId,
            SteamId = steamId
        };

        

    }
    private static bool GetBool(string script, Regex regex)
    {
        var value = regex.Match(script).Groups[1].Value;
        return bool.Parse(value);
    }

}