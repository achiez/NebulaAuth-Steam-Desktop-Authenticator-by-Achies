using Newtonsoft.Json;
using SteamLib.Web.Converters;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//TODO: Fix pragma warning

namespace SteamLib.Utility.MaFiles;

internal class LegacyMafile //TODO: move
{

    [JsonProperty("shared_secret")]
    [JsonRequired]
    public string SharedSecret { get; set; }

    [JsonProperty("identity_secret")]
    [JsonRequired]
    public string IdentitySecret { get; set; }

    [JsonProperty("device_id")]
    [JsonRequired]
    public string DeviceId { get; set; }

    [JsonProperty("revocation_code")]
    public string RevocationCode { get; set; }

    [JsonProperty("account_name")]
    public string AccountName { get; set; }

    [JsonProperty("Session")]
    public object? SessionData { get; set; }
    [JsonProperty("server_time")] public long ServerTime { get; set; } //Unused

    [JsonProperty("serial_number")]
    [JsonConverter(typeof(StringToLongIfNeededConverter))]
    public string SerialNumber { get; set; } //Unused
    [JsonProperty("uri")] public string Uri { get; set; } //Unused
    [JsonProperty("token_gid")] public string TokenGid { get; set; } //Unused
    [JsonProperty("secret_1")] public string Secret1 { get; set; } //Unused

}