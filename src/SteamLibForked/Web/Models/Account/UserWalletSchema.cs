using AchiesUtilities.Newtonsoft.JSON.Converters.Common;
using Newtonsoft.Json;

namespace SteamLib.Web.Models.Account;

#pragma warning disable CS8618
public class UserWalletSchema
{
    [JsonProperty("amount")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int Amount { get; set; }

    [JsonProperty("currency")] public string Currency { get; set; }
}