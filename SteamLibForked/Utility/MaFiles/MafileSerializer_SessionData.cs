using AchiesUtilities.Models;
using Newtonsoft.Json.Linq;
using SteamLib.Account;
using SteamLib.Authentication;
using SteamLib.Core.Enums;
using System.Security.Cryptography;
using System.Text;

namespace SteamLib.Utility.MaFiles;

public partial class MafileSerializer //SessionData
{
    private static MobileSessionData? DeserializeMobileSessionData(JObject j, bool allowSessionIdGeneration, out DeserializedMafileSessionResult result)
    {
        result = DeserializedMafileSessionResult.Invalid;
        var jRefreshToken = GetToken(j, nameof(MobileSessionData.RefreshToken), "refreshtoken", "refresh_token",
            "refresh", "OAuthToken");


        SteamAuthToken? refreshToken = null;
        if (jRefreshToken == null || jRefreshToken.Type == JTokenType.Null) return null;
        if (jRefreshToken.Type == JTokenType.String && SteamTokenHelper.TryParse(jRefreshToken.Value<string>()!, out var parsed))
        {
            refreshToken = parsed;
        }
        else if (jRefreshToken.Type == JTokenType.Object)
        {
            try
            {
                refreshToken = jRefreshToken.ToObject<SteamAuthToken>();
            }
            catch
            {
               //Ignored
            }
        }

        //if (refreshToken == null)
        //{
        //    result = DeserializedMafileSessionResult.Invalid;
        //    return null;
        //}


        var sessionId = GetString(j, "sessionid", "session_id", "session");
        var jAccessToken = GetToken(j, "accesstoken", "access_token", "access");
        jAccessToken ??= GetToken(j, "steamLoginSecure");

        if (sessionId == null && allowSessionIdGeneration)
        {
            sessionId = GenerateRandomHex(12);
        }else if (sessionId == null)
        {
            return null;
        }

        SteamAuthToken? accessToken = null;

        if (jAccessToken == null || jAccessToken.Type == JTokenType.Null)
        {
            accessToken = null;
        }
        else if (jAccessToken is { Type: JTokenType.Object })
        {
            try
            {
                accessToken = jRefreshToken.ToObject<SteamAuthToken>();
            }
            catch
            {
                // ignored
            }
        }
        else if (jAccessToken is { Type: JTokenType.String } &&
                 SteamTokenHelper.TryParse(jAccessToken.Value<string>()!, out var token) && token.Type == SteamAccessTokenType.Mobile)
        {
            accessToken = token;
        }
   

        var steamId = refreshToken?.SteamId ?? GetSessionSteamId(j);
        if (steamId == null)
        {
            result = DeserializedMafileSessionResult.Invalid;
            return null;
        }

        refreshToken ??= CreateInvalid(steamId.Value);
        var sessionData = new MobileSessionData(sessionId, steamId.Value, refreshToken.Value, accessToken, new Dictionary<SteamDomain, SteamAuthToken>());
        sessionData.IsValid = SessionDataValidator.Validate(null, sessionData).Succeeded;
        if(sessionData.IsValid == false)
            return null;

        if (refreshToken.Value.IsExpired || refreshToken.Value.Type != SteamAccessTokenType.MobileRefresh)
        {
            result = DeserializedMafileSessionResult.Expired;
        }
        else
        {
            result = DeserializedMafileSessionResult.Valid;
        }

        return sessionData;
    }

    private static SteamId? GetSessionSteamId(JObject j)
    {
        var token = GetToken(j, "steamid");
        if (token == null || token.Type == JTokenType.Null)
            return null;

        if(token.Type == JTokenType.Integer)
            return SteamId.FromSteam64(token.Value<long>());

        if (token.Type == JTokenType.String && long.TryParse(token.Value<string>()!, out var steamId))
        {
            return SteamId.FromSteam64(steamId);
        }

        return null;
    }

    //Workaround to avoid session being invalidated due to missing a valid token.
    //The reason for this change is the inability to proxy/change group for old mafiles, which creates more problems than benefits.
    //A temporary solution until I decide how to read the SteamID correctly without invalidating the entire session.
    //It also makes the LoginAgainOnImport mechanism useless, which is good outcome.
    //Most likely I need to reconsider the reaction to an “invalid” session and simply feed it to the software as “expired”.
    //Also, when deciding not to validate RefreshToken, I need to reconsider the entire validation method in the Validator class and think through the consequences in the rest of the code.
    //FIXME: Refactor code to avoid this workaround and make it more organic.
    //TODO: after fixing the issue, reflect changes in the original library
    private static SteamAuthToken CreateInvalid(SteamId steamId)
    {
       return new SteamAuthToken("invalid", steamId, UnixTimeStamp.Zero, SteamDomain.Community, SteamAccessTokenType.MobileRefresh);
    }

    private static string GenerateRandomHex(int byteLength = 12)
    {
        byte[] randomBytes = new byte[byteLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var hex = new StringBuilder(byteLength * 2);
        foreach (var b in randomBytes)
            hex.Append($"{b:x2}");

        return hex.ToString();
    }
}