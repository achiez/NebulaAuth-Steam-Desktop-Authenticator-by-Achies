using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;

namespace NebulaAuth.ViewModel.Other;

public partial class SettingsVM : ObservableObject
{
    public Settings Settings => Settings.Instance;

    public BackgroundMode BackgroundMode
    {
        get => Settings.BackgroundMode;
        set => Settings.BackgroundMode = value;
    }

    public bool HideToTray
    {
        get => Settings.HideToTray;
        set => Settings.HideToTray = value;
    }

    public Dictionary<BackgroundMode, string> BackgroundModes => new()
    {
        {BackgroundMode.Default, LocManager.GetOrDefault("Default", "SettingsDialog", "BackgroundMode", "Default")},
        {BackgroundMode.Custom, LocManager.GetOrDefault("Default", "SettingsDialog", "BackgroundMode", "Custom")},
        {BackgroundMode.Color, LocManager.GetOrDefault("Default", "SettingsDialog", "BackgroundMode", "NoBackground")}
    };

    public Dictionary<LocalizationLanguage, string> Languages { get; } = new()
    {
        {LocalizationLanguage.English, "English"},
        {LocalizationLanguage.Russian, "Русский"},
        {LocalizationLanguage.Ukrainian, "Українська"}
    };

    public Color? BackgroundColor
    {
        get => Settings.BackgroundColor;
        set => Settings.BackgroundColor = value;
    }

    public Color? IconColor
    {
        get => Settings.IconColor;
        set => Settings.IconColor = value;
    }

    public bool UseIcon
    {
        get => IconColor != null;
        set
        {
            if (value == false)
                IconColor = null;
            else
                IconColor = Color.FromRgb(202, 39, 39);

            OnPropertyChanged();
            OnPropertyChanged(nameof(IconColor));
        }
    }

    public bool UseBackground
    {
        get => BackgroundColor != null;
        set
        {
            if (value == false)
                BackgroundColor = null;
            else
                BackgroundColor = Color.FromRgb(202, 39, 39);

            OnPropertyChanged();
            OnPropertyChanged(nameof(BackgroundColor));
        }
    }


    public LocalizationLanguage Language
    {
        get => Settings.Language;
        set
        {
            Settings.Language = value;
            LocManager.SetApplicationLocalization(value);
        }
    }

    public bool LegacyMode
    {
        get => Settings.LegacyMode;
        set => Settings.LegacyMode = value;
    }

    public bool AllowAutoUpdate
    {
        get => Settings.AllowAutoUpdate;
        set => Settings.AllowAutoUpdate = value;
    }

    public bool UseAccountNameAsMafileName
    {
        get => Settings.UseAccountNameAsMafileName;
        set => Settings.UseAccountNameAsMafileName = value;
    }

    public bool IgnorePatchTuesdayErrors
    {
        get => Settings.IgnorePatchTuesdayErrors;
        set => Settings.IgnorePatchTuesdayErrors = value;
    }

    [ObservableProperty] private string? _password;


    [RelayCommand]
    private void SetPassword()
    {
        Settings.IsPasswordSet = PHandler.SetPassword(Password);
    }
}