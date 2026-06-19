using System.Net;
using AchiesUtilities.Extensions;
using AchiesUtilities.Models;
using Newtonsoft.Json;
using SteamLib.Core;
using SteamLib.Models.Account;
using SteamLibForked.Abstractions;
using SteamLibForked.Models.Core;
using SteamLibForked.Models.Session;

namespace SteamLib.Authentication;

public static class AdmissionHelper
{
    public const string ACCESS_COOKIE_NAME = "steamLoginSecure";
    public const string REFRESH_COOKIE_NAME = "steamRefresh_steam";
    public const string LANGUAGE_COOKIE_NAME = "Steam_Language";
    public const string SESSION_ID_COOKIE_NAME = "sessionid";

    private static readonly Uri SteamLoginUri = new(SteamConstants.STEAM_LOGIN);

    public static void InjectWebTradeEligibilityCookie(this CookieContainer container,
        WebTradeEligibility? eligibility = null)
    {
        eligibility ??= new WebTradeEligibility
        {
            Allowed = 1,
            AllowedAtTime = 0,
            SteamGuardRequiredDays = 15,
            NewDeviceCooldownDays = 0,
            TimeChecked = UnixTimeStamp.FromDateTime(DateTime.Now)
        };
        var json = JsonConvert.SerializeObject(eligibility);
        var encoded = WebUtility.UrlEncode(json);
        var cookie = new Cookie("webTradeEligibility", encoded, "/")
        {
            HttpOnly = true
        };
        container.Add(SteamDomains.DomainUris[SteamDomain.Community], cookie);
    }

    #region Main

    /// <summary>
    ///     Clear and set new session
    /// </summary>
    public static void SetSteamCookies(this CookieContainer container, ISessionData? sessionData,
        string setLanguage = "english")
    {
        container.ClearSteamCookies(setLanguage);
        if (sessionData == null)
        {
            TransferCommunityCookies(container);
            return;
        }

        container.SetSteamRefreshToken(sessionData.RefreshToken);

        var community = SteamDomains.GetDomainUri(SteamDomain.Community);
        container.Add(community, new Cookie(SESSION_ID_COOKIE_NAME, sessionData.SessionId, "/"));
        container.Add(community, new Cookie(LANGUAGE_COOKIE_NAME, setLanguage, "/"));
        TransferCommunityCookies(container);
        foreach (var domain in SteamDomains.WebDomains)
        {
            var token = sessionData.GetToken(domain);
            if (token == null) continue;
            container.SetSteamAccessToken(token.Value);
        }
    }

