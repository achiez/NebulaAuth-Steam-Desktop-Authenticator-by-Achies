using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic.FileIO;

namespace NebulaAuth.ViewModel.Other;

public partial class UpdaterVM : ObservableObject
{

    public UpdateInfoEventArgs UpdateInfoEventArgs { get; }
    public UpdaterVM(UpdateInfoEventArgs args)
    {
        UpdateInfoEventArgs = args;
    }
}