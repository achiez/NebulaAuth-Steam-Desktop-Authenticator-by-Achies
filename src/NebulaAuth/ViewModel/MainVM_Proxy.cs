using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Mafiles;

namespace NebulaAuth.ViewModel;

public partial class MainVM
{
    public ObservableCollection<MaProxy> Proxies { get; }

    public MaProxy? SelectedProxy
    {
        get => _selectedProxy;
        set => SetProxy(value, false);
    }

    public bool IsDefaultProxy => SelectedProxy == null && MaClient.DefaultProxy != null && SelectedMafile != null;
    private bool _internalProxyRefreshInProgress;

    [ObservableProperty] private bool _proxyExist = true;
    private MaProxy? _selectedProxy;

    private void SetProxy(MaProxy? proxy, bool system)
    {
        if (_internalProxyRefreshInProgress)
        {
            return;
        }

        if (!SetProperty(ref _selectedProxy, proxy, nameof(SelectedProxy)))
        {
            return;
        }

        if (!system && SelectedMafile != null)

        {
            SelectedMafile.Proxy = SelectedProxy;
            Storage.UpdateMafile(SelectedMafile);
        }

        CheckProxyExist();
    }

    [RelayCommand]
    private async Task OpenProxyManager()
    {
        try
        {
            var anyChanges = await DialogsController.ShowProxyManager();
            if (!anyChanges) return;
            _internalProxyRefreshInProgress = true;
            Proxies.Clear();
            foreach (var kvp in ProxyStorage.Proxies)
            {
                Proxies.Add(new MaProxy(kvp.Key, kvp.Value));
            }
        }
        finally
        {
            _internalProxyRefreshInProgress = false;
        }

        CheckProxyExist();
    }

    [RelayCommand(CanExecute = nameof(RemoveProxyCanExecute))]
    private void RemoveProxy(Mafile? mafile)
    {
        mafile ??= SelectedMafile;
        if (mafile?.Proxy == null) return;
        mafile.Proxy = null;
        if (mafile == SelectedMafile)
            SetProxy(null, false); //Not system, triggered by user
    }

    private bool RemoveProxyCanExecute(Mafile? mafile)
    {
        mafile ??= SelectedMafile;
        return mafile is { Proxy: not null };
    }

    private void CheckProxyExist()
    {
        if (SelectedProxy == null)
        {
            ProxyExist = true;
        }
        else
        {
            var selectedId = SelectedProxy.Id;
            ProxyExist = ProxyStorage.Proxies.TryGetValue(selectedId, out var existedProxy)
                         && SelectedProxy.Data
                             .Equals(
                                 existedProxy); //Id is not important in 'Equals()' as we extract it from the dictionary
        }

        OnPropertyChanged(nameof(ProxyExist));
        OnPropertyChanged(nameof(IsDefaultProxy));
    }
}