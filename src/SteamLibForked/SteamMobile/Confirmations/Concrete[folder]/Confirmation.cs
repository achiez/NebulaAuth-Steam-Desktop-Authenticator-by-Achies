﻿namespace SteamLib.SteamMobile.Confirmations;

public class Confirmation
{
    public long Id { get; }

    /// <summary>
    ///     The unique key used to act upon this confirmation.
    /// </summary>
    public ulong Nonce { get; }

    /// <summary>
    ///     Represents either the Trade Offer ID or market transaction ID that caused this confirmation to be created.
    /// </summary>
    public ulong CreatorId { get; }

    public string TypeName { get; init; }
    public string HeadLine { get; init; } = string.Empty;

    public DateTime Time { get; set; }

    public virtual ConfirmationDetails? Details { get; }

    /// <summary>
    ///     The type of this confirmation.
    /// </summary>
    public readonly ConfirmationType ConfType;

    /// <summary>
    ///     The value of the data-type HTML attribute returned for this contribution.
    /// </summary>
    public int IntType;

    public Confirmation(long id, ulong nonce, int intType, ulong creatorId, ConfirmationType confType, string typeName)
    {
        Id = id;
        Nonce = nonce;
        IntType = intType;
        CreatorId = creatorId;
        ConfType = confType;
        TypeName = typeName;
    }
}