using SteamLibForked.Models.Core;
using System.Collections.ObjectModel;

namespace SteamLib.Core;

public static class SteamDomains
{
    public static IReadOnlyDictionary<SteamDomain, string> Domains { get; } =
        new ReadOnlyDictionary<SteamDomain, string>(
            new Dictionary<SteamDomain, string>
            {
                {SteamDomain.Community, SteamConstants.STEAM_COMMUNITY},
                {SteamDomain.Store, SteamConstants.STEAM_STORE},
                {SteamDomain.Help, SteamConstants.STEAM_HELP},
                {SteamDomain.TV, SteamConstants.STEAM_TV},
                {SteamDomain.Checkout, SteamConstants.STEAM_CHECKOUT},
                {SteamDomain.Login, SteamConstants.STEAM_LOGIN},
                {SteamDomain.API, SteamConstants.STEAM_API}
            });

    public static IReadOnlyDictionary<SteamDomain, Uri> DomainUris { get; }
        = new ReadOnlyDictionary<SteamDomain, Uri>(
            Domains.ToDictionary(x => x.Key, x => new Uri(x.Value))
        );


    public static IEnumerable<SteamDomain> AllDomains { get; } =
    [
        SteamDomain.Community,
        SteamDomain.Store,
        SteamDomain.Help,
        SteamDomain.TV,
        SteamDomain.Checkout,
        SteamDomain.Login,
        SteamDomain.API
    ];

    public static IEnumerable<SteamDomain> AuthDomains { get; } =
    [
        SteamDomain.Community,
        SteamDomain.Store,
        SteamDomain.Help,
        SteamDomain.TV,
        SteamDomain.Checkout
    ];


    public static Uri GetDomainUri(SteamDomain domain)
    {
        if (domain == SteamDomain.Undefined)
            throw new ArgumentException("Invalid domain", nameof(domain));
        return DomainUris[domain];
    }

    public static SteamDomain GetDomain(string domain)
    {
        if (TryGetDomain(domain, out var result))
        {
            return result;
        }

        throw new ArgumentOutOfRangeException(nameof(domain), domain, "Unknown steam domain");
    }

    public static string GetDomain(SteamDomain domain)
    {
        if (domain == SteamDomain.Undefined)
            throw new ArgumentException("Invalid domain", nameof(domain));
        return Domains[domain];
    }

    public static bool TryGetDomain(string domain, out SteamDomain result)
    {
        var uri = new Uri(domain);
        var found = DomainUris.FirstOrDefault(x => x.Value.Host == uri.Host);
        if (found.Value == null)
        {
            result = SteamDomain.Undefined;
            return false;
        }

        result = found.Key;
        return true;
    }
}