using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class SetEncryptPasswordVM : ObservableObject
{
    [ObservableProperty] private string? _password;
}