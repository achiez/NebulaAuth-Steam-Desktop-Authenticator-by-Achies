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

    public Dictionary<ThemeType, string> ThemeTypes => new()
    {
        {ThemeType.Default, "Default"},
        {ThemeType.Black, "Black"},
        {ThemeType.Light, "Light"},
        {ThemeType.Luxury, "Luxury"},
        {ThemeType.Shadcn, "Shadcn"}
    };

    public Dictionary<LocalizationLanguage, string> Languages { get; } = new()
    {
        {LocalizationLanguage.English, "English"},
        {LocalizationLanguage.Russian, "Русский"},
        {LocalizationLanguage.Ukrainian, "Українська"}
    };

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

    [RelayCommand]
    private void ResetThemeDefaults()
    {
        Settings.ResetThemeDefaults();
        OnPropertyChanged(nameof(BackgroundBlur));
        OnPropertyChanged(nameof(BackgroundOpacity));
        OnPropertyChanged(nameof(BackgroundGamma));
        OnPropertyChanged(nameof(LeftOpacity));
        OnPropertyChanged(nameof(RightOpacity));
        OnPropertyChanged(nameof(ApplyBlurBackground));
    }


    #region Theme

    public BackgroundMode BackgroundMode
    {
        get => Settings.BackgroundMode;
        set => Settings.BackgroundMode = value;
    }

    public double BackgroundBlur
    {
        get => Settings.BackgroundBlur;
        set => Settings.BackgroundBlur = value;
    }

    public double BackgroundOpacity
    {
        get => Settings.BackgroundOpacity;
        set => Settings.BackgroundOpacity = value;
    }

    public double BackgroundGamma
    {
        get => Settings.BackgroundGamma;
        set => Settings.BackgroundGamma = value;
    }

    public double LeftOpacity
    {
        get => Settings.LeftOpacity;
        set => Settings.LeftOpacity = value;
    }

    public double RightOpacity
    {
        get => Settings.RightOpacity;
        set => Settings.RightOpacity = value;
    }

    public bool ApplyBlurBackground
    {
        get => Settings.ApplyBlurBackground;
        set => Settings.ApplyBlurBackground = value;
    }

    public ThemeType ThemeType
    {
        get => Settings.ThemeType;
        set => Settings.ThemeType = value;
    }

    #endregion
}