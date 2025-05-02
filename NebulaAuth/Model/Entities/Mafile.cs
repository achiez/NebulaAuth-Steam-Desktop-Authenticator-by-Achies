﻿using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Model.MAAC;
using Newtonsoft.Json;
using SteamLib;
using SteamLib.Account;

namespace NebulaAuth.Model.Entities;

[INotifyPropertyChanged]
public partial class Mafile : MobileDataExtended
{
    public MaProxy? Proxy { get; set; }
    public string? Group { get; set; }
    public string? Password { get; set; }

    [JsonIgnore]
    public PortableMaClient? LinkedClient
    {
        get => _linkedClient;
        set => SetProperty(ref _linkedClient, value);
    }

    [JsonIgnore] private PortableMaClient? _linkedClient;


    public void SetSessionData(MobileSessionData? sessionData)
    {
        SessionData = sessionData;
        OnPropertyChanged(nameof(SessionData));
    }

    public static Mafile FromMobileDataExtended(MobileDataExtended data)
    {
        return new Mafile
        {
            AccountName = data.AccountName,
            DeviceId = data.DeviceId,
            IdentitySecret = data.IdentitySecret,
            RevocationCode = data.RevocationCode,
            Secret1 = data.Secret1,
            SerialNumber = data.SerialNumber,
            SessionData = data.SessionData,
            SharedSecret = data.SharedSecret,
            ServerTime = data.ServerTime,
            TokenGid = data.TokenGid,
            Uri = data.Uri,
            SteamId = data.SteamId
        };
    }

    public static Mafile FromMobileDataExtended(MobileDataExtended data, MaProxy? proxy, string? group,
        string? password)
    {
        var result = FromMobileDataExtended(data);
        result.Proxy = proxy;
        result.Group = group;
        result.Password = password;
        return result;
    }
}