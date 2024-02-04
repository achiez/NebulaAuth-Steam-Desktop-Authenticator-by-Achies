using AutoUpdaterDotNET;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Model;
using NebulaAuth.View;
using NebulaAuth.ViewModel.Other;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;


namespace NebulaAuth.Core;

public static class UpdateManager
{
    private const string UPDATE_URL = "https://raw.githubusercontent.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/master/NebulaAuth/update.xml";
    public static void CheckForUpdates()
    {
        string jsonPath = Path.Combine(Environment.CurrentDirectory, "update-settings.json");
        AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
        AutoUpdater.ShowSkipButton = false;
        if (Settings.Instance.AllowAutoUpdate)
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
        AutoUpdater.Start(UPDATE_URL);


    }


    static UpdateManager()
    {
        //AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

    }

    private static async void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (args.IsUpdateAvailable)
            {
                DialogResult dialogResult;
                var dialog = new UpdaterView()
                {
                    DataContext = new UpdaterVM(args)
                };

                await DialogHost.Show(dialog);
                Application.Current.Shutdown();

            }
            else
            {

            }
        }
        else
        {
            if (args.Error is WebException)
            {

            }
            else
            {

            }
        }

    }

    private static void RunUpdate(UpdateInfoEventArgs args)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (AutoUpdater.DownloadUpdate(args))
            {
                Application.Current.Shutdown();
            }
        }, DispatcherPriority.ContextIdle);
    }
}