using Newtonsoft.Json.Linq;
using SteamLib.Account;
using SteamLib.Authentication;
using SteamLib.Core.Enums;
using SteamLib.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace SteamLib.Utility.MafileSerialization;

public partial class MafileSerializer //SessionData
{
    private MobileSessionData? DeserializeMobileSessionData(JObject j, out DeserializedMafileSessionResult result,
        out SteamId? steamId)
    {
        steamId = GetSessionSteamId(j);
        result = DeserializedMafileSessionResult.Invalid;
        var refreshToken = GetAuthToken(j, nameof(MobileSessionData.RefreshToken), "refreshtoken", "refresh_token",
            "refresh", "OAuthToken");

        if (refreshToken is not { Type:  SteamAccessTokenType.MobileRefresh })
        {
            result = DeserializedMafileSessionResult.Invalid;
            return null;
        }

        var accessToken = GetAuthToken(j, "accesstoken", "access_token", "access", "steamLoginSecure");
        if (accessToken is not { Type: SteamAccessTokenType.Mobile })
        {
            accessToken = null;
        }

        steamId = refreshToken.Value.SteamId;


        var sessionId = GetSessionId(j, Settings, "sessionid", "session_id", "session");
        if (sessionId == null)
        {
            result = DeserializedMafileSessionResult.Invalid;
            return null;
        }

        var sessionData = new MobileSessionData(sessionId, steamId.Value, refreshToken.Value, accessToken, tokens: null);
        sessionData.IsValid = SessionDataValidator.Validate(null, sessionData).Succeeded;
        if (sessionData.IsValid == false)
            return null;

        return sessionData;
    }

    private static SteamAuthToken? GetAuthToken(JObject j, params string[] aliases)
    {
        var jAuthToken = GetToken(j, aliases);


        SteamAuthToken? token = null;
        if (jAuthToken == null || jAuthToken.Type == JTokenType.Null) return null;
        if (jAuthToken.Type == JTokenType.String &&
            SteamTokenHelper.TryParse(jAuthToken.Value<string>()!, out var parsed))
        {
            token = parsed;
        }
        else if (jAuthToken.Type == JTokenType.Object)
        {
            try
            {
                token = jAuthToken.ToObject<SteamAuthToken>();
            }
            catch
            {
                //Ignored
            }
        }

        return token;
    }

    private static SteamId? GetSessionSteamId(JObject j)
    {
        var token = GetToken(j, "steamid");
        if (token == null || token.Type == JTokenType.Null)
            return null;

        if (token.Type == JTokenType.Integer)
            return SteamId.FromSteam64(token.Value<long>());

        if (token.Type == JTokenType.String && SteamId64.TryParse(token.Value<string>(), out var steamId))
        {
            return new SteamId(steamId);
        }

        return null;
    }

    private static string? GetSessionId(JObject j, MafileSerializerSettings settings, params string[] aliases)
    {
        var sessionId = GetString(j, aliases);
        if (sessionId == null && settings.DeserializationOptions.AllowSessionIdGeneration)
        {
            return GenerateRandomHex();
        }
        return sessionId;

        static string GenerateRandomHex(int byteLength = 12)
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

}