    /// <summary>
    ///     Clear and set new session
    /// </summary>
    public static void SetSteamMobileCookies(this CookieContainer container, ISessionData? mobileSession,
        string setLanguage = "english")
    {
        container.ClearSteamCookies(setLanguage);
        container.AddMinimalMobileCookies();
        if (mobileSession == null)
        {
            TransferCommunityCookies(container);
            return;
        }

        container.SetSteamRefreshToken(mobileSession.RefreshToken);

        var community = SteamDomains.GetDomainUri(SteamDomain.Community);
        container.Add(community, new Cookie("steamid", mobileSession.SteamId.Steam64.ToString()));
        container.Add(community, new Cookie(SESSION_ID_COOKIE_NAME, mobileSession.SessionId));
        container.Add(community, new Cookie(LANGUAGE_COOKIE_NAME, setLanguage));
        TransferCommunityCookies(container);
        foreach (var domain in SteamDomains.WebDomains)
        {
            var token = mobileSession.GetToken(domain);
            if (token == null) continue;
            var domainUri = SteamDomains.GetDomainUri(domain);
            container.SetSteamAccessTokenUnsafe(token.Value, domainUri);
        }
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

    public static void ClearAllCookies(this CookieContainer container)
    {
        var cookies = container.GetAllCookies().ToList();
        foreach (var cookie in cookies)
        {
            cookie.Expired = true;
        }
    }

    #endregion

    #region Helpers

    public static void TransferCommunityCookies(CookieContainer container)
    {
        var cookies = container.GetAllCookies();


        foreach (Cookie cookie in cookies)
        {
            if (!cookie.Domain.Contains("steamcommunity.com") || cookie.Expired ||
                cookie.Name.EqualsIgnoreCase(ACCESS_COOKIE_NAME)) continue;


            container.Add(SteamDomains.GetDomainUri(SteamDomain.Store), CloneCookie(cookie));
            container.Add(SteamDomains.GetDomainUri(SteamDomain.Help), CloneCookie(cookie));
            container.Add(SteamDomains.GetDomainUri(SteamDomain.TV), CloneCookie(cookie));
            container.Add(SteamDomains.GetDomainUri(SteamDomain.Checkout), CloneCookie(cookie));
        }

        return;

        static Cookie CloneCookie(Cookie cookie)
        {
            return new Cookie(cookie.Name, cookie.Value, cookie.Path)
                {Expires = cookie.Expires, Secure = cookie.Secure, HttpOnly = cookie.HttpOnly};
        }
    }

    /// <summary>
    ///     Sets a Steam refresh token as a cookie to the specified cookie container.
    /// </summary>
    /// <remarks>
    ///     The added cookie will have its expiration set according to the token's expiration time. This
    ///     method is typically used to enable authenticated requests to Steam services that require a refresh
    ///     token.
    /// </remarks>
    /// <param name="container">The cookie container to which the Steam refresh token cookie will be added. Cannot be null.</param>
    /// <param name="token">
    ///     The Steam authentication token to add as a refresh cookie. Must be of type Refresh or
    ///     MobileRefresh.
    /// </param>
    /// <exception cref="ArgumentException">Thrown if the token is not of type Refresh or MobileRefresh.</exception>
    public static void SetSteamRefreshToken(this CookieContainer container, SteamAuthToken token)
    {
        if (token.Type is not (SteamAccessTokenType.Refresh or SteamAccessTokenType.MobileRefresh))
            throw new ArgumentException(
                $"Token must be of type Refresh or MobileRefresh. Provided token has type: {token.Type}",
                nameof(token));
        SetSteamRefreshTokenUnsafe(container, token, SteamLoginUri);
    }

    /// <summary>
    ///     Sets a Steam refresh token as a cookie to the specified cookie container for the given domain.
    /// </summary>
    /// <remarks>
    ///     This method does not perform validation on the input parameters. Callers must ensure that the
    ///     provided values are valid and appropriate for use.
    /// </remarks>
    /// <param name="container">The cookie container to which the refresh token cookie will be added. Cannot be null.</param>
    /// <param name="token">
    ///     The Steam authentication token containing the signed token value and expiration information. Cannot
    ///     be null.
    /// </param>
    /// <param name="domainUri">The URI of the domain for which the refresh token cookie should be set. Cannot be null.</param>
    public static void SetSteamRefreshTokenUnsafe(CookieContainer container, SteamAuthToken token, Uri domainUri)
    {
        container.Add(domainUri, new Cookie(REFRESH_COOKIE_NAME, token.SignedToken)
        {
            Expires = token.Expires.ToLocalDateTime()
        });
    }

    /// <summary>
    ///     Sets a Steam access token to the specified cookie container for use with Steam web requests.
    /// </summary>
    /// <remarks>
    ///     This method is intended for standard web access tokens bound to a specific Steam web domain.
    ///     Mobile tokens are intentionally rejected, since their audiences are capability-based rather
    ///     than domain-based and may grant access to multiple web domains.
    /// </remarks>
    /// <param name="container">The cookie container to which the Steam access token will be added. Cannot be null.</param>
    /// <param name="token">
    ///     The Steam access token to add. Must be of type AccessToken and associated with a valid Steam
    ///     domain.
    /// </param>
    /// <exception cref="ArgumentException">Thrown if the token is not of type AccessToken.</exception>
    public static void SetSteamAccessToken(this CookieContainer container, SteamAuthToken token)
    {
        if (token.Type == SteamAccessTokenType.Mobile)
            throw new ArgumentException(
                $"Mobile access tokens cannot be added using this method. Use SetSteamAccessTokenUnsafe instead. Provided token has type: {token.Type}",
                nameof(token));
        if (token.Type is not SteamAccessTokenType.AccessToken)
            throw new ArgumentException($"Token must be of type AccessToken. Provided token has type: {token.Type}",
                nameof(token));

        var domainUri = SteamDomains.GetDomainUri(token.Domain);
        container.SetSteamAccessTokenUnsafe(token, domainUri);
    }


    /// <summary>
    ///     Sets a Steam access token as a secure, HTTP-only cookie to the specified cookie container for the given domain.
    /// </summary>
    /// <remarks>
    ///     This method does not perform validation on the input parameters and should only be used when
    ///     input values are trusted. The added cookie is marked as secure and HTTP-only, and its expiration is set
    ///     according to the token's expiration time.
    /// </remarks>
    /// <param name="container">The cookie container to which the Steam access token cookie will be added. Cannot be null.</param>
    /// <param name="token">The Steam access token to add as a cookie. Must contain a valid signed token and expiration.</param>
    /// <param name="domainUri">The URI of the domain for which the cookie will be set. Cannot be null.</param>
    public static void SetSteamAccessTokenUnsafe(this CookieContainer container, SteamAuthToken token, Uri domainUri)
    {
        container.Add(domainUri, new Cookie(ACCESS_COOKIE_NAME, token.SignedToken)
        {
            HttpOnly = true,
            Secure = true,
            Expires = token.Expires.ToLocalDateTime()
        });
    }

    public static bool IsSteamCookie(Cookie cookie)
    {
        return cookie.Domain.Contains("steamcommunity.com") || cookie.Domain.Contains("steampowered.com") ||
               cookie.Domain.Contains("steam.tv");
    }


    public static string? GetSessionId(this CookieContainer container, string domain = "steamcommunity.com")
    {
        var cookies = container.GetAllCookies();
        return cookies
            .FirstOrDefault(c => c.Name.Equals(SESSION_ID_COOKIE_NAME, StringComparison.InvariantCultureIgnoreCase)
                                 && !c.Expired
                                 && c.Domain.Contains(domain, StringComparison.InvariantCultureIgnoreCase))?
            .Value;
    }

    #endregion
}