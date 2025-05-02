using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AchiesUtilities.Web.Models;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;
using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using SteamLib.Api.Mobile;
using SteamLib.Authentication;
using SteamLib.Core.Interfaces;
using SteamLib.Exceptions;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.Utility;
using SteamLib.Web;

namespace NebulaAuth.Model.MAAC;

public partial class PortableMaClient : ObservableObject, IDisposable
{
    private const string LOC_PATH = "MAAC";
    public Mafile Mafile { get; }
    private HttpClient Client { get; }
    private SocketsHttpHandler ClientHandler { get; }
    private DynamicProxy Proxy { get; }

    private readonly CancellationTokenSource _cts = new();
    [ObservableProperty] private bool _autoConfirmMarket;

    [ObservableProperty] private bool _autoConfirmTrades;
    [ObservableProperty] private bool _isError;

    public PortableMaClient(Mafile mafile)
    {
        Mafile = mafile;
        Proxy = new DynamicProxy();
        Proxy.SetData(mafile.Proxy?.Data);
        var pair = ClientBuilder.BuildMobileClient(Proxy, mafile.SessionData);
        Client = pair.Client;
        ClientHandler = pair.Handler;
        UpdateCookies(mafile.SessionData);
        Mafile.PropertyChanged += Mafile_PropertyChanged;
    }

    private void Mafile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Mafile.SessionData))
        {
            UpdateCookies(Mafile.SessionData);
        }
    }


    private void UpdateCookies(IMobileSessionData? sessionData)
    {
        Application.Current.Dispatcher.Invoke(() => IsError = sessionData == null);
        ClientHandler.CookieContainer.ClearAllCookies();
        if (sessionData != null)
        {
            ClientHandler.CookieContainer.SetSteamMobileCookiesWithMobileToken(sessionData);
        }
        else
        {
            ClientHandler.CookieContainer.ClearSteamCookies();
            ClientHandler.CookieContainer.AddMinimalMobileCookies();
            AdmissionHelper.TransferCommunityCookies(ClientHandler.CookieContainer);
        }
    }


    public async Task<int> Confirm()
    {
        Proxy.SetData(Mafile.Proxy?.Data ?? MaClient.DefaultProxy);
        List<Confirmation> conf;
        try
        {
            conf = (await HandleTimerRequest(GetConfirmations)).ToList();
        }
        catch (ApplicationException ex)
        {
            Shell.Logger.Warn(ex, "Timer {accountName}: Error GetConf in timer.", Mafile.AccountName);
            return 0;
        }

        var toConfirm = new List<Confirmation>();
        if (AutoConfirmMarket)
        {
            var market = conf.Where(c => c.ConfType == ConfirmationType.MarketSellTransaction);
            toConfirm.AddRange(market);
        }

        if (AutoConfirmTrades)
        {
            var trade = conf.Where(c => c.ConfType == ConfirmationType.Trade);
            toConfirm.AddRange(trade);
        }

        if (toConfirm.Count == 0) return 0;
        try
        {
            Shell.Logger.Debug("Timer {accountName}: Sending confirmations. Count: {count}", Mafile.AccountName,
                toConfirm.Count);
            var success = await HandleTimerRequest(() => SendConfirmations(toConfirm));
            Shell.Logger.Debug("Timer {accountName}: Confirmation sent: {confirmResult}", Mafile.AccountName, success);
            return toConfirm.Count;
        }
        catch (ApplicationException ex)
        {
            Shell.Logger.Warn(ex, "Timer {accountName}: MultiConf error in Timer.", Mafile.AccountName);
            return 0;
        }
    }


    private async Task<IEnumerable<Confirmation>> GetConfirmations()
    {
        return await SteamMobileConfirmationsApi.GetConfirmations(Client, Mafile, Mafile.SessionData!.SteamId,
            _cts.Token);
    }

    private async Task<bool> SendConfirmations(IEnumerable<Confirmation> confirmations)
    {
        return await SteamMobileConfirmationsApi.SendMultipleConfirmations(Client, confirmations,
            Mafile.SessionData!.SteamId, Mafile, true, _cts.Token);
    }

    private async Task<T> HandleTimerRequest<T>(Func<Task<T>> func)
    {
        Exception innerException;
        try
        {
            return await SessionHandler.Handle(func, Mafile, Chp(), GetTimerPrefix());
        }
        catch (OperationCanceledException ex)
        {
            innerException = ex; //Ignored
        }
        catch (SessionInvalidException ex)
        {
            if (IgnoreTimerErrors())
            {
                Shell.Logger.Warn(
                    "Timer {accountName}: Session error while requesting in timer. Ignored due to IgnorePatchTuesdayErrors setting",
                    Mafile.AccountName);
            }
            else
            {
                Shell.Logger.Warn("Timer {accountName}: Session error while requesting in timer. Timer disabled",
                    Mafile.AccountName);
                SetError();
            }

            innerException = ex;
        }
        catch (Exception ex) when (ExceptionHandler.Handle(ex, GetTimerPrefix()))
        {
            innerException = ex;
        }

        throw new ApplicationException("Swallowed", innerException);
    }


    private bool IgnoreTimerErrors()
    {
        if (Settings.Instance.IgnorePatchTuesdayErrors == false) return false;

        var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        var pstNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pstZone);

        var startTime = pstNow.Date.AddHours(15).AddMinutes(55); // 15:55 PST //22:55 GMT //00:55 GMT+2
        var endTime = pstNow.Date.AddHours(17).AddMinutes(15); // 17:15 PST //00:15 GMT //02:15 GMT+2

        return pstNow.DayOfWeek == DayOfWeek.Tuesday && pstNow >= startTime && pstNow <= endTime;
    }


    private HttpClientHandlerPair Chp()
    {
        return new HttpClientHandlerPair(Client, ClientHandler);
    }

    private static string GetLocalization(string key)
    {
        return LocManager.GetCodeBehindOrDefault(key, LOC_PATH, key);
    }

    private string GetTimerPrefix()
    {
        return GetLocalization("TimerPrefix") + Mafile.AccountName + ": ";
    }

    public void SetError()
    {
        Application.Current.Dispatcher.Invoke(() => IsError = true);
        Shell.Logger.Warn("Timer {accountName}: disabled due to error.", Mafile.AccountName);
        SnackbarController.SendSnackbar(GetTimerPrefix() + GetLocalization("TimerSessionError"));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        Mafile.PropertyChanged -= Mafile_PropertyChanged;
        Client.Dispose();
        ClientHandler.Dispose();
    }
}