using Newtonsoft.Json;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//Pragma disable for legacy code


namespace NebulaAuth.LegacyConverter;

public class Manifest
{
    [JsonProperty("encrypted")] public bool Encrypted { get; set; }

    [JsonProperty("first_run")] public bool FirstRun { get; set; }

    [JsonProperty("entries")] public Entry[] Entries { get; set; }

    [JsonProperty("periodic_checking")] public bool PeriodicChecking { get; set; }

    [JsonProperty("periodic_checking_interval")]
    public long PeriodicCheckingInterval { get; set; }

    [JsonProperty("periodic_checking_checkall")]
    public bool PeriodicCheckingCheckAll { get; set; }

    [JsonProperty("auto_confirm_market_transactions")]
    public bool AutoConfirmMarketTransactions { get; set; }

    [JsonProperty("auto_confirm_trades")] public bool AutoConfirmTrades { get; set; }
}

public class Entry
{
    [JsonProperty("encryption_iv")] public string EncryptionIv { get; set; }

    [JsonProperty("encryption_salt")] public string EncryptionSalt { get; set; }

    [JsonProperty("filename")] public string Filename { get; set; }

    [JsonProperty("steamid")] public ulong SteamId { get; set; }
}