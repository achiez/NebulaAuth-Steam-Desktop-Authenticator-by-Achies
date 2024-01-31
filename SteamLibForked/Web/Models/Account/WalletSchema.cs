using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SteamLib.Web.Models.Account;

public class WalletInfoSchema
{
    [JsonProperty("success")]
    public int Success { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("country_code")]
    public string CountryCode { get; set; }

    [JsonProperty("alternate_min_amount")]
    public bool AlternateMinAmount { get; set; }

    [JsonProperty("amounts")]
    public List<int> Amounts { get; set; }

    [JsonProperty("related_trans_type")]
    public bool RelatedTransType { get; set; }

    [JsonProperty("related_trainsid")]
    public bool RelatedTrainsId { get; set; }

    [JsonProperty("user_wallet")]
    public UserWalletSchema UserWalletSchema { get; set; }
}