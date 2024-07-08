using AchiesUtilities.Collections;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace NebulaAuth.ViewModel.Other;

public partial class ProxyManagerVM : ObservableObject
{
    private const string LOCALIZATION_KEY = "ProxyManagerVM";

    [ObservableProperty] private KeyValuePair<int, ProxyData>? _selectedProxy;
    [ObservableProperty] private string _addProxyField = string.Empty;
    [ObservableProperty] private KeyValuePair<int, ProxyData>? _defaultProxy;
    public ObservableDictionary<int, ProxyData> Proxies => ProxyStorage.Proxies;

    private static readonly Regex IdRegex = new(@"(?:\{(\d+)\})");


    public ProxyManagerVM()
    {
        if (MaClient.DefaultProxy != null)
            DefaultProxy = Proxies.FirstOrDefault(kvp => kvp.Value.Equals(MaClient.DefaultProxy));
    }

    [RelayCommand]
    private void AddProxy()
    {
        if (string.IsNullOrEmpty(AddProxyField)) return;
        if (AddProxyField.Contains(Environment.NewLine))
        {
            var split = AddProxyField.Split(Environment.NewLine);
            var idPresent = (bool?)null;
            var proxies = new List<KeyValuePair<int?, ProxyData>>();
            var i = 0;
            foreach (var s in split.Where(s => string.IsNullOrWhiteSpace(s) == false))
            {
                i++;
                var str = s;
                int? id = null;
                var match = IdRegex.Match(str);
                if (match.Success)
                {
                    id = int.Parse(match.Groups[1].Value);
                    str = IdRegex.Replace(str, "");
                }

                idPresent ??= match.Success;
                if (idPresent.Value != match.Success)
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
                    proxies.Add(new KeyValuePair<int?, ProxyData>(id, proxy));
                }
                else
                {
                    SnackbarController.SendSnackbar(string.Format(GetLocalizationOrDefault("WrongFormatOnLine"), i));
                    return;
                }
            }

            foreach (var kvp in proxies)
            {
                ProxyStorage.SetProxy(kvp.Key, kvp.Value);
            }
        }
        else
        {
            int? id = null;
            var input = AddProxyField;
            if (IdRegex.IsMatch(AddProxyField))
            {
                id = int.Parse(IdRegex.Match(AddProxyField).Groups[1].Value);
                input = IdRegex.Replace(input, "");
            }

            if (ProxyStorage.DefaultScheme.TryParse(input, out var data))
            {
                ProxyStorage.SetProxy(id, data);
            }
            else
            {
                SnackbarController.SendSnackbar(GetLocalizationOrDefault("WrongFormat"));
                return;
            }
        }
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