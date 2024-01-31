using CommunityToolkit.Mvvm.ComponentModel;
using SteamLib.SteamMobile;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;

namespace NebulaAuth.ViewModel;

public partial class MainVM
{
    private Timer _codeTimer;
    [ObservableProperty] private double _codeProgress;
    [ObservableProperty] private string _code;


    [MemberNotNull(nameof(_codeTimer))]
    [MemberNotNull(nameof(_code))]
    private void CreateCodeTimer()
    {
        var currentTime = TimeAligner.GetSteamTime();
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

        if(Application.Current == null) return;
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            CodeProgress = codeProgress;
            if (code != null) Code = code;
        });
    }
}