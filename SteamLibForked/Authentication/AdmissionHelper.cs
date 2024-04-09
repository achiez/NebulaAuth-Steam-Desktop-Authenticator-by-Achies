using AchiesUtilities.Extensions;
using JetBrains.Annotations;
using SteamLib.Account;
using SteamLib.Core;
using SteamLib.Core.Enums;
using SteamLib.Core.Interfaces;
using System.Net;

namespace SteamLib.Authentication;

[PublicAPI]
public static class AdmissionHelper
{
    public const string ACCESS_COOKIE_NAME = "steamLoginSecure";
    public const string REFRESH_COOKIE_NAME = "steamRefresh_steam";
    public const string LANGUAGE_COOKIE_NAME = "Steam_Language";
    public const string SESSION_ID_COOKIE_NAME = "sessionid";

    #region Main

    /// <summary>
    /// Clear and set new session
    /// </summary>
    public static void SetSteamCookies(this CookieContainer container, ISessionData sessionData, string setLanguage = "english")
    {
        container.ClearSteamCookies(setLanguage);


        AddRefreshToken(container, sessionData.RefreshToken);

        var community = SteamDomains.GetDomainUri(SteamDomain.Community);
        container.Add(community, new Cookie(SESSION_ID_COOKIE_NAME, sessionData.SessionId, "/"));
        container.Add(community, new Cookie(LANGUAGE_COOKIE_NAME, setLanguage, "/"));
        TransferCommunityCookies(container);
        foreach (var domain in SteamDomains.AllDomains)
        {
            var token = sessionData.GetToken(domain);
            if (token == null) continue;
            AddTokenCookie(container, token.Value);
        }
    }

    public static void SetDomainCookie(this CookieContainer container, SteamDomain domain, SteamAuthToken token)
    {
        var uri = SteamDomains.GetDomainUri(domain);
        foreach (var cookie in container.GetCookies(uri).Where(c => c.Expired == false && c.Name.EqualsIgnoreCase(ACCESS_COOKIE_NAME)))
        {
            cookie.Expired = true;
        }

        AddTokenCookie(container, token);
    }


    /// <summary>
    /// Clear and set new session
    /// </summary>
    public static void SetSteamMobileCookies(this CookieContainer container, IMobileSessionData mobileSession, 
        string setLanguage = "english")
    {

        container.ClearSteamCookies(setLanguage);
        container.AddMinimalMobileCookies();

        AddRefreshToken(container, mobileSession.RefreshToken);

        var community = SteamDomains.GetDomainUri(SteamDomain.Community);
        container.Add(community, new Cookie("steamid", mobileSession.SteamId.Steam64.ToString()));
        container.Add(community, new Cookie(SESSION_ID_COOKIE_NAME, mobileSession.SessionId));
        container.Add(community, new Cookie(LANGUAGE_COOKIE_NAME, setLanguage));
        TransferCommunityCookies(container);

        foreach (var domain in SteamDomains.AllDomains)
        {
            var token = mobileSession.GetToken(domain);
            if (token == null || token.Value.IsExpired) continue;
            AddTokenCookie(container, token.Value);
        }
    }

    /// <summary>
    /// Clear and set new session. Not recommended. Uses <see cref="IMobileSessionData.GetMobileToken()"/> for domain <see cref="SteamDomain.Community"/> instead of its own cookie. It's okay to use it only for confirmations. But Market, Trading and other pages won't be authenticated
    /// </summary>
    public static void SetSteamMobileCookiesWithMobileToken(this CookieContainer container, IMobileSessionData mobileSession,
        string setLanguage = "english")
    {

        container.ClearSteamCookies(setLanguage);
        container.AddMinimalMobileCookies();

        AddRefreshToken(container, mobileSession.RefreshToken);

        var community = SteamDomains.GetDomainUri(SteamDomain.Community);
        container.Add(community, new Cookie("steamid", mobileSession.SteamId.Steam64.ToString()));
        container.Add(community, new Cookie(SESSION_ID_COOKIE_NAME, mobileSession.SessionId));
        container.Add(community, new Cookie(LANGUAGE_COOKIE_NAME, setLanguage));
        TransferCommunityCookies(container);

        var domainCookieSet = false;
        foreach (var domain in SteamDomains.AllDomains)
        {
            
            var token = mobileSession.GetToken(domain);
            if (token == null || token.Value.IsExpired) continue;
            if(domain == SteamDomain.Community )
                domainCookieSet = true;
            AddTokenCookie(container, token.Value);
        }

        var mobileToken = mobileSession.GetMobileToken();
        if (domainCookieSet == false && mobileToken is {IsExpired: false})
            AddTokenCookie(container, SteamDomain.Community, mobileToken.Value);
    }


