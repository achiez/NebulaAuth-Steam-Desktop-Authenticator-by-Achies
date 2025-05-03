using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using AchiesUtilities.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using SteamLib.Account;
using SteamLib.Core.Enums;
using SteamLib.Core.Models;

namespace SteamLib.Authentication;

public static class SteamTokenHelper
{
    public static readonly Regex SteamTokenRegex =
        new(@"^(?<steamIdSignature>(?<steamId>7\d{16,})(?:%7C%7C))?(?<jwt>.+)$", RegexOptions.Compiled);

    public static readonly IReadOnlyDictionary<string, SteamDomain> AudDomains =
        new ReadOnlyDictionary<string, SteamDomain>(new Dictionary<string, SteamDomain>
        {
            {"web:community", SteamDomain.Community},
            {"web:store", SteamDomain.Store},
            {"web:help", SteamDomain.Help},
            {"web:steamtv", SteamDomain.TV},
            {"web:checkout", SteamDomain.Checkout}
        });


    public static bool TryParse(string input, out SteamAuthToken result)
    {
        return Parse(input, out result, true);
    }

    public static SteamAuthToken Parse(string input)
    {
        if (Parse(input, out var result, false))
        {
            return result;
        }

        throw new ArgumentException("Provided Token is not valid JWT token", nameof(input));
    }

    private static bool Parse(string input, out SteamAuthToken result, bool trying)
    {
        result = default;
        var match = SteamTokenRegex.Match(input);
        if (match.Success == false)
        {
            if (trying) return false;
            throw new ArgumentException("Provided Token is not valid SteamLoginSecure or SteamAccess token");
        }


        var jwtStr = match.Groups["jwt"].Value;
        JsonWebToken jwt;
        try
        {
            jwt = new JsonWebToken(jwtStr);
        }
        catch (Exception e)
        {
            if (trying) return false;
            throw new ArgumentException("Provided Token is not valid JWT token", nameof(input), e);
        }

        var steamIdStr = jwt.Subject;
        if (steamIdStr == null || long.TryParse(steamIdStr, out var steamId) == false || steamId < SteamId64.SEED)
        {
            if (trying) return false;
            throw new ArgumentException(
                $"Provided Token is not valid JWT token. Subject claim is null or not valid SteamId '{steamIdStr}'",
                nameof(input));
        }

        SteamId id;
        try
        {
            id = SteamId.FromSteam64(steamId);
        }
        catch (Exception ex)
        {
            if (trying) return false;
            throw new ArgumentException(
                $"Provided Token is not valid JWT token. Subject is claim not valid SteamId '{steamIdStr}'",
                nameof(input), ex);
        }

        var exp = jwt.ValidTo;
        if (exp == DateTime.MinValue)
        {
            if (trying)
                return false;

            throw new ArgumentException("Provided Token has no 'exp' claim.", nameof(input));
        }

        var expires = UnixTimeStamp.FromDateTime(exp);
        var domain = SteamDomain.Undefined;
        var audiences = jwt.Audiences.ToList();
        var tokenType = SteamAccessTokenType.Unknown;
        if (jwt.Audiences.ToList().Count > 0)
        {
            var aud = audiences[0];
            if (AudDomains.TryGetValue(aud, out domain) == false && aud != "web")
            {
                if (trying)
                {
                    result = new SteamAuthToken(jwtStr, id, expires, domain, tokenType);
                    return false;
                }

                throw new ArgumentException($"Provided Token has invalid 'aud' claim. Value: {aud}", nameof(input));
            }

            tokenType = SteamAccessTokenType.AccessToken;

            if (aud == "web")
            {
                var isMobile = audiences.Any(a => a.Equals("mobile"));
                var isRefresh = audiences.Any(a => a.Equals("renew"));
                if (isMobile ^ isRefresh)
                {
                    tokenType = isMobile ? SteamAccessTokenType.Mobile : SteamAccessTokenType.Refresh;
                }
                else if (isMobile && isRefresh)
                {
                    tokenType = SteamAccessTokenType.MobileRefresh;
                }
                else if (audiences.Count == 1)
                {
                    tokenType = SteamAccessTokenType.Web;
                }
                else
                {
                    if (trying)
                    {
                        result = new SteamAuthToken(jwtStr, id, expires, domain, SteamAccessTokenType.Unknown);
                        return false;
                    }

                    var auds = string.Join(", ", audiences);
                    throw new ArgumentException($"Provided Token has invalid 'aud' claim combination. Values: {auds}",
                        nameof(input));
                }
            }
        }


        result = new SteamAuthToken(jwtStr, id, expires, domain, tokenType);
        return true;
    }

    public static string ExtractJwtFromSetCookiesHeader(HttpHeaders headers)
    {
        var setCookies = headers.GetValues("Set-Cookie");
        var steamLoginSecureHeader = setCookies.FirstOrDefault(x => x.StartsWith("steamLoginSecure="));
        if (steamLoginSecureHeader == null)
            throw new ArgumentException("Can't find steamLoginSecure cookie in Set-Cookie header");

        var firstCookiePart = steamLoginSecureHeader
            .ToCharArray()
            .SkipWhile(ch => !ch.Equals('='))
            .Skip(1)
            .TakeWhile(ch => !ch.Equals(';'))
            .ToArray();

        var steamLoginSecure = new string(firstCookiePart);
        return ExtractJwtToken(steamLoginSecure);
    }

    public static string CombineJwtWithSteamId(long steamId, string loginValue)
    {
        return steamId + "%7C%7C" + loginValue;
    }

    internal static bool CheckIfProperLoginValue(string value)
    {
        return value.Contains("%7C%7C");
    }

    internal static string CombineLoginValueIfNeeded(long steamId, string loginValue)
    {
        return CheckIfProperLoginValue(loginValue) == false ? CombineJwtWithSteamId(steamId, loginValue) : loginValue;
    }

    public static string ExtractJwtToken(string steamLoginSecure)
    {
        var match = SteamTokenRegex.Match(steamLoginSecure);
        if (match.Success == false) throw new ArgumentException("Can't extract JWT Access Token from steamLoginSecure");
        return match.Groups["jwt"].Value;
    }
}