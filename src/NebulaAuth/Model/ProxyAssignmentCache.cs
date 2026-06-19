using System;
using System.Collections.Generic;
using System.Linq;
using AchiesUtilities.Collections;
using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.Model;

/// <summary>
///     In-memory cache tracking which proxy IDs are currently assigned to accounts.
///     Provides fast lookup for free proxy selection without iterating all mafiles.
///     Initialized at startup and kept in sync whenever proxy assignments change.
/// </summary>
public static class ProxyAssignmentCache
{
    private static readonly Dictionary<int, HashSet<string>> _proxyToAccounts = new();

    public static void Initialize(IEnumerable<Mafile> mafiles)
    {
        _proxyToAccounts.Clear();
        foreach (var maf in mafiles)
        {
            if (maf.Proxy != null)
                AddInternal(maf.Proxy.Id, maf.AccountName);
        }
    }

    /// <summary>
    ///     Call whenever a proxy assignment on a mafile changes and is persisted to disk.
    /// </summary>
    public static void UpdateAssignment(string? accountName, int? oldProxyId, int? newProxyId)
    {
        if (accountName == null) return;
        if (oldProxyId.HasValue) RemoveInternal(oldProxyId.Value, accountName);
        if (newProxyId.HasValue) AddInternal(newProxyId.Value, accountName);
    }

    public static bool IsProxyFree(int proxyId)
    {
        return !_proxyToAccounts.TryGetValue(proxyId, out var set) || set.Count == 0;
    }

    public static int GetAccountCount(int proxyId)
    {
        return _proxyToAccounts.TryGetValue(proxyId, out var set) ? set.Count : 0;
    }

    /// <summary>
    ///     Returns a random free proxy (not the default proxy), or null if none available.
    /// </summary>
    public static KeyValuePair<int, ProxyData>? GetRandomFreeProxy(
        ObservableDictionary<int, ProxyData> proxies,
        int? excludeDefaultProxyId)
    {
        var free = proxies
            .Where(kvp => kvp.Key != excludeDefaultProxyId && IsProxyFree(kvp.Key))
            .ToList();
        if (free.Count == 0) return null;
        return free[Random.Shared.Next(free.Count)];
    }

    private static void AddInternal(int proxyId, string accountName)
    {
        if (!_proxyToAccounts.TryGetValue(proxyId, out var set))
        {
            set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _proxyToAccounts[proxyId] = set;
        }

        set.Add(accountName);
    }

    private static void RemoveInternal(int proxyId, string accountName)
    {
        if (_proxyToAccounts.TryGetValue(proxyId, out var set))
            set.Remove(accountName);
    }
}