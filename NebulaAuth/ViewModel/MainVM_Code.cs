using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using SteamLib.SteamMobile;

namespace NebulaAuth.ViewModel;

public partial class MainVM
{
    [ObservableProperty] private string _code = "Code";
    [ObservableProperty] private double _codeProgress;
    private Timer _codeTimer;


    [MemberNotNull(nameof(_codeTimer))]
    private void CreateCodeTimer()
    {
        _codeTimer = new Timer(UpdateCode, null, 0, 1000);
    }


    private void UpdateCode(object? state = null)
    {
        var currentTime = TimeAligner.GetSteamTime();
        var untilChange = currentTime % 30;
        var codeProgress = untilChange / 30D * 100;

        string? code = null;
        if (untilChange == 0 && SelectedMafile != null)
        {
            code = SteamGuardCodeGenerator.GenerateCode(SelectedMafile!.SharedSecret);
        }


        if (Application.Current == null) return;
        Application.Current.Dispatcher.BeginInvoke((string? c) =>
        {
            if (Application.Current.MainWindow?.WindowState == WindowState.Minimized) return;
            CodeProgress = codeProgress;
            if (c != null) Code = c;
        }, DispatcherPriority.DataBind, code);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CopyCode()
    {
        var selectedMafile = SelectedMafile;
        if (selectedMafile == null) return;
        try
        {
            Clipboard.SetText(Code);
            Code = LocManager.GetOrDefault("CodeCopied", "MainWindow", "CodeCopied");
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex);
        }
        finally
        {
            await Task.Delay(200);
            selectedMafile = SelectedMafile;
            if (selectedMafile != null)
                Code = SteamGuardCodeGenerator.GenerateCode(selectedMafile.SharedSecret);
        }
    }
}