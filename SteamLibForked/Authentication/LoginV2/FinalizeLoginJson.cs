using Newtonsoft.Json;

namespace SteamLib.Authentication.LoginV2;

public class FinalizeLoginJson
{
    [JsonProperty("steamID")]
    public ulong SteamId { get; set; }

    [JsonProperty("transfer_info")] 
    public List<TransferInfo> TransferInfo { get; set; } = [];
}

public class TransferInfo
{
    [JsonProperty("url")] 
    public string Url { get; set; } = null!;

    [JsonProperty("params")]
    public TransferInfoParams TransferInfoParams { get; set; } = null!;
}

public class TransferInfoParams
{
    [JsonProperty("nonce")]
    public string Nonce { get; set; } = null!;

    [JsonProperty("auth")] 
    public string Auth { get; set; } = null!;
}