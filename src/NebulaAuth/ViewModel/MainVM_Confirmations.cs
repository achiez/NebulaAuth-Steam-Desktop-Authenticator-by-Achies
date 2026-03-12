using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using SteamLib.SteamMobile.Confirmations;

namespace NebulaAuth.ViewModel;

public partial class MainVM //Confirmations
{
    public ObservableCollection<Confirmation> Confirmations { get; } = [];
    public bool ConfirmationsVisible => SelectedMafile == _confirmationsLoadedForMafile;
    private Mafile? _confirmationsLoadedForMafile;

    [RelayCommand]
    private async Task GetConfirmations()
    {
        if (SelectedMafile == null) return;
        var maf = SelectedMafile;
        List<Confirmation> conf;
        try
        {
            var confI = await SessionHandler.Handle(() => MaClient.GetConfirmations(SelectedMafile), SelectedMafile);
            conf = confI.ToList();
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex))
        {
            return;
        }


        _confirmationsLoadedForMafile = maf;
        OnPropertyChanged(nameof(ConfirmationsVisible));
        Confirmations.Clear();
        var marketConfirmations = conf
            .Where(c => c.ConfType == ConfirmationType.MarketSellTransaction)
            .Cast<MarketConfirmation>()
            .ToList();

        if (marketConfirmations.Count > 1)
        {
            var indexOfLast = conf.IndexOf(marketConfirmations.First());
            foreach (var mCon in marketConfirmations)
            {
                conf.Remove(mCon);
            }

            var mConf = new MarketMultiConfirmation(marketConfirmations);
            conf.Insert(indexOfLast, mConf);
        }

        foreach (var con in conf)
        {
            Confirmations.Add(con);
        }
    }

    [RelayCommand]
    private Task Confirm(Confirmation? confirmation)
    {
        if (SelectedMafile == null || confirmation == null) return Task.CompletedTask;
        return SendConfirmation(SelectedMafile, confirmation, true);
    }

    [RelayCommand]
    private Task Cancel(Confirmation? confirmation)
    {
        if (SelectedMafile == null || confirmation == null) return Task.CompletedTask;
        return SendConfirmation(SelectedMafile, confirmation, false);
    }

    [RelayCommand]
    private Task ConfirmMarketItem(object? commandParameter)
    {
        if (SelectedMafile == null) return Task.CompletedTask;
        if (!TryParseMarketItemCommandParameter(commandParameter, out var parent, out var item))
            return Task.CompletedTask;
        return SendMarketItemConfirmation(SelectedMafile, parent, item, true);
    }

    [RelayCommand]
    private Task CancelMarketItem(object? commandParameter)
    {
        if (SelectedMafile == null) return Task.CompletedTask;
        if (!TryParseMarketItemCommandParameter(commandParameter, out var parent, out var item))
            return Task.CompletedTask;
        return SendMarketItemConfirmation(SelectedMafile, parent, item, false);
    }

    private async Task SendConfirmation(Mafile mafile, Confirmation confirmation, bool confirm)
    {
        bool result;
        try
        {
            if (confirmation is MarketMultiConfirmation multi)
            {
                result = await MaClient.SendMultipleConfirmation(mafile, multi.Confirmations, confirm);
            }
            else
            {
                result = await MaClient.SendConfirmation(mafile, confirmation, confirm);
            }
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex))
        {
            return;
        }

        if (result)
        {
            Confirmations.Remove(confirmation);
        }
        else
        {
            SnackbarController.SendSnackbar(GetLocalization("ConfirmationError"));
        }
    }

    private async Task SendMarketItemConfirmation(Mafile mafile, MarketMultiConfirmation parent,
        MarketConfirmation item, bool confirm)
    {
        bool result;
        try
        {
            result = await MaClient.SendConfirmation(mafile, item, confirm);
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex))
        {
            return;
        }

        if (!result)
        {
            SnackbarController.SendSnackbar(GetLocalization("ConfirmationError"));
            return;
        }

        // Update parent group after a successful per-item action
        parent.Confirmations.Remove(item);

        if (parent.Confirmations.Count == 0)
        {
            Confirmations.Remove(parent);
            return;
        }

        if (parent.Confirmations.Count == 1)
        {
            var remaining = parent.Confirmations[0];
            var idx = Confirmations.IndexOf(parent);
            if (idx >= 0)
            {
                Confirmations[idx] = remaining;
            }
            else
            {
                Confirmations.Add(remaining);
            }
            return;
        }

        parent.Time = parent.Confirmations.FirstOrDefault()?.Time ?? parent.Time;
    }

    private static bool TryParseMarketItemCommandParameter(object? commandParameter,
        out MarketMultiConfirmation parent,
        out MarketConfirmation item)
    {
        parent = null!;
        item = null!;
        if (commandParameter is not object[] values || values.Length < 2)
            return false;

        parent = values[0] as MarketMultiConfirmation ?? null!;
        item = values[1] as MarketConfirmation ?? null!;
        return parent != null && item != null;
    }
}