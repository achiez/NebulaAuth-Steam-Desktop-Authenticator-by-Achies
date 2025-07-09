﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AchiesUtilities.Collections;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Model;

namespace NebulaAuth.ViewModel.Linker;

public partial class LinkAccountAuthStepVM : LinkAccountStepVM
{
    public ObservableDictionary<int, ProxyData> Proxies => ProxyStorage.Proxies;
    public Func<Task> Callback { get; set; } = () => Task.CompletedTask;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string _login = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string _password = string.Empty;

    [ObservableProperty] private KeyValuePair<int, ProxyData>? _selectedProxy;

    public LinkAccountAuthStepVM()
    {
    }

    public LinkAccountAuthStepVM(KeyValuePair<int, ProxyData>? selectedProxy)
    {
        SelectedProxy = selectedProxy;
    }

    public override Task Next()
    {
        return Callback.Invoke();
    }

    public override bool CanExecute()
    {
        return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
    }

    public (string, string, KeyValuePair<int, ProxyData>?) GetState(CancellationToken cancellationToken)
    {
        return (Login, Password, SelectedProxy);
    }

    public override void Cancel()
    {
        Login = string.Empty;
        Password = string.Empty;
    }
}