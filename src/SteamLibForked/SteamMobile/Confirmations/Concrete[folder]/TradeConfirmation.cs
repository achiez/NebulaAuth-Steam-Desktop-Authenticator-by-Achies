namespace SteamLib.SteamMobile.Confirmations;

public class TradeConfirmation : Confirmation
{
    public string UserAvatarUri { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsReceiveNothing { get; set; }
    public override TradeConfirmationDetails? Details { get; }

    public long TradeId => CreatorId;

    public TradeConfirmation(long id, ulong nonce, long creator, string typeName) : base(id, nonce, 1, creator,
        ConfirmationType.Trade, typeName)
    {
    }
}