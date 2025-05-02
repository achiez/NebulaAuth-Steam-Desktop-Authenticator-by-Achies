using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.ViewModel.Other;

public partial class LoginAgainOnImportVM : ObservableObject
{
    public ObservableCollection<MaProxy> Proxies { get; } = new();

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

    public bool IsFormValid => !string.IsNullOrWhiteSpace(Password);
    [ObservableProperty] private bool _mafileHasProxy;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsFormValid))]
    private string _password = null!;

    [ObservableProperty] private bool _savePassword;


    private MaProxy? _selectedProxy;
    private bool _useMafileProxy;
    [ObservableProperty] private string _userName = null!;

    public LoginAgainOnImportVM(Mafile mafile, IEnumerable<MaProxy> proxies)
    {
        UserName = mafile.AccountName;
        MafileHasProxy = mafile.Proxy != null;
        UseMafileProxy = MafileHasProxy;
        Proxies = new ObservableCollection<MaProxy>(proxies);
    }

    public LoginAgainOnImportVM()
    {
    }

    [RelayCommand]
    private void RemoveProxy()
    {
        SelectedProxy = null;
    }
}