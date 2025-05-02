using Newtonsoft.Json;

namespace SteamLib.Authentication;

#pragma warning disable CS8618
public class TransferParameters
{
    [JsonProperty("steamid")] public long SteamId { get; set; }

    [JsonProperty("token_secure")] public string TokenSecure { get; set; }

    [JsonProperty("auth")] public string Auth { get; set; }

    [JsonProperty("remember_login")] public bool RememberLogin { get; set; }

    [JsonProperty("webcookie")] public string WebCookie { get; set; }
}