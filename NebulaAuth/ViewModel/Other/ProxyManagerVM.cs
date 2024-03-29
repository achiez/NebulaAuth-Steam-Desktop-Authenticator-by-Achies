﻿using AchiesUtilities.Collections;
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
            foreach (var str in split)
            {
                i++;
                int? id = null;
                var match = IdRegex.Match(str);
                if (match.Success) id = int.Parse(match.Groups[1].Value);
                idPresent ??= match.Success;

                if (idPresent.Value != match.Success)
                {
                    SnackbarController.SendSnackbar(GetLocalizationOrDefault("WrongFormatSomeIdsMissing"));
                    return;
                }



                try
                {
                    var proxy = ProxyData.Parse(str, ProxyStorage.FORMAT);
                    if (id != null && proxies.Any(kvp => kvp.Key == id))
                    {
                        SnackbarController.SendSnackbar(string.Format(GetLocalizationOrDefault("DuplicateId"), id));
                        return;
                    }
                    proxies.Add(new KeyValuePair<int?, ProxyData>(id, proxy));
                }
                catch (FormatException)
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

            ProxyData data;
            try
            {
                data = ProxyData.Parse(input, ProxyStorage.FORMAT);
            }
            catch (FormatException)
            {
                SnackbarController.SendSnackbar(GetLocalizationOrDefault("WrongFormat"));
                return;
            }

            ProxyStorage.SetProxy(id, data);


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
        if (SelectedProxy == null) return;
        ProxyStorage.RemoveProxy(SelectedProxy.Value.Key);
        CheckIfDefaultProxyStay();
    }

    [RelayCommand]
    private void SetDefault()
    {
        if (SelectedProxy == null) return;
        DefaultProxy = SelectedProxy;
        MaClient.DefaultProxy = SelectedProxy.Value.Value;
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
    private void CopyProxy(ProxyData? obj)
    {
        if (obj == null) return;
        Clipboard.SetText(obj.ToString(ProxyStorage.FORMAT));
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