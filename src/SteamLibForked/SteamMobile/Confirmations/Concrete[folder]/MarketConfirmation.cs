namespace SteamLib.SteamMobile.Confirmations;

public class MarketConfirmation : Confirmation
{
    public string ItemImageUri { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string PriceString { get; init; } = string.Empty;

    public MarketConfirmation(long id, ulong key, ulong creator, string typeName) : base(id, key, 3, creator,
        ConfirmationType.MarketSellTransaction, typeName)
    {
    }
}