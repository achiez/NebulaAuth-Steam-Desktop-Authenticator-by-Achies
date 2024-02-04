using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Model.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NebulaAuth.ViewModel.Other;

public partial class LoginAgainOnImportVM : ObservableObject
{
    public ObservableCollection<MaProxy> Proxies { get; } = new();
    [ObservableProperty] private string _password = null!;
    [ObservableProperty] private bool _savePassword;
    [ObservableProperty] private string _userName = null!;
    [ObservableProperty] private bool _mafileHasProxy;

    public MaProxy? SelectedProxy
    {
        get => _selectedProxy;
        set
        {
            if (SetProperty(ref _selectedProxy, value) && value != null)
            {
                UseMafileProxy = false;
            }
        }
    }
    public bool UseMafileProxy
    {
        get => _useMafileProxy;
        set
        {
            if (SetProperty(ref _useMafileProxy, value) && value)
            {
                SelectedProxy = null;
            }
        }
    }




    private MaProxy? _selectedProxy;
    private bool _useMafileProxy;
    public LoginAgainOnImportVM(Mafile mafile, IEnumerable<MaProxy> proxies)
    {
        UserName = mafile.AccountName;
        MafileHasProxy = mafile.Proxy != null;
        UseMafileProxy = MafileHasProxy;
        Proxies = new(proxies);
    }

    public LoginAgainOnImportVM()
    { }
}