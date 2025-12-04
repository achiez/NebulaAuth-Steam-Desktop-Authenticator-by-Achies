using System.Windows;
using System.Windows.Controls;
using NebulaAuth.ViewModel.Other;

namespace NebulaAuth.View.Dialogs;

public partial class EmailManagerDialog : UserControl
{
    public EmailManagerDialog()
    {
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is EmailManagerVM vm && sender is PasswordBox passwordBox)
        {
            vm.Password = passwordBox.Password;
        }
    }
}

