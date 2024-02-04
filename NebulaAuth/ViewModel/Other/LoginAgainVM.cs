using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.ViewModel.Other;

public partial class LoginAgainVM : ObservableObject
{
    [ObservableProperty] private string _password = null!;
    [ObservableProperty] private bool _savePassword;
    [ObservableProperty] private string _userName = null!;

    public LoginAgainVM()
    { }
}