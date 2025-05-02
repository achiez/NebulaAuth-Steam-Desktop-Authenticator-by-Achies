using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AchiesUtilities.Collections;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;

namespace NebulaAuth.ViewModel.Other;

public partial class ProxyManagerVM : ObservableObject
{
    private const string LOCALIZATION_KEY = "ProxyManagerVM";

    private static readonly Regex IdRegex = new(@"\{(\d+)\}$", RegexOptions.Compiled);
    public ObservableDictionary<int, ProxyData> Proxies => ProxyStorage.Proxies;
    [ObservableProperty] private string _addProxyField = string.Empty;
    [ObservableProperty] private KeyValuePair<int, ProxyData>? _defaultProxy;

    [ObservableProperty] private KeyValuePair<int, ProxyData>? _selectedProxy;


    public ProxyManagerVM()
    {
        if (MaClient.DefaultProxy != null)
            DefaultProxy = Proxies.FirstOrDefault(kvp => kvp.Value.Equals(MaClient.DefaultProxy));
    }

    [RelayCommand]
    private void AddProxy()
    {
        var input = AddProxyField;
        if (string.IsNullOrEmpty(input)) return;


        var split = input
            .Split(Environment.NewLine)
            .Where(s => string.IsNullOrWhiteSpace(s) == false)
            .ToArray();

        if (split.Length == 0) return;

        bool? idPresent = null;
        var proxies = new List<KeyValuePair<int?, ProxyData>>();
        var i = 0;


        foreach (var s in split.Where(s => string.IsNullOrWhiteSpace(s) == false))
        {
            i++;
            var str = s;
            int? id = null;
            var idMatch = IdRegex.Match(str);
            if (idMatch.Success)
            {
                id = int.Parse(idMatch.Groups[1].Value);
                str = IdRegex.Replace(str, "");
            }

            idPresent ??= idMatch.Success;
            if (idPresent.Value != idMatch.Success)
            {
                SnackbarController.SendSnackbar(GetLocalizationOrDefault("WrongFormatSomeIdsMissing"));
                return;
            }


            if (ProxyStorage.DefaultScheme.TryParse(str, out var proxy))
            {
                if (id != null && proxies.Any(kvp => kvp.Key == id))
                {
                    SnackbarController.SendSnackbar(string.Format(GetLocalizationOrDefault("DuplicateId"), id));
                    return;
                }

                proxies.Add(KeyValuePair.Create(id, proxy));
            }
            else
            {
                if (split.Length == 1)
                {
                    SnackbarController.SendSnackbar(GetLocalizationOrDefault("WrongFormat"));
                    return;
                }

                SnackbarController.SendSnackbar(string.Format(GetLocalizationOrDefault("WrongFormatOnLine"), i));
                return;
            }
        }

        ProxyStorage.SetProxies(proxies);
        ProxyStorage.OrderCollection();
        AddProxyField = string.Empty;
        CheckIfDefaultProxyStay();
    }


    private void CheckIfDefaultProxyStay()
    {
        if (!DefaultProxy.HasValue || Proxies.Any(kvp => kvp.Equals(DefaultProxy.Value))) return;
        DefaultProxy = null;
        MaClient.DefaultProxy = null;
    }

    [RelayCommand]
    private void RemoveProxy()
    {
        var selected = SelectedProxy;
        if (selected == null) return;
        var s = selected.Value;


        KeyValuePair<int, ProxyData>? nextNeighbor = null;
        KeyValuePair<int, ProxyData>? prevNeighbor = null;
        foreach (var id in Proxies.Keys.Order())
        {
            if (id < s.Key)
            {
                prevNeighbor = KeyValuePair.Create(id, Proxies[id]);
            }
            else if (id > s.Key)
            {
                nextNeighbor = KeyValuePair.Create(id, Proxies[id]);
                break;
            }
        }

        ProxyStorage.RemoveProxy(s.Key);
        SelectedProxy = nextNeighbor ?? prevNeighbor;
        CheckIfDefaultProxyStay();
    }

    [RelayCommand]
    private void SetDefault(object? arg)
    {
        if (arg is not KeyValuePair<int, ProxyData> proxy) return;
        DefaultProxy = proxy;
        MaClient.DefaultProxy = proxy.Value;
        ProxyStorage.Save();
    }

    [RelayCommand]
    private void RemoveDefault()
    {
        DefaultProxy = null;
        MaClient.DefaultProxy = null;
        ProxyStorage.Save();
    }

    [RelayCommand]
    private void CopyProxy(ProxyData? data)
    {
        if (data == null) return;
        try
        {
            Clipboard.SetText(ProxyStorage.GetProxyString(data));
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex);
        }
    }

    [RelayCommand]
    private void CopyProxyAddress(ProxyData? obj)
    {
        if (obj == null) return;
        Clipboard.SetText(obj.ToString(ProxyStorage.ADDRESS_FORMAT));
    }

    private static string GetLocalizationOrDefault(string key)
    {
        return LocManager.GetCodeBehindOrDefault(key, LOCALIZATION_KEY, key);
    }
}