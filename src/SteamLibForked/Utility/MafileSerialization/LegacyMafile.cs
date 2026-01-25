using Newtonsoft.Json;

namespace SteamLib.Utility.MafileSerialization;

internal class LegacyMafile
{
    [JsonProperty("shared_secret")]
    public string SharedSecret { get; set; } = null!;

    [JsonProperty("identity_secret")]
    public string IdentitySecret { get; set; } = null!;

    [JsonProperty("device_id")]
    public string DeviceId { get; set; } = null!;

    [JsonProperty("revocation_code")] public string RevocationCode { get; set; } = null!;
    [JsonProperty("account_name")] public string AccountName { get; set; } = null!;
    [JsonProperty("Session")] public object? SessionData { get; set; }
    [JsonProperty("server_time")] public long ServerTime { get; set; } //Unused
    [JsonProperty("steamid")] public long SteamId { get; set; }
    [JsonProperty("serial_number")] public string SerialNumber { get; set; } = null!; //Unused
    [JsonProperty("uri")] public string Uri { get; set; } = null!; //Unused
    [JsonProperty("token_gid")] public string TokenGid { get; set; } = null!; //Unused
    [JsonProperty("secret_1")] public string Secret1 { get; set; } = null!; //Unused
}