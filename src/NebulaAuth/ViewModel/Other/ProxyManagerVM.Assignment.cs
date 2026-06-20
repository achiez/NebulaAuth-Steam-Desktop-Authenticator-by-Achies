using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Mafiles;
using SteamLibForked.Models.SteamIds;

namespace NebulaAuth.ViewModel.Other;

public partial class ProxyManagerVM
{
    public int TotalAccounts => Storage.MaFiles.Count;
    public int TotalProxies => ProxyStorage.Proxies.Count;
    public int AssignedProxies => Storage.MaFiles.Count(m => m.Proxy != null);

    [ObservableProperty] private string? _assignmentInput;
    [ObservableProperty] private string? _assignmentTip;

    /// <summary>
    ///     true = assign a random free proxy to unspecified accounts; false = remove proxy from unspecified accounts.
    /// </summary>
    [ObservableProperty] private bool _unspecifiedAssignRandom = true;

    public void RefreshAssignmentCounters()
    {
        OnPropertyChanged(nameof(TotalAccounts));
        OnPropertyChanged(nameof(TotalProxies));
        OnPropertyChanged(nameof(AssignedProxies));
    }

    [RelayCommand]
    private async Task ApplyAssignment()
    {
        AssignmentTip = null;
        var input = AssignmentInput;
        if (string.IsNullOrWhiteSpace(input)) return;

        var lines = input.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var mafs = Storage.MaFiles.ToList();
        var defaultProxyId = GetDefaultProxyId();

        var success = 0;
        var errors = 0;
        var notFound = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                string login;
                // resolvedProxy: null means unspecified (apply toggle behaviour)
                MaProxy? resolvedProxy;

                var colonIdx = line.IndexOf(':');
                if (colonIdx >= 0)
                {
                    login = line[..colonIdx].Trim();
                    var proxyPart = line[(colonIdx + 1)..].Trim();

                    if (string.IsNullOrEmpty(proxyPart))
                    {
                        resolvedProxy = null; // unspecified
                    }
                    else if (int.TryParse(proxyPart, out var parsedId))
                    {
                        // Mode: login:ID — proxy ID
                        if (!ProxyStorage.Proxies.TryGetValue(parsedId, out var byId))
                        {
                            errors++;
                            continue;
                        }

                        resolvedProxy = new MaProxy(parsedId, byId);
                    }
                    else if (ProxyStorage.DefaultScheme.TryParse(proxyPart, out var parsedProxy) || ProxyStorage.SignAtScheme.TryParse(proxyPart, out parsedProxy))
                    {
                        // Mode: login:proxy_string — find existing or add
                        resolvedProxy = FindOrAddProxy(parsedProxy);
                    }
                    else
                    {
                        errors++;
                        continue;
                    }
                }
                else
                {
                    login = line;
                    resolvedProxy = null; // unspecified
                }

                if (string.IsNullOrWhiteSpace(login))
                {
                    errors++;
                    continue;
                }

                var maf = FindMafile(mafs, login);
                if (maf == null)
                {
                    notFound++;
                    continue;
                }

                var newProxy = resolvedProxy ?? (UnspecifiedAssignRandom
                    ? BuildRandomFreeProxy(defaultProxyId)
                    : null);

                var oldProxyId = maf.Proxy?.Id;
                maf.Proxy = newProxy;
                ProxyAssignmentCache.UpdateAssignment(maf.AccountName, oldProxyId, newProxy?.Id);
                await Storage.UpdateMafileAsync(maf);
                success++;
            }
            catch
            {
                errors++;
            }
        }

        AnyChanges = true;
        RefreshAssignmentCounters();
        AssignmentTip = BuildAssignmentTip(success, notFound, errors);
    }

    [RelayCommand]
    private void ClearAssignmentTip()
    {
        AssignmentTip = null;
    }

    /// <summary>
    ///     Finds a proxy equal to <paramref name="proxyData" /> in storage.
    ///     If not found, adds it and returns the new entry.
    /// </summary>
    private static MaProxy FindOrAddProxy(ProxyData proxyData)
    {
        var existing = ProxyStorage.Proxies.FirstOrDefault(kvp => kvp.Value.Equals(proxyData));
        if (existing.Value != null)
            return new MaProxy(existing.Key, existing.Value);

        ProxyStorage.SetProxy(null, proxyData);

        // Fetch the entry we just added (SetProxy guarantees it was stored)
        var added = ProxyStorage.Proxies.First(kvp => kvp.Value.Equals(proxyData));
        return new MaProxy(added.Key, added.Value);
    }

    private static MaProxy? BuildRandomFreeProxy(int? defaultProxyId)
    {
        var free = ProxyAssignmentCache.GetRandomFreeProxy(ProxyStorage.Proxies, defaultProxyId);
        return free.HasValue ? new MaProxy(free.Value.Key, free.Value.Value) : null;
    }

    private static int? GetDefaultProxyId()
    {
        if (MaClient.DefaultProxy == null) return null;
        var defKvp = ProxyStorage.Proxies.FirstOrDefault(kvp => kvp.Value.Equals(MaClient.DefaultProxy));
        return defKvp.Value != null ? defKvp.Key : null;
    }

    private static Mafile? FindMafile(List<Mafile> mafs, string login)
    {
        SteamId64? steamId = null;
        if (SteamId64.TryParse(login, out var id64))
            steamId = id64;
        if (steamId != null)
            return mafs.FirstOrDefault(m => m.SteamId == steamId || m.AccountName == login);
        return mafs.FirstOrDefault(m =>
            string.Equals(m.AccountName, login, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildAssignmentTip(int success, int notFound, int errors)
    {
        if (success > 0 && errors == 0 && notFound == 0)
            return string.Format(GetLocalizationOrDefault("AssignmentSuccess"), success);
        return string.Format(GetLocalizationOrDefault("AssignmentPartialSuccess"), success, notFound, errors);
    }
}