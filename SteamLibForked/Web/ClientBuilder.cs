using System.Net;
using System.Net.Http.Headers;
using AchiesUtilities.Web.Models;
using SteamLib.Authentication;
using SteamLib.Core.Interfaces;

namespace SteamLib.Web;

public static class ClientBuilder
{
    public static HttpClientHandlerPair BuildMobileClient(IWebProxy? proxy, IMobileSessionData? sessionData,
        bool disposeHandler = true)
    {
        sessionData?.EnsureValidated();
        var handler = new HttpClientHandler();
        var client = new HttpClient(handler, disposeHandler);

        client.DefaultRequestHeaders.Accept.ParseAdd(
            "application/json, text/javascript, text/html, application/xml, text/xml, */*");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("okhttp/3.12.12");

        if (proxy != null)
        {
            handler.Proxy = proxy;
        }

        var container = handler.CookieContainer;
        if (sessionData == null)
        {
            container.ClearMobileSessionCookies();
        }
        else
        {
            container.SetSteamMobileCookiesWithMobileToken(sessionData);
        }

        ConfigureCommon(handler, client);
        return new HttpClientHandlerPair(client, handler);
    }


    private static void ConfigureCommon(HttpClientHandler handler, HttpClient client)
    {
        ConfigureCommonClient(client);
        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
    }

    private static void ConfigureCommonClient(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(50);
        client.DefaultRequestHeaders.Referrer = new Uri("https://steamcommunity.com");
        client.DefaultRequestHeaders.Add("Origin", "https://steamcommunity.com");
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
    }
}