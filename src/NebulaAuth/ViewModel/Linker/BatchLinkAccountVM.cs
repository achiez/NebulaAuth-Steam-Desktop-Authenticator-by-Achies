using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AchiesUtilities.Web.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Services;
using NebulaAuth.Utility;
using NLog;
using SteamLib;
using SteamLib.Abstractions;
using SteamLib.Api.Services;
using SteamLib.Authentication;
using SteamLib.Authentication.LoginV2;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Authorization;
using SteamLib.Exceptions.Mobile;
using SteamLib.Factory.Helpers;
using SteamLib.ProtoCore.Enums;
using SteamLib.SteamMobile.AuthenticatorLinker;
using SteamLib.Web;
using SteamLibForked.Abstractions;
using SteamLibForked.Abstractions.Linker;
using SteamLibForked.Exceptions.Authorization;
using SteamLibForked.Models.Session;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NebulaAuth.ViewModel.Linker;

public partial class BatchLinkAccountVM : ObservableObject, IDisposable, ISmsCodeProvider, IPhoneNumberProvider, IEmailProvider
{
    private static readonly Logger Logger = Shell.Logger;
    private static readonly ILogger Logger2 = Shell.ExtensionsLogger;

    int IEmailProvider.RetryCount => 3;

    [ObservableProperty]
    private string _accountsFilePath = string.Empty;

    [ObservableProperty]
    private string _emailsFilePath = string.Empty;

    [ObservableProperty]
    private string _mappingFilePath = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusText = "Готов к обработке";

    [ObservableProperty]
    private int _totalAccounts;

    [ObservableProperty]
    private int _processedAccounts;

    [ObservableProperty]
    private int _successfulAccounts;

    [ObservableProperty]
    private int _failedAccounts;

    private CancellationTokenSource? _cts;
    private Dictionary<string, string> _emailMapping = new();
    private Dictionary<string, EmailAccount> _emailAccounts = new();
    private string? _currentProcessingLogin;

    public BatchLinkAccountVM()
    {
    }