    public static void AddMinimalMobileCookies(this CookieContainer container)
    {
        var community = SteamDomains.GetDomainUri(SteamDomain.Community);
        container.Add(community, new Cookie("mobileClientVersion", "777777 3.6.1"));
        container.Add(community, new Cookie("mobileClient", "android"));
    }

    #endregion

    #region Clear
    public static void ClearSteamCookies(this CookieContainer container, string setLanguage = "english")
    {
        var cookies = container.GetAllCookies().Where(IsSteamCookie).ToList();
        foreach (var cookie in cookies)
        {
            cookie.Expired = true;
        }
        container.Add(SteamDomains.GetDomainUri(SteamDomain.Community), new Cookie(LANGUAGE_COOKIE_NAME, setLanguage));
        TransferCommunityCookies(container);
    }

    public static void ClearMobileSessionCookies(this CookieContainer container, string setLanguage = "english")
    {
        container.ClearSteamCookies(setLanguage);
        container.AddMinimalMobileCookies();
        TransferCommunityCookies(container);
    }


    #endregion

    #region Helpers
    public static void TransferCommunityCookies(CookieContainer container)
    {
        var cookies = container.GetAllCookies();


        foreach (Cookie cookie in cookies)
        {
            if (cookie.Domain.Contains("steamcommunity.com") == false || cookie.Expired || cookie.Name.EqualsIgnoreCase(ACCESS_COOKIE_NAME)) continue;


            container.Add(SteamDomains.GetDomainUri(SteamDomain.Store), CloneCookie(cookie));
            container.Add(SteamDomains.GetDomainUri(SteamDomain.Help), CloneCookie(cookie));
            container.Add(SteamDomains.GetDomainUri(SteamDomain.TV), CloneCookie(cookie));
        }

        return;

        static Cookie CloneCookie(Cookie cookie) 
            => new(cookie.Name, cookie.Value, cookie.Path) { Expires = cookie.Expires, Secure = cookie.Secure, HttpOnly = cookie.HttpOnly };
    }

    public static void AddRefreshToken(CookieContainer container, SteamAuthToken token)
    {
        if (token.Type is not (SteamAccessTokenType.Refresh or SteamAccessTokenType.MobileRefresh))
            throw new ArgumentException($"Token must be of type Refresh or MobileRefresh. Provided token has type: {token.Type}", nameof(token));
        var refreshToken = token.SignedToken;
        container.Add(SteamDomains.LoginSteamDomain, new Cookie(REFRESH_COOKIE_NAME, refreshToken)
        {
            Expires = token.Expires.ToLocalDateTime()
        });
    }

    public static void AddTokenCookie(CookieContainer container, SteamAuthToken token)
    {
        if (token.Type is not SteamAccessTokenType.AccessToken)
            throw new ArgumentException($"Token must be of type AccessToken. Provided token has type: {token.Type}", nameof(token));

        AddTokenCookie(container, token.Domain, token);
    }
    private static void AddTokenCookie(CookieContainer container, SteamDomain domain, SteamAuthToken token)
    {
        var domainUri = SteamDomains.GetDomainUri(domain);
        container.Add(domainUri, new Cookie(ACCESS_COOKIE_NAME, token.SignedToken)
        {
            HttpOnly = true,
            Secure = true,
            Expires = token.Expires.ToLocalDateTime()
        });
    }


    public static bool IsSteamCookie(Cookie cookie) =>
        cookie.Domain.Contains("steamcommunity.com") || cookie.Domain.Contains("steampowered.com") || cookie.Domain.Contains("steam.tv");


    public static string? GetSessionId(this CookieContainer container, string domain = "steamcommunity.com")
    {
        var cookies = container.GetAllCookies();
        return cookies
            .FirstOrDefault(c => c.Name.Equals(SESSION_ID_COOKIE_NAME, StringComparison.InvariantCultureIgnoreCase)
                                 && c.Expired == false
                                 && c.Domain.Contains(domain, StringComparison.InvariantCultureIgnoreCase))?
            .Value;
    }


    #endregion

}