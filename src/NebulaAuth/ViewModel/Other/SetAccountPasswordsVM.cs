using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Model;
using SteamLibForked.Models.SteamIds;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NebulaAuth.ViewModel.Other;

public partial class SetAccountPasswordsVM : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetPasswordsCommand))]
    private bool _isEncryptionPasswordSet;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetEncryptionPasswordCommand))]
    private string? _encryptionPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetPasswordsCommand))]
    private string? _accountsPasswords;
    [ObservableProperty] private string? _tip;

    public SetAccountPasswordsVM()
    {
        _isEncryptionPasswordSet = PHandler.IsPasswordSet;
    }


    [RelayCommand(CanExecute = nameof(SetEncryptionPasswordCanExecute))]
    private void SetEncryptionPassword()
    {
        if (string.IsNullOrWhiteSpace(EncryptionPassword))
        {
            return;
        }
        Settings.Instance.IsPasswordSet = PHandler.SetPassword(EncryptionPassword);
        IsEncryptionPasswordSet = PHandler.IsPasswordSet;
        EncryptionPassword = null;
    }

    private bool SetEncryptionPasswordCanExecute() => !string.IsNullOrWhiteSpace(EncryptionPassword);


    [RelayCommand(CanExecute = nameof(SetPasswordsCanExecute))]
    private async Task SetPasswords()
    {
        Tip = null;
        var input = AccountsPasswords;
        if (string.IsNullOrWhiteSpace(input)) return;
        var lines = input.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        var success = 0;
        var errors = 0;
        var notFound = 0;

        var mafs = Storage.MaFiles.ToList();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var split = line.Split(":", 2);
                if (split.Length != 2)
                {
                    errors++;
                    continue;
                }

                string login = split[0];
                string password = split[1];
                SteamId64? steamId = null;

                if (SteamId64.TryParse(login, out var id64))
                {
                    steamId = id64;
                }

                var maf = steamId != null
                    ? mafs.FirstOrDefault(m => m.SteamId == steamId || m.AccountName == login)
                    : mafs.FirstOrDefault(m => string.Equals(m.AccountName, login, StringComparison.OrdinalIgnoreCase));

                if (maf == null)
                {
                    notFound++;
                    continue;
                }

                maf.Password = PHandler.Encrypt(password);
                await Storage.UpdateMafileAsync(maf);
                success++;
            }
            catch
            {
                errors++;
            }
        }

        if (success > 0 && errors == 0 && notFound == 0)
        {
            Tip = "Успех";
        }
        else
        {
            Tip = $"Успешно: {success}, Не найдено: {notFound}, Ошибки: {errors}";
        }

    }

    private bool SetPasswordsCanExecute()
    {
        return !string.IsNullOrWhiteSpace(AccountsPasswords) && IsEncryptionPasswordSet;
    }

    [RelayCommand]
    private void ClearTip()
    {
        Tip = null;
    }
}