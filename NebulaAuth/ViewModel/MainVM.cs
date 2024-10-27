using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using NebulaAuth.View.Dialogs;
using SteamLib.Exceptions;
using SteamLib.SteamMobile;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace NebulaAuth.ViewModel;

public partial class MainVM : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Mafile> _maFiles = Storage.MaFiles;

    [UsedImplicitly]
    public SnackbarMessageQueue MessageQueue => SnackbarController.MessageQueue;


    public Mafile? SelectedMafile
    {
        get => _selectedMafile;
        set => SetMafile(value);
    }
    private Mafile? _selectedMafile;

    public bool IsMafileSelected => SelectedMafile != null;


    public MainVM()
    {
        CreateCodeTimer();
        Proxies = new ObservableCollection<MaProxy>(ProxyStorage.Proxies.Select(kvp =>
            new MaProxy(kvp.Key, kvp.Value)));
        Storage.MaFiles.CollectionChanged += MaFilesOnCollectionChanged;
        QueryGroups();
        UpdateManager.CheckForUpdates();
        if (Storage.DuplicateFound > 0)
        {
            SnackbarController.SendSnackbar(
                GetLocalization("DuplicateMafilesFound") + " " + Storage.DuplicateFound,
                TimeSpan.FromSeconds(4));
        }
    }


    private void SetMafile(Mafile? mafile)
    {
        if (mafile != SelectedMafile)
        {
            _selectedMafile = mafile;
            OnPropertyChanged(nameof(SelectedMafile));
            OnPropertyChanged(nameof(TradeTimerEnabled));
            OnPropertyChanged(nameof(MarketTimerEnabled));
            MaClient.SetAccount(mafile);
            OnPropertyChanged(nameof(ConfirmationsVisible));
            SetCurrentProxy();
            OnPropertyChanged(nameof(IsDefaultProxy));
            if (mafile != null) Code = SteamGuardCodeGenerator.GenerateCode(mafile.SharedSecret);
            OnPropertyChanged(nameof(IsMafileSelected));
        }
    }


    [RelayCommand]
    public async Task LoginAgain()
    {
        if (SelectedMafile == null)
        {
            return;
        }
        var loginAgainVm = await DialogsController.ShowLoginAgainDialog(SelectedMafile.AccountName);
        if (loginAgainVm == null)
        {
            return;
        }

        var password = loginAgainVm.Password;
        var waitDialog = new WaitLoginDialog();
        var wait = DialogHost.Show(waitDialog);
        try
        {
            await MaClient.LoginAgain(SelectedMafile, password, loginAgainVm.SavePassword, waitDialog);
            SnackbarController.SendSnackbar(GetLocalization("SuccessfulLogin"));
        }
        catch (LoginException ex)
        {
            SnackbarController.SendSnackbar(ErrorTranslatorHelper.TranslateLoginError(ex.Error), TimeSpan.FromSeconds(1.5));
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex))
        {
            Shell.Logger.Error(ex);
        }
        finally
        {
            DialogsController.CloseDialog();
            await wait;
        }
    }
    private void MaFilesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SearchText = string.Empty;
    }


    [RelayCommand]
    private async Task RefreshSession()
    {
        if (SelectedMafile == null) return;
        try
        {
            await MaClient.RefreshSession(SelectedMafile);
            SnackbarController.SendSnackbar(GetLocalization("SessionRefreshed"));
        }
        catch (Exception ex) when (ExceptionHandler.Handle(ex))
        {
            Shell.Logger.Error(ex);
        }
    }

    [RelayCommand]
    public async Task LinkAccount()
    {
        await DialogsController.ShowLinkerDialog();
    }

    [RelayCommand]
    private async Task RemoveAuthenticator()
    {
        var selectedMafile = SelectedMafile;
        if (selectedMafile == null) return;
        if (string.IsNullOrWhiteSpace(selectedMafile.RevocationCode))
        {
            SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Error", "Error") + ": " + GetLocalization("MissingRCode"));
            return;
        }
        try
        {
            if (await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmRemovingAuthenticator")))
            {
                var result = await SessionHandler.Handle(() => MaClient.RemoveAuthenticator(selectedMafile), selectedMafile);
                SnackbarController.SendSnackbar(
                    result.Success ? GetLocalization("AuthenticatorRemoved") : GetLocalization("AuthenticatorNotRemoved"));

                if (result.Success)
                {
                    Storage.MoveToRemoved(selectedMafile);
                    MaFiles.Remove(selectedMafile);
                }
            }
            else
            {
                SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Canceled", "Canceled"));
            }
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex, handleAllExceptions: true))
        {
            Shell.Logger.Error(ex);
        }
    }

    [RelayCommand]
    private async Task ConfirmLogin()
    {
        if (SelectedMafile == null) return;

        try
        {
            var res = await SessionHandler.Handle(() => MaClient.ConfirmLoginRequest(SelectedMafile), SelectedMafile);
            if (res.Success)
            {
                SnackbarController.SendSnackbar($"{GetLocalization("ConfirmLoginSuccess")} {res.IP} ({res.Country})");
            }
            else
            {
                string msg = res.Error switch
                {
                    LoginConfirmationError.NoRequests => GetLocalization("ConfirmLoginFailedNoRequests"),
                    LoginConfirmationError.MoreThanOneRequest => GetLocalization("ConfirmLoginFailedMoreThanOneRequest"), //TODO
                    _ => throw new ArgumentOutOfRangeException()
                };
                SnackbarController.SendSnackbar(msg);
            }
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex, handleAllExceptions: true))
        {
            Shell.Logger.Error(ex);
        }
    }


    private static string GetLocalization(string key)
    {
        const string locPath = "MainVM";
        return LocManager.GetCodeBehindOrDefault(key, locPath, key);
    }
}