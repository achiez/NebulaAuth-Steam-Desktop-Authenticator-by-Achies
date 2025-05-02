using SteamLib.Core;
using SteamLib.Exceptions.General;
using SteamLib.Web.Models.GlobalMarketInfo;
using SteamLib.Web.Scrappers.HTML;

namespace SteamLib.Web;

public static class SteamWebApi
{
    /// <summary>
    ///     Login is not required
    /// </summary>
    /// <param name="client"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<GlobalInfoModel> GetMarketGlobalInfo(HttpClient client,
        CancellationToken cancellationToken = default)
    {
        var resp = await client.GetStringAsync(SteamConstants.STEAM_MARKET, cancellationToken);
        try
        {
            return MarketGlobalInfoScrapper.Scrap(resp);
        }
        catch (Exception ex)
        {
            throw new UnsupportedResponseException(resp, ex);
        }
    }
}