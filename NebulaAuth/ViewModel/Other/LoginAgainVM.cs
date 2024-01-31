using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class LoginAgainVM : ObservableObject
{
    [ObservableProperty] private string _password = null!;
    [ObservableProperty] private bool _savePassword;
    [ObservableProperty] private string _userName = null!;
}