using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NebulaAuth.View;


public partial class LinksView : UserControl
{
    public LinksView()
    {
        InitializeComponent();
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
        Process.Start(new ProcessStartInfo("https://yourwebsite.com") {UseShellExecute = true});
    }

    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://yourwebsite.com") {UseShellExecute = true});
    }
}