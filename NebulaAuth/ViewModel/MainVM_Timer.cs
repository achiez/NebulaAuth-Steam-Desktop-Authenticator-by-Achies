using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using SteamLib.Exceptions;
using SteamLib.SteamMobile.Confirmations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NebulaAuth.ViewModel;

public partial class MainVM //Timer
{
    private readonly Timer _confirmTimer;
    [ObservableProperty] private bool _marketTimerEnabled;
    [ObservableProperty] private bool _tradeTimerEnabled;
    private int _timerCheckSeconds = Settings.Instance.TimerSeconds;
    public int TimerCheckSeconds
    {
        get => _timerCheckSeconds;
        set => SetTimer(value);
    }


    private async void ConfirmByTimer(object? state = null)
    {
        if (SelectedMafile == null)
            return;
        var selected = SelectedMafile;
        if (InterruptTimer(selected, null)) return;
        List<Confirmation> conf;
        try
        {
            conf = (await HandleTimerRequest(() => MaClient.GetConfirmations(selected), SelectedMafile)).ToList();
        }
        catch (ApplicationException ex)
        {
            Shell.Logger.Warn(ex, "Error GetConf in timer.");
            return;
        }
        if (InterruptTimer(selected, conf)) return;
        var toConfirm = new List<Confirmation>();
        if (MarketTimerEnabled)
        {
            var market = conf.Where(c => c.ConfType == ConfirmationType.MarketSellTransaction);
            toConfirm.AddRange(market);
        }
        if (TradeTimerEnabled)
        {
            var trade = conf.Where(c => c.ConfType == ConfirmationType.Trade);
            toConfirm.AddRange(trade);
        }
        if (InterruptTimer(selected, toConfirm)) return;
        bool result;
        try
        {
            Shell.Logger.Debug("Sending confirmations. Count: {count}", toConfirm.Count);
            result = await HandleTimerRequest(() => MaClient.SendMultipleConfirmation(SelectedMafile, toConfirm, confirm: true), SelectedMafile);
        }
        catch (ApplicationException ex)
        {
            Shell.Logger.Warn(ex, "MultiConf error in Timer.");
            return;
        }
        SnackbarController.SendSnackbar(result ? $"{GetLocalizationOrDefault("TimerConfirmed")} {toConfirm.Count}" : GetLocalizationOrDefault("TimerNotConfirmed"));
    }

    private bool InterruptTimer(Mafile cachedValue, List<Confirmation>? confirmations)
    {
        return SelectedMafile == null
               || (MarketTimerEnabled || TradeTimerEnabled) == false
               || SelectedMafile != cachedValue
               || confirmations is { Count: 0 };

    }

    private void OffTimer(bool dispatcher)
    {
        if (dispatcher)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(OffTimerInline));
        }
        else
        {
            OffTimerInline();
        }
        return;

        void OffTimerInline()
        {
            MarketTimerEnabled = false;
            TradeTimerEnabled = false;
        }
    }

    private async Task<T> HandleTimerRequest<T>(Func<Task<T>> func, Mafile mafile)
    {
        Exception innerException;
        try
        {
            return await SessionHandler.Handle(func, mafile);
        }
        catch (SessionInvalidException ex)
        {
            Shell.Logger.Warn("Session error while requesting in timer. Timer disabled");
            SnackbarController.SendSnackbar(GetLocalizationOrDefault("TimerSessionError"));
            OffTimer(dispatcher: true);
            innerException = ex;
        }
        catch (Exception ex) when (ExceptionHandler.Handle(ex, GetLocalizationOrDefault("TimerPrefix")))
        {
            innerException = ex;
        }
        throw new ApplicationException("Swallowed", innerException);
    }


  

    private void SetTimer(int value)
    {
        if (_timerCheckSeconds == value) return;
        if (value < 10)
        {
            SnackbarController.SendSnackbar(GetLocalizationOrDefault("TimerTooFast"));
            OnPropertyChanged(nameof(TimerCheckSeconds));
            return;
        }

        Settings.TimerSeconds = value;
        _timerCheckSeconds = value;
        OnPropertyChanged(nameof(TimerCheckSeconds));
        _confirmTimer.Change(value * 1000, value * 1000);
    }
}