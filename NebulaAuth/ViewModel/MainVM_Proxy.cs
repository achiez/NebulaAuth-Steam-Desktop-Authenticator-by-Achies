using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.ViewModel;

public partial class MainVM
{
    public ObservableCollection<MaProxy> Proxies { get; }

    public MaProxy? SelectedProxy
    {
        get => _selectedProxy;
        set
        {
            if (SetProperty(ref _selectedProxy, value))
            {
                OnPropertyChanged(nameof(IsDefaultProxy));
                OnProxyChanged();
            }
        }
    }

    public bool IsDefaultProxy => SelectedProxy == null && MaClient.DefaultProxy != null && SelectedMafile != null;
    private bool _handleProxyChange;

    [ObservableProperty] private bool _proxyExist = true;
    private MaProxy? _selectedProxy;

    //TODO: Refactor this method
    private void SetCurrentProxy()
    {
        if (ReferenceEquals(_selectedProxy, SelectedMafile?.Proxy) == false &&
            _selectedProxy?.Equals(SelectedMafile?.Proxy) == false)
            _handleProxyChange = true;

        if (SelectedMafile == null)
        {
            SelectedProxy = null;
            ProxyExist = true;
            return;
        }

        if (SelectedMafile.Proxy == null)
        {
            SelectedProxy = SelectedMafile.Proxy;
        }
        else
        {
            var existed = Proxies.FirstOrDefault(p => p.Id == SelectedMafile.Proxy.Id);


            if (existed == null || existed.Data.Equals(SelectedMafile.Proxy.Data) == false)
            {
                SelectedProxy = SelectedMafile.Proxy;
            }
            else
            {
                SelectedProxy = existed;
            }
        }

        CheckProxyExist();
    }

    private void CheckProxyExist()
    {
        if (SelectedProxy == null)
        {
            ProxyExist = true;
            return;
        }

        var selectedId = SelectedProxy.Id;
        ProxyExist = ProxyStorage.Proxies.TryGetValue(selectedId, out var existedProxy)
                     && SelectedProxy.Data
                         .Equals(existedProxy); //Id is not important in 'Equals()' as we extract it from the dictionary
    }

    [RelayCommand]
    private async Task OpenProxyManager()
    {
        await DialogsController.ShowProxyManager(SelectedProxy);
        var oldSelection = SelectedProxy;
        Proxies.Clear();
        foreach (var kvp in ProxyStorage.Proxies)
        {
            Proxies.Add(new MaProxy(kvp.Key, kvp.Value));
        }

        SelectedProxy = oldSelection;
        SetCurrentProxy();
        OnPropertyChanged(nameof(IsDefaultProxy));
        _handleProxyChange = false;
    }

    [RelayCommand]
    private void RemoveProxy()
    {
        if (SelectedProxy == null) return;
        if (SelectedMafile == null) return;
        SelectedMafile.Proxy = null;
        SelectedProxy = null;
        Storage.UpdateMafile(SelectedMafile);
        ProxyExist = true;
    }

    private void OnProxyChanged()
    {
        if (_handleProxyChange)
        {
            _handleProxyChange = false;
            return;
        }

        if (SelectedMafile == null) return;
        ProxyExist = true;
        SelectedMafile.Proxy = SelectedProxy;
        Storage.UpdateMafile(SelectedMafile);
    }
}