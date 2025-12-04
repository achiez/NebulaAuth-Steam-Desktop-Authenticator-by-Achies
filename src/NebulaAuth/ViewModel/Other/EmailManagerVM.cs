using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.ViewModel.Other;

public partial class EmailManagerVM : ObservableObject
{
    public ObservableCollection<EmailAccount> EmailAccounts { get; } = EmailStorage.EmailAccounts;

    [ObservableProperty]
    private EmailAccount? _selectedEmailAccount;

    private string? _password;

    public string? Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value) && SelectedEmailAccount != null)
            {
                SelectedEmailAccount.Password = value ?? string.Empty;
            }
        }
    }

    partial void OnSelectedEmailAccountChanged(EmailAccount? value)
    {
        Password = value?.Password ?? string.Empty;
    }

    [RelayCommand]
    private void AddEmailAccount()
    {
        var newAccount = new EmailAccount();
        EmailStorage.AddEmailAccount(newAccount);
        SelectedEmailAccount = newAccount;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEmailAccount))]
    private void RemoveEmailAccount()
    {
        if (SelectedEmailAccount == null) return;
        EmailStorage.RemoveEmailAccount(SelectedEmailAccount);
        SelectedEmailAccount = EmailAccounts.FirstOrDefault();
    }

    private bool CanRemoveEmailAccount()
    {
        return SelectedEmailAccount != null;
    }

    [RelayCommand]
    private void SaveChanges()
    {
        if (SelectedEmailAccount != null && !string.IsNullOrEmpty(Password))
        {
            SelectedEmailAccount.Password = Password;
        }
        EmailStorage.Save();
        SnackbarController.SendSnackbar(LocManager.GetCodeBehindOrDefault("EmailAccountsSaved", "Settings", "Email accounts saved"));
    }
}

