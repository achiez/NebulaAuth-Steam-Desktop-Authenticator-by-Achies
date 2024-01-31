using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using NebulaAuth.View.Dialogs;

namespace NebulaAuth.ViewModel;

public partial class MainVM //Other
{
    private void SessionHandlerOnLoginCompleted(object? sender, EventArgs e)
    {
        var currentSession = DialogHost.GetDialogSession(null);
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (currentSession is { Content: WaitLoginDialog, IsEnded: false })
            {
                try
                {
                    currentSession.Close();
                }
                catch
                {
                    //Ignored
                }
            }
        });
    }

    private async void SessionHandlerOnLoginStarted(object? sender, EventArgs e)
    {
        if (DialogHost.IsDialogOpen(null)) return;
        await Application.Current.Dispatcher.BeginInvoke(async () =>
        {
            await DialogHost.Show(new WaitLoginDialog());
        });
    }
}