using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;

namespace NebulaAuth.View;

public partial class LinksView : UserControl
{
    public LinksView()
    {
        InitializeComponent();
    }

    private void LinksView_Loaded(object sender, RoutedEventArgs e)
    {
        WebsiteButton.IsEnabled = LinksManager.WebsiteUrl != null;
        DocumentationButton.IsEnabled = LinksManager.DocumentationUrl != null;
    }

    private void Telegram_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://t.me/nebulaauth") {UseShellExecute = true});
    }

    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies")
            {UseShellExecute = true});
    }

    private void Website_Click(object sender, RoutedEventArgs e)
    {
        if (LinksManager.WebsiteUrl != null)
            Process.Start(new ProcessStartInfo(LinksManager.WebsiteUrl) {UseShellExecute = true});
    }

    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        if (LinksManager.DocumentationUrl != null)
            Process.Start(new ProcessStartInfo(LinksManager.DocumentationUrl) {UseShellExecute = true});
    }

    private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        DialogHost.Close(null);
        UpdateManager.CheckForUpdates(true);
    }
}