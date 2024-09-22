using CommunityToolkit.Mvvm.ComponentModel;
using SteamLib.SteamMobile;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NebulaAuth.ViewModel;

public partial class MainVM
{
    private Timer _codeTimer;
    [ObservableProperty] private double _codeProgress;
    [ObservableProperty] private string _code;


    [MemberNotNull(nameof(_codeTimer))]
    private void CreateCodeTimer()
    {
        _codeTimer = new Timer(UpdateCode, null, 0, 1000);
    }

 

    private void UpdateCode(object? state = null)
    {
        var currentTime = TimeAligner.GetSteamTime();
        var untilChange = currentTime - currentTime / 30L * 30L;
        var codeProgress = untilChange / 30D * 100;

        string? code = null;
        if (untilChange == 0 && SelectedMafile != null)
        {
            code = SteamGuardCodeGenerator.GenerateCode(SelectedMafile!.SharedSecret);
        }


        if (Application.Current == null) return;
        Application.Current.Dispatcher.Invoke((string? c) =>
        {
            if (Application.Current.MainWindow?.WindowState == WindowState.Minimized) return;
            CodeProgress = codeProgress;
            if (c != null) Code = c;
        }, DispatcherPriority.Background, code);
    }
}