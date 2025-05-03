using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.View.Dialogs;
using NebulaAuth.ViewModel;
using NebulaAuth.ViewModel.Other;

namespace NebulaAuth;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainVM();
        Application.Current.MainWindow = this;
        TrayManager.InitializeTray();
        ThemeManager.InitializeTheme();
        Title = Title + " " + Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        Loaded += OnApplicationStarted;
    }

    private async void OnApplicationStarted(object? sender, EventArgs e)
    {
        if (Settings.Instance.IsPasswordSet == false) return;
        await Dispatcher.BeginInvoke(ShowSetPasswordDialog, DispatcherPriority.ContextIdle);
    }

    private async Task ShowSetPasswordDialog()
    {
        var vm = new SetEncryptPasswordVM();
        var dialog = new SetCryptPasswordDialog
        {
            DataContext = vm
        };
        var result = await DialogHost.Show(dialog);
        var pass = vm.Password;
        if (result is true && string.IsNullOrWhiteSpace(pass) == false)
        {
            PHandler.SetPassword(pass);
        }
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.ToString())
        {
            UseShellExecute = true
        });
    }


    #region Dran'n'Drop

    private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (int.TryParse(e.Text, out _) == false) e.Handled = true;
    }

    private void Rectangle_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) == false)
        {
            e.Handled = true;
            return;
        }

        DragNDropPanel.Visibility = Visibility.Visible;
        DragNDropOverlay.Visibility = Visibility.Visible;
    }

    private async void Rectangle_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
        var filePaths = (string[]) e.Data.GetData(DataFormats.FileDrop)!;
        if (filePaths.Length == 0) return;
        if (DataContext is MainVM mainVm)
        {
            await mainVm.AddMafile(filePaths);
        }

        DragNDropPanel.Visibility = Visibility.Hidden;
        DragNDropOverlay.Visibility = Visibility.Hidden;
    }

    private void Rectangle_DragLeave(object sender, DragEventArgs e)
    {
        DragNDropPanel.Visibility = Visibility.Hidden;
        DragNDropOverlay.Visibility = Visibility.Hidden;
    }


    private void DialogHost_DialogOpened(object sender, DialogOpenedEventArgs eventArgs)
    {
        DragNDropBorder.AllowDrop = false;
    }

    private void DialogHost_DialogClosed(object sender, DialogClosedEventArgs eventArgs)
    {
        DragNDropBorder.AllowDrop = true;
    }

    #endregion

}