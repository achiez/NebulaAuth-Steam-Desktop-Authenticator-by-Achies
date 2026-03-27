using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using NebulaAuth.Helpers;

namespace NebulaAuth.View;

public partial class InventoryWindow : Window
{
    private SteamQRCodeAuthenticator? _qrAuthenticator;

    public InventoryWindow(ViewModel.InventoryVM viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Bring window to front and ensure it stays on top
            this.Activate();
            this.Focus();
            this.Topmost = false; // Allow other windows to come on top after

            if (DataContext is not ViewModel.InventoryVM vm) return;

            vm.StatusMessage = "Initializing browser...";
            vm.IsLoading = true;

            // Per-account user data folder — each account gets its own browser profile
            var accountId = vm.Mafile?.AccountName
                            ?? vm.Mafile?.SessionData?.SteamId.Steam64.ToString()
                            ?? "default";

            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NebulaAuth", "WebView2Profiles", accountId);

            // Enable browser extensions support
            var options = new CoreWebView2EnvironmentOptions
            {
                AreBrowserExtensionsEnabled = true
            };

            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
            await SteamWebView.EnsureCoreWebView2Async(env);

            // Auto-download and load Steam Inventory Helper + any other extensions
            await ExtensionManager.EnsureAndLoadExtensionsAsync(
                SteamWebView.CoreWebView2,
                status =>
                {
                    vm.StatusMessage = status;
                });

            // Initialize QR code authenticator
            _qrAuthenticator = new SteamQRCodeAuthenticator();

            _qrAuthenticator.StatusChanged += (_, args) =>
            {
                vm.StatusMessage = args.Status;
                vm.IsLoading = args.IsLoading;
            };

            await _qrAuthenticator.InitializeAsync(SteamWebView.CoreWebView2, vm.Mafile!);
        }
        catch (Exception ex)
        {
            if (DataContext is ViewModel.InventoryVM vm)
            {
                vm.StatusMessage = $"Error: {ex.Message}";
                vm.IsLoading = false;
            }
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (SteamWebView.CoreWebView2 != null)
            {
                SteamWebView.CoreWebView2.Reload();
                if (DataContext is ViewModel.InventoryVM vm)
                {
                    vm.StatusMessage = "Refreshing...";
                }
            }
        }
        catch (Exception)
        {
            // Refresh failed silently
        }
    }
}
