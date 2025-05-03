using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SteamLib.Core.Interfaces;
using SteamLib.Exceptions;

namespace NebulaAuth.View.Dialogs;

/// <summary>
///     Логика взаимодействия для WaitLoginDialog.xaml
/// </summary>
public partial class WaitLoginDialog : ICaptchaResolver
{
    private TaskCompletionSource<string> _tcs = new();

    public WaitLoginDialog()
    {
        InitializeComponent();
    }

    public async Task<string> Resolve(Uri imageUrl, HttpClient client)
    {
        CaptchaGrid.Visibility = Visibility.Visible;
        var stream = await client.GetStreamAsync(imageUrl);
        return await Application.Current.Dispatcher.Invoke(async () =>
        {
            var image = await LoadImage(stream);
            CaptchaImage.Source = image;
            try
            {
                return await _tcs.Task;
            }
            catch (TaskCanceledException)
            {
                throw new LoginException(LoginError.CaptchaRequired);
            }
        });
    }

    private async Task<BitmapImage> LoadImage(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        var image = new BitmapImage();

        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = ms;
        image.EndInit();
        await stream.DisposeAsync();
        return image;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        _tcs.SetCanceled();
    }

    private void SendCaptchaBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CaptchaTB.Text)) return;
        var oldTcs = _tcs;
        _tcs = new TaskCompletionSource<string>();
        oldTcs.SetResult(CaptchaTB.Text);
    }
}