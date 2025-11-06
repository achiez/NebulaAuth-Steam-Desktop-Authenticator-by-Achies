using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NebulaAuth.ViewModel.Other;

public partial class SettingsVM : ObservableObject
{
    public Settings Settings => Settings.Instance;

    #region SettingsProps

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
        set
        {
            Settings.UseAccountNameAsMafileName = value;
            ApplyRenameSettingCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IgnorePatchTuesdayErrors
    {
        get => Settings.IgnorePatchTuesdayErrors;
        set => Settings.IgnorePatchTuesdayErrors = value;
    }



    #endregion

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

    public bool RippleDisabled
    {
        get => Settings.RippleDisabled;
        set => Settings.RippleDisabled = value;
    }

    #endregion

    [ObservableProperty] private string? _password;
    [ObservableProperty] private string? _renameResultText;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyRenameSettingCommand))]
    private bool _useAccountNameAsMafileNamePreview;

    [ObservableProperty]
    private double _renameMafilesProgress;

    public SettingsVM()
    {
        _useAccountNameAsMafileNamePreview = Settings.UseAccountNameAsMafileName;
    }

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
        OnPropertyChanged(nameof(RippleDisabled));
    }

    [RelayCommand(CanExecute = nameof(ApplyRenameSettingCanExecute))]
    private async Task ApplyRenameSetting()
    {
        RenameResultText = null;
        var targetValue = UseAccountNameAsMafileNamePreview;
        if (UseAccountNameAsMafileName == targetValue) return;
        RenameMafilesProgress = 0;
        Storage.MafileRenameResult? result = null;
        try
        {
            result = await Storage.RenameMafiles(targetValue, new Progress<double>(p => RenameMafilesProgress = p));
            UseAccountNameAsMafileName = targetValue;
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex, "Error while renaming mafiles");
        }
        finally
        {
            RenameMafilesProgress = 0;
        }

        if (result == null)
        {
            RenameResultText = GetLoc("Error");
            return;
        }

        if (result.Total == 0) return;

        if (result.NotRenamed == 0)
        {
            var l = GetLoc("AllRenamed");
            RenameResultText = string.Format(l, result.Total, result.BackupFileName);
            return;
        }
        else
        {
            var l = GetLoc("PartialSuccess");
            RenameResultText =
                string.Format(l, result.Total, result.Renamed, result.Errors, result.AlreadyExist, result.BackupFileName);

        }

        string GetLoc(string key)
        {
            return LocManager.GetCodeBehindOrDefault(key, "SettingsVM", "MafileRenaming", key);
        }
    }

    private bool ApplyRenameSettingCanExecute() => UseAccountNameAsMafileNamePreview != UseAccountNameAsMafileName;
}