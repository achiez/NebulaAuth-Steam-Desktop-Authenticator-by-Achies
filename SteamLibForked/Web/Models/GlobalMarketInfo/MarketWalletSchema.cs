using AchiesUtilities.Newtonsoft.JSON.Converters.Common;
using Newtonsoft.Json;
using SteamLib.Core.Enums;

#pragma warning disable CS8618

namespace SteamLib.Web.Models.GlobalMarketInfo;

public class MarketWalletSchema
{
    [JsonProperty("wallet_currency")]
    public Currency WalletCurrency { get; set; }

    [JsonProperty("wallet_country")]
    public string WalletCountry { get; set; }

    [JsonProperty("wallet_state")]
    public string WalletState { get; set; }

    [JsonProperty("wallet_fee")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletFee { get; set; }

    [JsonProperty("wallet_fee_minimum")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletFeeMinimum { get; set; }

    [JsonProperty("wallet_fee_percent")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double WalletFeePercent { get; set; }

    [JsonProperty("wallet_publisher_fee_percent_default")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double WalletPublisherFeePercentDefault { get; set; }

    [JsonProperty("wallet_fee_base")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletFeeBase { get; set; }

    [JsonProperty("wallet_balance")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletBalance { get; set; }

    [JsonProperty("wallet_delayed_balance")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletDelayedBalance { get; set; }

    [JsonProperty("wallet_max_balance")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletMaxBalance { get; set; }

    [JsonProperty("wallet_trade_max_balance")]
    [JsonConverter(typeof(IntToStringConverter))]
    public int WalletTradeMaxBalance { get; set; }

    [JsonProperty("success")]
    public int Success { get; set; }

    [JsonProperty("rwgrsn")]
    public int Rwgrsn { get; set; }
}