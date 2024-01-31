namespace SteamLib.SteamMobile.Confirmations;

public class Confirmation
{
    public long Id { get; }

    /// <summary>
    /// The unique key used to act upon this confirmation.
    /// </summary>
    public ulong Nonce { get; }

    /// <summary>
    /// The value of the data-type HTML attribute returned for this contribution.
    /// </summary>
    public int IntType;

    /// <summary>
    /// Represents either the Trade Offer ID or market transaction ID that caused this confirmation to be created.
    /// </summary>
    public long CreatorId { get; } 

    /// <summary>
    /// The type of this confirmation.
    /// </summary>
    public readonly ConfirmationType ConfType;

    public string TypeName { get; init; }
    public string HeadLine { get; init; } = string.Empty;

    public DateTime Time { get; set; }

    public virtual ConfirmationDetails? Details { get; }

    public Confirmation(long id, ulong nonce, int intType, long creatorId, ConfirmationType confType, string typeName)
    {
        Id = id;
        Nonce = nonce;
        IntType = intType;
        CreatorId = creatorId;
        ConfType = confType;
        TypeName = typeName;
    }
    
}