using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NebulaAuth.Model.Entities;
using Newtonsoft.Json;

namespace NebulaAuth.Model;

public static class EmailStorage
{
    private const string EMAIL_STORAGE_FILE = "emails.json";
    private static ObservableCollection<EmailAccount> _emailAccounts = new();

    public static ObservableCollection<EmailAccount> EmailAccounts
    {
        get => _emailAccounts;
        private set => _emailAccounts = value;
    }

    public static void Initialize()
    {
        if (!File.Exists(EMAIL_STORAGE_FILE))
        {
            EmailAccounts = new ObservableCollection<EmailAccount>();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(EMAIL_STORAGE_FILE);
            var accounts = JsonConvert.DeserializeObject<List<EmailAccount>>(json) ?? new List<EmailAccount>();
            EmailAccounts = new ObservableCollection<EmailAccount>(accounts);
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex, "Failed to load email storage");
            EmailAccounts = new ObservableCollection<EmailAccount>();
        }
    }

    public static void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(EmailAccounts.ToList(), Formatting.Indented);
            File.WriteAllText(EMAIL_STORAGE_FILE, json);
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex, "Failed to save email storage");
        }
    }

    public static void AddEmailAccount(EmailAccount account)
    {
        if (EmailAccounts.Any(e => e.Email.Equals(account.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Email {account.Email} already exists");
        }

        EmailAccounts.Add(account);
        Save();
    }

    public static void RemoveEmailAccount(EmailAccount account)
    {
        EmailAccounts.Remove(account);
        Save();
    }

    public static void UpdateEmailAccount(EmailAccount account)
    {
        var existing = EmailAccounts.FirstOrDefault(e => e.Email.Equals(account.Email, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            var index = EmailAccounts.IndexOf(existing);
            EmailAccounts[index] = account;
            Save();
        }
    }

    public static EmailAccount? GetEmailAccount(string email)
    {
        return EmailAccounts.FirstOrDefault(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}

