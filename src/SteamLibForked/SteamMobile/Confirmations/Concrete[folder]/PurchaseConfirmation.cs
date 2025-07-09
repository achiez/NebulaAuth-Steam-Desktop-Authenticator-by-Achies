namespace SteamLib.SteamMobile.Confirmations;

public class PurchaseConfirmation : Confirmation
{
    public PurchaseConfirmation(long id, ulong nonce, int intType, ulong creatorId, string typeName) : base(id, nonce, intType, creatorId, ConfirmationType.Purchase, typeName)
    {
    }
}