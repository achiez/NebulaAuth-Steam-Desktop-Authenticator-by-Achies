using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class SdaPasswordDialogVM : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    private string _password = string.Empty;

    public bool IsFormValid => !string.IsNullOrWhiteSpace(Password);
}
