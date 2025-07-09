using System.Threading.Tasks;
using System.Windows;

namespace NebulaAuth.View.Dialogs;

/// <summary>
///     Логика взаимодействия для WaitLoginDialog.xaml
/// </summary>
public partial class WaitLoginDialog
{
    private TaskCompletionSource<string> _tcs = new();

    public WaitLoginDialog()
    {
        InitializeComponent();
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