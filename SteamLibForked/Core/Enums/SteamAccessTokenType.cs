using SteamLib.Api.Mobile;

namespace SteamLib.Core.Enums;

public enum SteamAccessTokenType
{
    Unknown = 0,

    /// <summary>
    ///     Temporary token that returned after login
    /// </summary>
    Web,

    /// <summary>
    ///     Access token that attached to domain
    /// </summary>
    AccessToken,

    /// <summary>
    ///     Refresh token that can be used to refresh AccessToken
    /// </summary>
    Refresh,

    /// <summary>
    ///     Mobile token that can be used to access mobile endpoints. Returned after login or by refreshing with
    ///     <see cref="SteamMobileApi.RefreshJwt" />
    /// </summary>
    Mobile,

    /// <summary>
    ///     Refresh token that can be used to refresh Mobile token
    /// </summary>
    MobileRefresh
}