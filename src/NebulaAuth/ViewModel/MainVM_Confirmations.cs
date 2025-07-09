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
    public ObservableCollection<Confirmation> Confirmations { get; } = new();
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
}