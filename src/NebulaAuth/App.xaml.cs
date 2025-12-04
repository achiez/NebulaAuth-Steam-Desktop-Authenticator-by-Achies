using System;
using System.Windows;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Exceptions;
using NebulaAuth.Model.MAAC;

namespace NebulaAuth;

public partial class App
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            var splashScreen = new SplashScreen("Theme\\SplashScreen.png");
            splashScreen.Show(false, true);
            base.OnStartup(e);
            LocManager.Init();
            LocManager.SetApplicationLocalization(Settings.Instance.Language);
            Shell.Initialize();
            var threads = Environment.ProcessorCount > 0 ? Environment.ProcessorCount : 1;
            await Storage.Initialize(threads);
            MAACStorage.Initialize();
            EmailStorage.Initialize();
            var mainWindow = new MainWindow();
            Current.MainWindow = mainWindow;
            mainWindow.Show();
            splashScreen.Close(TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            var msg = ex.ToString();
            if (ex is CantAlignTimeException)
            {
                msg = LocManager.Get("CantAlignTimeError");
            }

            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);

            Shell.Logger.Fatal(ex, "Application startup failed");
            Current.Shutdown(1);
        }
    }
}