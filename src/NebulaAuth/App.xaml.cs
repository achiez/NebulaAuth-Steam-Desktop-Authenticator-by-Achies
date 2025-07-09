using System;
using System.IO;
using System.Linq;
using System.Windows;
using AchiesUtilities.Extensions;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Exceptions;

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

            var files = 0;
            if (Directory.Exists(Storage.MAFILE_F))
                files = Directory.GetFiles(Storage.MafileFolder)
                    .Count(f => Path.GetExtension(f).EqualsIgnoreCase(".mafile"));

            var threads = files > 0 ? files / 100 + 1 : 1;
            await Storage.Initialize(threads);
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