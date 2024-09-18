using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class LoginAgainVM : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))] 
    private string _password = null!;

    [ObservableProperty]
    private bool _savePassword;

    [ObservableProperty]
    private string _userName = null!;


    public bool IsFormValid => !string.IsNullOrWhiteSpace(Password);

    public LoginAgainVM()
    { }
}