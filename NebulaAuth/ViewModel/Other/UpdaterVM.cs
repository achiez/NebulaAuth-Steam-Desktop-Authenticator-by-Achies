using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class UpdaterVM : ObservableObject
{

    public UpdateInfoEventArgs UpdateInfoEventArgs { get; }
    public UpdaterVM(UpdateInfoEventArgs args)
    {
        UpdateInfoEventArgs = args;
    }
}