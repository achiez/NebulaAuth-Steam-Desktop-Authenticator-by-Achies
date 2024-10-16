using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.MAAC;

namespace NebulaAuth.ViewModel;

public partial class MainVM //MAAC
{
    public bool MaacDisplay
    {
        get => _maacDisplay;
        set
        {
            if (SetProperty(ref _maacDisplay, value))
            {
                SwitchMAACDispaly(value);
            }
        }
    }
    private bool _maacDisplay;

    public bool MarketTimerEnabled
    {
        get => SelectedMafile?.LinkedClient?.AutoConfirmMarket ?? false;
        set => SetMarketTimer(value);
    }
    public bool TradeTimerEnabled
    {
        get => SelectedMafile?.LinkedClient?.AutoConfirmTrades ?? false;
        set => SetTradeTimer(value);
    }

    public int TimerCheckSeconds
    {
        get => Settings.Instance.TimerSeconds;
        set => SetTimer(value);
    }

    private void SetMarketTimer(bool value)
    {
        var selectedMafile = SelectedMafile;
        if (selectedMafile == null) return;
        if (value && selectedMafile.LinkedClient == null)
        {
            MultiAccountAutoConfirmer.TryAddToConfirm(selectedMafile);
        }
        if (!value && selectedMafile is { LinkedClient.AutoConfirmTrades: false })
        {
            MultiAccountAutoConfirmer.RemoveFromConfirm(selectedMafile);
        }

        if (selectedMafile.LinkedClient != null)
        {
            selectedMafile.LinkedClient.AutoConfirmMarket = value;
        }
    }

    private void SetTradeTimer(bool value)
    {
        var selectedMafile = SelectedMafile;
        if (selectedMafile == null) return;
        if (value && selectedMafile.LinkedClient == null)
        {
            MultiAccountAutoConfirmer.TryAddToConfirm(selectedMafile);
        }

        if (!value && selectedMafile is { LinkedClient.AutoConfirmMarket: false })
        {
            MultiAccountAutoConfirmer.RemoveFromConfirm(selectedMafile);
        }

        if (selectedMafile.LinkedClient != null)
        {
            selectedMafile.LinkedClient.AutoConfirmTrades = value;
        }
    }

    private void SetTimer(int value)
    {
        var timerCheckSeconds = Settings.TimerSeconds;
        if (timerCheckSeconds == value) return;
        if (timerCheckSeconds < 10)
        {
            timerCheckSeconds = 10; //Guard
        }
        if (value < 10)
        {
            value = timerCheckSeconds;
            SnackbarController.SendSnackbar(GetLocalization("TimerTooFast"));
        }
        Settings.TimerSeconds = value;
        OnPropertyChanged(nameof(TimerCheckSeconds));
        if (timerCheckSeconds != TimerCheckSeconds)
            SnackbarController.SendSnackbar(GetLocalization("TimerChanged"));
    }

    [RelayCommand]
    public void RemoveFromMAAC(object? maf)
    {
        if (maf is not Mafile mafile) return;
        MultiAccountAutoConfirmer.RemoveFromConfirm(mafile);
    }


    private void SwitchMAACDispaly(bool newValue)
    {
        if (newValue)
        {
            MaFiles = MultiAccountAutoConfirmer.Clients;
        }
        else
        {
            PerformQuery();
        }
    }
}