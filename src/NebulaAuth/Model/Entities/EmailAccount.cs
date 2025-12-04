using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace NebulaAuth.Model.Entities;

public partial class EmailAccount : ObservableObject
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _imapServer = "imap.gmail.com";

    [ObservableProperty]
    private int _imapPort = 993;

    [ObservableProperty]
    private bool _useSsl = true;

    [ObservableProperty]
    private string? _displayName;

    [JsonIgnore]
    public string DisplayNameOrEmail => !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : Email;
}

