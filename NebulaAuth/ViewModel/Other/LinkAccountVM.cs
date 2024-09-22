using AchiesUtilities.Collections;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using NLog;
using SteamLib;
using SteamLib.Account;
using SteamLib.Authentication;
using SteamLib.Authentication.LoginV2;
using SteamLib.Core.Interfaces;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Mobile;
using SteamLib.ProtoCore.Exceptions;
using SteamLib.SteamMobile.AuthenticatorLinker;
using SteamLib.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace NebulaAuth.ViewModel.Other;

public partial class LinkAccountVM : ObservableObject, IEmailProvider, IPhoneNumberProvider, ISmsCodeProvider
{
    private const string LOCALIZATION_KEY = "LinkVM";
    private static Logger Logger => Shell.Logger;
    private static Microsoft.Extensions.Logging.ILogger Logger2 => Shell.ExtensionsLogger;
    #region Properties


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordFieldVisible))]
    private bool _isLogin;


    [ObservableProperty] private bool _isEmailCode;
    [ObservableProperty] private bool _isPhoneNumber;
    [ObservableProperty] private bool _isEmailConfirmation;
    [ObservableProperty] private bool _isLinkCode;
    [ObservableProperty] private bool _isCompleted;

    [ObservableProperty] private bool _isFieldVisible = true;



    [ObservableProperty] private string _fieldText;
    [ObservableProperty] private string _passFieldText;
    [ObservableProperty] private string _hintText = GetLocalizationOrDefault("EnterLoginAndPassword");

    private TaskCompletionSource<string> _emailCodeTcs = new();
    private TaskCompletionSource<long?> _phoneNumberTcs = new();
    private TaskCompletionSource _emailConfTcs = new();
    private TaskCompletionSource<string> _linkCodeTcs = new();

    private bool isLinkStarted;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProceedCommand))]
    private bool _canProceed = true;
    public bool IsPasswordFieldVisible => !IsLogin;

    private LoginV2ExecutorOptions _loginV2ExecutorOptions;
    private SteamAuthenticatorLinker _linker;
    private MobileSessionData _sessionData;

    #endregion

    #region HttpClient

    private static HttpClient Client { get; }
    private static HttpClientHandler Handler { get; }
    private static DynamicProxy Proxy { get; }
    public ObservableDictionary<int, ProxyData> Proxies => ProxyStorage.Proxies;

    public KeyValuePair<int, ProxyData>? SelectedProxy
    {
        get => _selectedProxy;
        set
        {
            SetProperty(ref _selectedProxy, value);
            SetProxy();
        }
    }
    private KeyValuePair<int, ProxyData>? _selectedProxy;


    #endregion

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public LinkAccountVM()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        if (MaClient.DefaultProxy != null)
        {
            var def = Proxies.FirstOrDefault(p => p.Value.Equals(MaClient.DefaultProxy));
            if (def.Value != null!)
            {
                SelectedProxy = def;
            }
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true, CanExecute = nameof(CanProceed))]
    public async Task Proceed()
    {
        if (IsCompleted)
            DialogHost.Close(null);

        CanProceed = false;

        #region Login

        if (IsLogin == false)
        {
            SetProxy();
            ClearCookies();
            _loginV2ExecutorOptions = new LoginV2ExecutorOptions(LoginV2Executor.NullConsumer, Client)
            {
                DeviceDetails = LoginV2ExecutorOptions.GetMobileDefaultDevice(),
                WebsiteId = "Mobile",
                EmailAuthProvider = this,
                Logger = Logger2
            };

            try
            {
                IsLogin = true;
                var userName = FieldText;
                var pass = PassFieldText;
                FieldText = string.Empty;
                IsFieldVisible = false;
                HintText = string.Empty;
                _sessionData = (MobileSessionData)await LoginV2Executor.DoLogin(_loginV2ExecutorOptions, userName, pass);
                Handler.CookieContainer.SetSteamMobileCookiesWithMobileToken(_sessionData);
                IsEmailCode = true;
            }
            catch (EResultException ex)
            {
                Logger.Error(ex, "Link exception on login");
                HintText = GetLocalizationOrDefault("CantLogin") + ErrorTranslatorHelper.TranslateEResult(ex.Result);
                InvokeOnDispatcher(ResetState);
                return;
            }
            catch (LoginException ex)
            {
                Logger.Error(ex, "Link exception on login");
                HintText = GetLocalizationOrDefault("CantLogin") + ErrorTranslatorHelper.TranslateLoginError(ex.Error);
                InvokeOnDispatcher(ResetState);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Link exception on login");
                HintText = GetLocalizationOrDefault("CantLogin") + ex.Message;
                InvokeOnDispatcher(ResetState);
                return;
            }

        }

        if (IsEmailCode == false)
        {
            _emailCodeTcs.SetResult(FieldText);
            HintText = string.Empty;
            FieldText = string.Empty;
            _emailCodeTcs = new TaskCompletionSource<string>();
            IsFieldVisible = false;
            return;
        }

        #endregion

        if (isLinkStarted)
            goto linkStarted;

        try
        {
            isLinkStarted = true;
            var linkOptions = new LinkOptions(Client, LoginV2Executor.NullConsumer, this,
                null, this, this, backupHandler: Backup, Logger2);
            _linker = new SteamAuthenticatorLinker(linkOptions);
            var result = await _linker.LinkAccount(_sessionData);
            IsLinkCode = true;
            IsCompleted = true;
            var mafile = Mafile.FromMobileDataExtended(result);
            Storage.SaveMafile(mafile);
            File.Delete(Path.Combine("mafiles_backup", mafile.AccountName + ".mafile"));
            HintText =
                string.Format(GetLocalizationOrDefault("MafileLinked"),
                    mafile.RevocationCode,
                    mafile.SessionData?.SteamId.Steam64);

            CanProceed = true;
            return;
        }
        catch (AuthenticatorLinkerException ex)
        {
            Logger.Error(ex, "Link exception");
            HintText = $"{GetLocalizationCommon("Error")}: {ErrorTranslatorHelper.TranslateLinkerError(ex.Error)}";
            InvokeOnDispatcher(ResetState);
            return;

        }
        catch (HttpRequestException ex)
        {
            var msg = ex.StatusCode?.ToString() ?? ex.Message;
            HintText = $"{GetLocalizationCommon("RequestError")}: {msg}";
            InvokeOnDispatcher(ResetState);
            return;
        }
        catch (EResultException ex)
        {
            Logger.Error(ex, "Link exception");
            HintText = GetLocalizationOrDefault("ErrorWithCode") + ErrorTranslatorHelper.TranslateEResult(ex.Result);
            InvokeOnDispatcher(ResetState);
            return;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Link exception");
            HintText = GetLocalizationOrDefault("UnknownError") + ex.Message;
            InvokeOnDispatcher(ResetState);
            return;
        }


    linkStarted:
        if (IsPhoneNumber == false)
        {
            var phoneText = FieldText;
            FieldText = string.Empty;

            if (string.IsNullOrWhiteSpace(phoneText))
            {
                HintText = string.Empty;
                IsFieldVisible = false;
                _phoneNumberTcs.SetResult(null);
                _phoneNumberTcs = new();
                return;
            }

            if (!string.IsNullOrWhiteSpace(phoneText) && phoneText.Length >= 4 && long.TryParse(phoneText, out var phone))
            {
                HintText = string.Empty;
                IsFieldVisible = false;
                _phoneNumberTcs.SetResult(phone);
                _phoneNumberTcs = new();
                return;
            }
            else
            {
                HintText = GetLocalizationOrDefault("PleaseEnterCorrectPhone");
                CanProceed = true;
                return;
            }
        }

        if (IsEmailConfirmation == false)
        {
            HintText = string.Empty;
            _emailConfTcs.SetResult();
            _emailConfTcs = new();
            CanProceed = false;
            return;
        }

        if (IsLinkCode == false)
        {
            var linkCode = FieldText;
            FieldText = string.Empty;
            if (!string.IsNullOrWhiteSpace(linkCode) && linkCode.Length >= 4)
            {
                HintText = string.Empty;
                IsFieldVisible = false;
                _linkCodeTcs.SetResult(linkCode);
                _linkCodeTcs = new();
                return;
            }
            else
            {
                HintText = GetLocalizationOrDefault("PleaseEnterCorrectCode");
                CanProceed = true;
                return;
            }
        }

    }

    [RelayCommand]
    public void ResetProxy()
    {
        if (IsPasswordFieldVisible == false) return;
        SelectedProxy = null;
    }

    private void InvokeOnDispatcher(Action action)
    {
        Application.Current.Dispatcher.BeginInvoke(action, null);
    }

    private void ResetState()
    {
        PassFieldText = string.Empty;
        IsLogin = false;
        IsFieldVisible = true;
        IsEmailCode = false;
        isLinkStarted = false;
        IsPhoneNumber = false;
        IsEmailConfirmation = false;
        CanProceed = true;
        _emailCodeTcs = new TaskCompletionSource<string>();
    }

    private void Backup(MobileDataExtended data)
    {
        if (Directory.Exists("mafiles_backup") == false)
        {
            Directory.CreateDirectory("mafiles_backup");
        }
        var json = Storage.SerializeMafile(data, null);
        File.WriteAllText(Path.Combine("mafiles_backup", data.AccountName + ".mafile"), json);
    }

    #region Providers

    public int MaxRetryCount { get; }
    public Task<string> GetEmailAuthCode(ILoginConsumer caller)
    {
        CanProceed = true;
        HintText = GetLocalizationOrDefault("EnterEmailCode");
        IsFieldVisible = true;
        return _emailCodeTcs.Task;
    }

    public Task<string> GetAddAuthenticatorCode(ILoginConsumer caller)
    {
        IsPhoneNumber = true;
        IsEmailConfirmation = true;
        CanProceed = true;
        HintText = GetLocalizationOrDefault("EnterEmailCode");
        IsFieldVisible = true;
        return _linkCodeTcs.Task;
    }

    public Task ConfirmEmailLink(ILoginConsumer caller, EmailConfirmationType confirmationType)
    {
        IsPhoneNumber = true;
        CanProceed = true;
        HintText = GetLocalizationOrDefault("ClickOnEmailLink");
        return _emailConfTcs.Task;
    }

    public Task<long?> GetPhoneNumber(ILoginConsumer caller)
    {
        CanProceed = true;
        HintText = GetLocalizationOrDefault("EnterPhoneNumber");
        IsFieldVisible = true;
        return _phoneNumberTcs.Task;
    }
    public async Task<int> GetSmsCode(ILoginConsumer caller, long? phoneNumber, string? hint)
    {
        IsPhoneNumber = true;
        IsEmailConfirmation = true;
        CanProceed = true;
        HintText = string.Format(GetLocalizationOrDefault("PhoneHint"), hint);
        IsFieldVisible = true;
        var code = await _linkCodeTcs.Task;
        return int.Parse(code);
    }

    #endregion


    #region Client

    static LinkAccountVM()
    {
        Proxy = new DynamicProxy(null);
        var clientPair = ClientBuilder.BuildMobileClient(Proxy, null);
        Client = clientPair.Client;
        Handler = clientPair.Handler;
    }

    private void ClearCookies()
    {
        Handler.CookieContainer.ClearMobileSessionCookies();
    }

    private void SetProxy()
    {
        Proxy.SetData(SelectedProxy?.Value);
    }

    #endregion

    [RelayCommand]
    private void OpenTroubleshooting()
    {
        const string troubleshootingURI =
            "https://achiez.github.io/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/docs/{0}/LinkingTroubleshooting";

        var localized = string.Format(troubleshootingURI, LocManager.GetCurrentLanguageCode());
        Process.Start(new ProcessStartInfo(new Uri(localized).ToString())
        {
            UseShellExecute = true
        });
    }

    private static string GetLocalizationOrDefault(string key)
    {
        return LocManager.GetCodeBehindOrDefault(key, LOCALIZATION_KEY, key);
    }

    private static string GetLocalizationCommon(string key)
    {
        return LocManager.GetCommonOrDefault(key, key);
    }


}