using Newtonsoft.Json;

namespace SteamLib.Authentication.LoginV2;

class FinalizeLoginJson
{
    [JsonProperty("steamID")]
    public ulong SteamId { get; set; }

    [JsonProperty("transfer_info")]
    public List<TransferInfo> TransferInfo { get; set; }
}

class TransferInfo
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("params")]
    public TransferInfoParams TransferInfoParams { get; set; }
}


class TransferInfoParams
{
    [JsonProperty("nonce")]
    public string Nonce { get; set; }

    [JsonProperty("auth")]
    public string Auth { get; set; }
}