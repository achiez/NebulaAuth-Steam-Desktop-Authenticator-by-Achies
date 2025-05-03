using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;
using Newtonsoft.Json;

namespace NebulaAuth.Model;

public partial class Settings : ObservableObject
{
    public static Settings Instance { get; }

    static Settings()
    {
        if (File.Exists("settings.json") == false)
        {
            Instance = new Settings();
            Instance.PropertyChanged += SettingsOnPropertyChanged;
            return;
        }

        try
        {
            var json = File.ReadAllText("settings.json");
            var settings = JsonConvert.DeserializeObject<Settings>(json) ?? throw new NullReferenceException();
            Instance = settings;
        }
        catch (Exception ex)
        {
            SnackbarController.SendSnackbar("Ошибка при загрузке настроек. Настройки были сброшены");
            SnackbarController.SendSnackbar(ex.Message);
            Instance = new Settings();
        }

        Instance.PropertyChanged += SettingsOnPropertyChanged;
    }

    private static void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Save();
    }

    public static void Save()
    {
        var json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
        File.WriteAllText("settings.json", json);
    }

    #region Properties

    [ObservableProperty] private BackgroundMode _backgroundMode = BackgroundMode.Default;
    [ObservableProperty] private bool _hideToTray;
    [ObservableProperty] private int _timerSeconds = 60;
    [ObservableProperty] private Color? _backgroundColor;
    [ObservableProperty] private Color? _iconColor;
    [ObservableProperty] private bool _isPasswordSet;
    [ObservableProperty] private LocalizationLanguage _language = LocalizationLanguage.English;
    [ObservableProperty] private bool _legacyMode = true;
    [ObservableProperty] private bool _allowAutoUpdate;
    [ObservableProperty] private bool _useAccountNameAsMafileName;
    [ObservableProperty] private bool _ignorePatchTuesdayErrors;

    #endregion
}

public enum BackgroundMode
{
    Default,
    Custom,
    Color
}