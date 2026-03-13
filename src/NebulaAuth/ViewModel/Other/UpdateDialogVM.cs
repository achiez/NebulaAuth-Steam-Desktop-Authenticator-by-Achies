using System;
using System.Diagnostics;
using System.Windows;
using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model.Update;

namespace NebulaAuth.ViewModel.Other;

public partial class UpdateDialogVM : ObservableObject
{
    public UpdateInfoEventArgs Args { get; }
    public ChangelogEntry? Changelog { get; }
    public string? HtmlFallbackUrl { get; }
    public string Version => Args.CurrentVersion;
    public bool HasJsonChangelog => Changelog?.Changes?.Count > 0;

    [ObservableProperty] private bool _showRemindOptions;

    public UpdateDialogVM(UpdateInfoEventArgs args, ChangelogEntry? changelog, string? htmlFallbackUrl)
    {
        Args = args;
        Changelog = changelog;
        HtmlFallbackUrl = htmlFallbackUrl;
    }

    [RelayCommand]
    private void UpdateNow()
    {
        DialogHost.Close(null);
        if (AutoUpdater.DownloadUpdate(Args))
        {
            Application.Current.Shutdown();
        }
    }

    [RelayCommand]
    private void ToggleRemindOptions()
    {
        ShowRemindOptions = !ShowRemindOptions;
    }

    [RelayCommand]
    private void SelectRemindOption(string option)
    {
        var delay = option switch
        {
            "1H" => TimeSpan.FromHours(1),
            "1D" => TimeSpan.FromDays(1),
            "3D" => TimeSpan.FromDays(3),
            "7D" => TimeSpan.FromDays(7),
            _ => TimeSpan.FromDays(1)
        };
        UpdateManager.SetRemindAfter(delay);
        DialogHost.Close(null);
    }

    [RelayCommand]
    private void SkipVersion()
    {
        UpdateManager.SkipVersion(Version);
        DialogHost.Close(null);
    }

    [RelayCommand]
    private void OpenLink(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});
    }
}