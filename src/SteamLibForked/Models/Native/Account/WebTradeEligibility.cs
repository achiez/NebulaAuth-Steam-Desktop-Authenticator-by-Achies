using AchiesUtilities.Models;
using AchiesUtilities.Newtonsoft.JSON.Converters.Special;
using Newtonsoft.Json;

namespace SteamLib.Models.Account;

public class WebTradeEligibility
{
    [JsonProperty("allowed")] public int Allowed { get; set; }

    /// <summary>
    ///     16416 - SteamGuard
    ///     16672 - Password reset
    /// </summary>
    [JsonProperty("reason")]
    public int Reason { get; set; }

    [JsonProperty("allowed_at_time")] public long AllowedAtTime { get; set; }

    [JsonProperty("steamguard_required_days")]
    public int SteamGuardRequiredDays { get; set; }

    [JsonProperty("new_device_cooldown_days")]
    public int NewDeviceCooldownDays { get; set; }

    [JsonProperty("expiration")]
    [JsonConverter(typeof(UnixTimeStampConverter))]
    public UnixTimeStamp Expiration { get; set; }

    [JsonProperty("time_checked")]
    [JsonConverter(typeof(UnixTimeStampConverter))]
    public UnixTimeStamp TimeChecked { get; set; }
}