namespace SteamLib.SteamMobile.Confirmations;

public class PurchaseConfirmation : Confirmation
{
    public PurchaseConfirmation(long id, ulong nonce, ulong creatorId, string typeName) : base(id, nonce, 12, creatorId,
        ConfirmationType.Purchase, typeName)
    {
    }
}