    [RelayCommand]
    private void SelectAccountsFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Text files|*.txt|All files|*.*",
            Title = "Выберите файл с аккаунтами (login:password)"
        };

        if (dialog.ShowDialog() == true)
        {
            AccountsFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void SelectEmailsFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Text files|*.txt|All files|*.*",
            Title = "Выберите файл с почтами (email:password:imap_server:imap_port)"
        };

        if (dialog.ShowDialog() == true)
        {
            EmailsFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void SelectMappingFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Text files|*.txt|All files|*.*",
            Title = "Выберите файл с сопоставлением (login:email)"
        };

        if (dialog.ShowDialog() == true)
        {
            MappingFilePath = dialog.FileName;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartProcessing))]
    private async Task StartProcessing()
    {
        _cts = new CancellationTokenSource();
        IsProcessing = true;
        ProcessedAccounts = 0;
        SuccessfulAccounts = 0;
        FailedAccounts = 0;

        try
        {
            // Загружаем аккаунты
            var accounts = LoadAccounts();
            TotalAccounts = accounts.Count;

            // Загружаем почты если указан файл
            if (!string.IsNullOrWhiteSpace(EmailsFilePath))
            {
                LoadEmails();
            }

            // Загружаем сопоставление если указан файл
            if (!string.IsNullOrWhiteSpace(MappingFilePath))
            {
                LoadMapping();
            }

            StatusText = $"Обработка {TotalAccounts} аккаунтов...";

            // Обрабатываем аккаунты последовательно (1 поток)
            foreach (var account in accounts)
            {
                if (_cts?.Token.IsCancellationRequested == true)
                {
                    StatusText = "Обработка отменена";
                    break;
                }

                await ProcessAccount(account.Login, account.Password);
                ProcessedAccounts++;
            }

            StatusText = $"Завершено. Успешно: {SuccessfulAccounts}, Ошибок: {FailedAccounts}";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during batch linking");
            StatusText = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private bool CanStartProcessing()
    {
        return !IsProcessing && !string.IsNullOrWhiteSpace(AccountsFilePath) && File.Exists(AccountsFilePath);
    }

    [RelayCommand]
    private void CancelProcessing()
    {
        _cts?.Cancel();
        StatusText = "Отмена...";
    }

    private List<(string Login, string Password)> LoadAccounts()
    {
        var accounts = new List<(string, string)>();
        var lines = File.ReadAllLines(AccountsFilePath);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split(':', 2);
            if (parts.Length == 2)
            {
                accounts.Add((parts[0].Trim(), parts[1].Trim()));
            }
        }

        return accounts;
    }

    private void LoadEmails()
    {
        var lines = File.ReadAllLines(EmailsFilePath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split(':');
            if (parts.Length >= 2)
            {
                var email = parts[0].Trim();
                var password = parts[1].Trim();
                var imapServer = parts.Length > 2 ? parts[2].Trim() : "imap.gmail.com";
                var imapPort = parts.Length > 3 && int.TryParse(parts[3].Trim(), out var port) ? port : 993;

                // Шифруем пароль если установлен мастер-пароль
                var encryptedPassword = Settings.Instance.IsPasswordSet ? PHandler.Encrypt(password) : password;

                var emailAccount = new EmailAccount
                {
                    Email = email,
                    Password = encryptedPassword,
                    ImapServer = imapServer,
                    ImapPort = imapPort,
                    UseSsl = true
                };

                // Сохраняем оригинальный пароль для использования в IMAP
                var tempAccount = new EmailAccount
                {
                    Email = email,
                    Password = password, // Нешифрованный для IMAP
                    ImapServer = imapServer,
                    ImapPort = imapPort,
                    UseSsl = true
                };
                _emailAccounts[email] = tempAccount;
                
                // Добавляем в хранилище только если его там еще нет
                if (EmailStorage.GetEmailAccount(email) == null)
                {
                    try
                    {
                        EmailStorage.AddEmailAccount(emailAccount);
                    }
                    catch (InvalidOperationException)
                    {
                        // Уже существует, обновляем
                        EmailStorage.UpdateEmailAccount(emailAccount);
                    }
                }
            }
        }
    }

    private void LoadMapping()
    {
        var lines = File.ReadAllLines(MappingFilePath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split(':');
            if (parts.Length == 2)
            {
                var login = parts[0].Trim();
                var email = parts[1].Trim();
                _emailMapping[login] = email;
            }
        }
    }

    private async Task ProcessAccount(string login, string password)
    {
        try
        {
            _currentProcessingLogin = login;
            StatusText = $"Обработка аккаунта: {login}";

            // Получаем email для этого аккаунта если есть сопоставление
            var email = _emailMapping.TryGetValue(login, out var mappedEmail) ? mappedEmail : null;
            var emailAccount = email != null && _emailAccounts.TryGetValue(email, out var acc) ? acc : null;

            // Создаем клиент для этого аккаунта
            var proxy = new DynamicProxy();
            var pair = ClientBuilder.BuildMobileClient(proxy, null);
            using var client = pair.Client;
            using var handler = pair.Handler;

            // Выбираем прокси если есть дефолтный
            KeyValuePair<int, ProxyData>? selectedProxy = null;
            if (MaClient.DefaultProxy != null)
            {
                var def = ProxyStorage.Proxies.FirstOrDefault(p => p.Value.Equals(MaClient.DefaultProxy));
                if (def.Value != null!)
                {
                    selectedProxy = def;
                }
            }

            proxy.SetData(selectedProxy?.Value);

            // Выполняем логин
            var log = new LoginV2ExecutorOptions(StaticLoginConsumer.Instance, client)
            {
                DeviceDetails = DeviceDetailsDefaultBuilder.GetMobileDefaultDevice(),
                WebsiteId = "Mobile",
                AuthProviders = [this],
                Logger = Logger2
            };

            ISessionData session;
            try
            {
                session = await LoginV2Executor.DoLogin(log, login, password);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to login account {login}");
                FailedAccounts++;
                return;
            }

            if (session is not MobileSessionData msd)
            {
                Logger.Error($"Session is not MobileSessionData for account {login}");
                FailedAccounts++;
                return;
            }

            // Привязываем аккаунт
            var linkOptions = new LinkOptions(client, StaticLoginConsumer.Instance, this, this, this, Storage.WriteBackup, Logger2);
            var linker = new SteamAuthenticatorLinker(linkOptions);
            
            MobileDataExtended result;
            try
            {
                result = await linker.LinkAccount(msd);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to link account {login}");
                FailedAccounts++;
                return;
            }

            // Сохраняем mafile
            var mafile = Mafile.FromMobileDataExtended(result);
            if (selectedProxy.HasValue)
                mafile.Proxy = new MaProxy(selectedProxy.Value.Key, selectedProxy.Value.Value);
            if (Settings.Instance.IsPasswordSet)
                mafile.Password = PHandler.Encrypt(password);
            if (!string.IsNullOrWhiteSpace(email))
                mafile.Email = email;

            Storage.SaveMafile(mafile);
            SuccessfulAccounts++;

            Logger.Info($"Successfully linked account {login}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error processing account {login}");
            FailedAccounts++;
        }
    }

    // Реализация интерфейсов для линковки
    public bool IsSupportedGuardType(ILoginConsumer consumer, EAuthSessionGuardType type)
    {
        return type == EAuthSessionGuardType.EmailCode;
    }

    public async Task UpdateAuthSession(HttpClient authClient, ILoginConsumer loginConsumer,
        UpdateAuthSessionModel model, CancellationToken cancellationToken = default)
    {
        // Получаем email для текущего обрабатываемого аккаунта
        if (string.IsNullOrWhiteSpace(_currentProcessingLogin))
            throw new InvalidOperationException("Не указан логин для получения кода");

        var email = _emailMapping.TryGetValue(_currentProcessingLogin, out var mappedEmail) ? mappedEmail : null;
        var emailAccount = email != null && _emailAccounts.TryGetValue(email, out var acc) ? acc : null;

        if (emailAccount == null)
            throw new InvalidOperationException($"Email не найден для аккаунта {_currentProcessingLogin}");

        // Пытаемся получить код автоматически
        var code = await EmailCodeParser.GetSteamCodeFromEmailAsync(emailAccount, 60, cancellationToken);
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException($"Не удалось получить код с почты {email}");

        var req = AuthRequestHelper.CreateEmailCodeRequest(code, model.ClientId, model.SteamId);
        await AuthenticationServiceApi.UpdateAuthSessionWithSteamGuardCode(authClient, req, cancellationToken);
    }

    public Task<long?> GetPhoneNumber(ILoginConsumer caller)
    {
        return Task.FromResult<long?>(null);
    }

    public Task ConfirmEmailLink(ILoginConsumer caller, EmailConfirmationType confirmationType,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<int> GetSmsCode(ILoginConsumer caller, long? phoneNumber, string? phoneHint)
    {
        throw new NotSupportedException("SMS код не поддерживается в массовой привязке");
    }

    public async ValueTask<string> GetAddAuthenticatorCode(ILoginConsumer caller,
        CancellationToken cancellationToken = default)
    {
        // Получаем email для текущего обрабатываемого аккаунта
        if (string.IsNullOrWhiteSpace(_currentProcessingLogin))
            throw new InvalidOperationException("Не указан логин для получения кода");

        var email = _emailMapping.TryGetValue(_currentProcessingLogin, out var mappedEmail) ? mappedEmail : null;
        var emailAccount = email != null && _emailAccounts.TryGetValue(email, out var acc) ? acc : null;

        if (emailAccount == null)
            throw new InvalidOperationException($"Email не найден для аккаунта {_currentProcessingLogin}");

        var code = await EmailCodeParser.GetSteamCodeFromEmailAsync(emailAccount, 60, cancellationToken);
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException($"Не удалось получить код с почты {email}");

        return code;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

