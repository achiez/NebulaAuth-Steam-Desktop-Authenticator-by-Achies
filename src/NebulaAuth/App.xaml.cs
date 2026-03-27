using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Exceptions;

namespace NebulaAuth;

public partial class App
{
    private static Mutex? _singleInstanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Ensure only one instance of NebulaAuth is running
        const string mutexName = "NebulaAuth_SingleInstance";
        _singleInstanceMutex = new Mutex(true, mutexName, out bool isNewInstance);

        if (!isNewInstance)
        {
            // Another instance is already running — activate it instead
            Console.WriteLine("[App] Another NebulaAuth instance is running. Activating existing instance...");

            var currentProcess = Process.GetCurrentProcess();
            var existingProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                .FirstOrDefault(p => p.Id != currentProcess.Id);

            if (existingProcess != null)
            {
                // Activate the existing window
                IntPtr hwnd = existingProcess.MainWindowHandle;
                if (hwnd != IntPtr.Zero)
                {
                    SetForegroundWindow(hwnd);
                    ShowWindow(hwnd, ShowWindowCommand.Restore);
                }
            }

            Current.Shutdown(0);
            return;
        }

        try
        {
            var splashScreen = new SplashScreen("Theme\\SplashScreen.png");
            splashScreen.Show(false, true);
            base.OnStartup(e);

            await Shell.Initialize();
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

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    // Windows API imports for window activation
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, ShowWindowCommand nCmdShow);

    private enum ShowWindowCommand
    {
        Hide = 0,
        Show = 1,
        Minimize = 6,
        Restore = 9
    }
}