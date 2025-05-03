using Newtonsoft.Json;

namespace SteamLib.Authentication;

#pragma warning disable CS8618
public class OAuthData
{
    [JsonProperty("steamid")] public long SteamId { get; set; }
    [JsonProperty("oauth_token")] public string OAuthToken { get; set; }
    [JsonProperty("webcookie")] public string WebCookie { get; set; }
    [JsonProperty("wgtoken")] public string SteamLogin { get; set; }
    [JsonProperty("wgtoken_secure")] public string SteamLoginSecure { get; set; }
}