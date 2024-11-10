using Newtonsoft.Json;

namespace SteamLib.Utility.MafileSerialization;

internal class LegacyMafile
{
    [JsonProperty("shared_secret"), JsonRequired] public string SharedSecret { get; set; } = default!;
    [JsonProperty("identity_secret"), JsonRequired] public string IdentitySecret { get; set; } = default!;
    [JsonProperty("device_id"), JsonRequired] public string DeviceId { get; set; } = default!;
    [JsonProperty("revocation_code")] public string RevocationCode { get; set; } = default!;
    [JsonProperty("account_name")] public string AccountName { get; set; } = default!;
    [JsonProperty("Session")] public object? SessionData { get; set; } = default!;
    [JsonProperty("server_time")] public long ServerTime { get; set; } //Unused
    [JsonProperty("steamid")] public long SteamId { get; set; }
    [JsonProperty("serial_number")] public string SerialNumber { get; set; } = default!; //Unused
    [JsonProperty("uri")] public string Uri { get; set; } = default!; //Unused
    [JsonProperty("token_gid")] public string TokenGid { get; set; } = default!; //Unused
    [JsonProperty("secret_1")] public string Secret1 { get; set; } = default!; //Unused
}