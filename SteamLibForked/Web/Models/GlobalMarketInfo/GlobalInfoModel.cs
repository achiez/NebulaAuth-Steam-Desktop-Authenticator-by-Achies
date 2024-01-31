using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
#pragma warning disable CS8618

namespace SteamLib.Web.Models.GlobalMarketInfo;

[PublicAPI]
public class GlobalInfoModel
{
    [MemberNotNullWhen(true, nameof(SteamId))]
    public bool IsLoggedIn { get; set; }
    
    /// <summary>
    /// <see langword="null"/> if <see cref="IsLoggedIn"/> is <see langword="null"/> or wallet doesn't exist
    /// </summary>
    public MarketWalletSchema? WalletInfo { get; set; }
    public bool RequiresBillingInfo { get; set; }
    public bool HasBillingStates { get; set; }
    public string CountryCode { get; set; }
    public string Language { get; set; }
    public long? SteamId { get; set; }
    public string SessionId { get; set; }
}