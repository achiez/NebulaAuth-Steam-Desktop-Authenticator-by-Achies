using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.View.Dialogs;
using NebulaAuth.ViewModel;
using NebulaAuth.ViewModel.Other;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace NebulaAuth;

public partial class MainWindow
{

    private static string CodeCopiedString => LocManager.GetOrDefault("CodeCopied", "MainWindow", "CodeCopied");
    public MainWindow()
    {
        InitializeComponent();
        base.DataContext = new MainVM();
        Application.Current.MainWindow = this;
        TrayManager.InitializeTray();
        ThemeManager.InitializeTheme();
        base.Title = base.Title + " " + Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        this.Loaded += OnApplicationStarted;
    }

    private async void OnApplicationStarted(object? sender, EventArgs e)
    {
        if (Settings.Instance.IsPasswordSet == false) return;
        await Dispatcher.BeginInvoke(ShowSetPasswordDialog, DispatcherPriority.ContextIdle);
    }

    private async Task ShowSetPasswordDialog()
    {
        var vm = new SetEncryptPasswordVM();
        var dialog = new SetCryptPasswordDialog()
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
    

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var lb = (ListBox)sender;
        lb.SelectedValue = null;
    }

    private async void SteamGuard_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        var tb = (TextBox)sender;
        if (tb.Text == CodeCopiedString) return;
        var code = tb.Text;
        try
        {
            Clipboard.SetText(code);
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex);
            return;
        }
        tb.Text = CodeCopiedString;
        await Task.Delay(200);
        if (tb.Text == CodeCopiedString)
        {
            tb.Text = code;
        }
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
        string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        if (filePaths.Length == 0) return;
        if (this.DataContext is MainVM mainVm)
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

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.ToString())
        {
            UseShellExecute = true
        });
    }


}