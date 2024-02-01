using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using SteamLib.Exceptions;
using SteamLib.SteamMobile;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NebulaAuth.View.Dialogs;

namespace NebulaAuth.ViewModel;

public partial class MainVM : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Mafile> _maFiles = Storage.MaFiles;
    public SnackbarMessageQueue MessageQueue => SnackbarController.MessageQueue;

    public Mafile? SelectedMafile
    {
        get => _selectedMafile;
        set => SetMafile(value);
    }
    private Mafile? _selectedMafile;


    public MainVM()
    {
        CreateCodeTimer();
        _confirmTimer = new Timer(ConfirmByTimer, null, TimeSpan.FromSeconds(_timerCheckSeconds), TimeSpan.FromSeconds(_timerCheckSeconds));
        Proxies = new ObservableCollection<MaProxy>(ProxyStorage.Proxies.Select(kvp =>
            new MaProxy(kvp.Key, kvp.Value)));
        Storage.MaFiles.CollectionChanged += MaFilesOnCollectionChanged;
        QueryGroups();
        SessionHandler.LoginStarted += SessionHandlerOnLoginStarted;
        SessionHandler.LoginCompleted += SessionHandlerOnLoginCompleted;
        UpdateManager.CheckForUpdates();
    }
 

    private void SetMafile(Mafile? mafile)
    {
        if (mafile != SelectedMafile)
        {
            _selectedMafile = mafile;
            OnPropertyChanged(nameof(SelectedMafile));
            MaClient.SetAccount(mafile);
            OnPropertyChanged(nameof(ConfirmationsVisible));
            if (Settings.DisableTimersOnChange) OffTimer(dispatcher: false);
            SetCurrentProxy();
            OnPropertyChanged(nameof(IsDefaultProxy));
            if (mafile != null) Code = SteamGuardCodeGenerator.GenerateCode(mafile.SharedSecret);
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
            SnackbarController.SendSnackbar(GetLocalizationOrDefault("SuccessfulLogin"));
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
            SnackbarController.SendSnackbar(GetLocalizationOrDefault("SessionRefreshed"));
        }
        catch (Exception ex) when (ExceptionHandler.Handle(ex))
        {
            Shell.Logger.Error(ex);
        }
    }

    [RelayCommand]
    public async Task LinkAccount()
    {
        OffTimer(false);
        await DialogsController.ShowLinkerDialog();
    }

    [RelayCommand]
    private async Task RemoveAuthenticator()
    {
        var selectedMafile = SelectedMafile;
        if (selectedMafile == null) return;
        if (string.IsNullOrWhiteSpace(selectedMafile.RevocationCode))
        {
            SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Error", "Error") + ": " + GetLocalizationOrDefault("MissingRCode"));
            return;
        }
        try
        {
            if (await DialogsController.ShowConfirmCancelDialog(GetLocalizationOrDefault("ConfirmRemovingAuthenticator")))
            {
                var result = await SessionHandler.Handle(() => MaClient.RemoveAuthenticator(SelectedMafile), SelectedMafile);
                SnackbarController.SendSnackbar(
                    result.Success ? GetLocalizationOrDefault("AuthenticatorRemoved") : GetLocalizationOrDefault("AuthenticatorNotRemoved"));

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
            LoginConfirmationResult res = await SessionHandler.Handle(() => MaClient.ConfirmLoginRequest(SelectedMafile), SelectedMafile);
            if (res.Success)
            {
                SnackbarController.SendSnackbar($"{GetLocalizationOrDefault("ConfirmLoginSuccess")} {res.IP} ({res.Country})");
            }
            else
            {
                string msg = res.Error switch
                {
                    LoginConfirmationError.NoRequests => GetLocalizationOrDefault("ConfirmLoginFailedNoRequests"),
                    LoginConfirmationError.MoreThanOneRequest => GetLocalizationOrDefault("ConfirmLoginFailedMoreThanOneRequest"), //TODO
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
}