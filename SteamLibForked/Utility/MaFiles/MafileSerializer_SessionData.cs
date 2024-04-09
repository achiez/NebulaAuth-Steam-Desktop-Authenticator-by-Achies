using Newtonsoft.Json.Linq;
using SteamLib.Account;
using SteamLib.Authentication;
using SteamLib.Core.Enums;

namespace SteamLib.Utility.MaFiles;

public partial class MafileSerializer //SessionData
{
    private static MobileSessionData? DeserializeMobileSessionData(JObject j, out DeserializedMafileSessionResult result)
    {
        result = DeserializedMafileSessionResult.Invalid;
        var refreshTokenToken = GetToken(j, nameof(MobileSessionData.RefreshToken), "refreshtoken", "refresh_token",
            "refresh", "OAuthToken");


        SteamAuthToken refreshToken;
        if (refreshTokenToken == null || refreshTokenToken.Type == JTokenType.Null) return null;
        if (refreshTokenToken.Type == JTokenType.String && SteamTokenHelper.TryParse(refreshTokenToken.Value<string>()!, out var parsed))
        {
            refreshToken = parsed;
        }
        else if (refreshTokenToken.Type == JTokenType.Object)
        {
            try
            {
                refreshToken = refreshTokenToken.ToObject<SteamAuthToken>();
            }
            catch
            {
                return null;
            }
        }
        else
        {
            return null;
        }


        var sessionId = GetString(j, "sessionid", "session_id", "session");
        var accessTokenToken = GetToken(j, "accesstoken", "access_token", "access");
        accessTokenToken ??= GetToken(j, "steamLoginSecure");

        if (string.IsNullOrWhiteSpace(sessionId)) return null;

        SteamAuthToken? accessToken = null;
        if (accessTokenToken == null || accessTokenToken.Type == JTokenType.Null)
        {
            accessToken = null;
        }
        if (accessTokenToken is { Type: JTokenType.Object })
        {
            try
            {
                accessToken = refreshTokenToken.ToObject<SteamAuthToken>();
            }
            catch
            {
                // ignored
            }
        }
        else if (accessTokenToken is { Type: JTokenType.String } &&
                 SteamTokenHelper.TryParse(accessTokenToken.Value<string>()!, out var token) && token.Type == SteamAccessTokenType.Mobile)
        {
            accessToken = token;
        }
   

        var sessionData = new MobileSessionData(sessionId, refreshToken.SteamId, refreshToken, accessToken, new Dictionary<SteamDomain, SteamAuthToken>());
        sessionData.IsValid = SessionDataValidator.Validate(null, sessionData).Succeeded;
        if(sessionData.IsValid == false)
            return null;

        if (refreshToken.IsExpired || refreshToken.Type != SteamAccessTokenType.MobileRefresh)
        {
            result = DeserializedMafileSessionResult.Expired;
        }
        else
        {
            result = DeserializedMafileSessionResult.Valid;
        }

        return sessionData;
    }
}