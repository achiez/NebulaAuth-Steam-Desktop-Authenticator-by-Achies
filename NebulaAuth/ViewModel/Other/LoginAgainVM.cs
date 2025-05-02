using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class LoginAgainVM : ObservableObject
{
    public bool IsFormValid => !string.IsNullOrWhiteSpace(Password);

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsFormValid))]
    private string _password = null!;

    [ObservableProperty] private bool _savePassword;

    [ObservableProperty] private string _userName = null!;
}