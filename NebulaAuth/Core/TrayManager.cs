using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using NebulaAuth.Model;
using Application = System.Windows.Application;

namespace NebulaAuth.Core;

public static class TrayManager
{
    private static NotifyIcon _notifyIcon;
    private static readonly Window MainWindow = Application.Current.MainWindow!;
    public static bool IsEnabled => Settings.Instance.HideToTray;


    public static void InitializeTray()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Text = "NebulaAuth";

        Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Theme/nebula lock.ico"))!.Stream;
        _notifyIcon.Icon = new Icon(iconStream);
        
        _notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

        var contextMenu = new ContextMenuStrip();

        contextMenu.Items.Add("Выйти", null!, onClick: OnExitClick);

        _notifyIcon.ContextMenuStrip = contextMenu;

        MainWindow.StateChanged += MainWindow_StateChanged;
    }

    private static void OnExitClick(object? sender, EventArgs e)
    {
        ExitApplication();
    }

    private static void NotifyIcon_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowMainWindow();
        }
    }

    private static void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (MainWindow.WindowState == WindowState.Minimized)
        {
            HideMainWindow();
        }
    }

    private static void ShowMainWindow()
    {
        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        _notifyIcon.Visible = false;
    }

    private static void HideMainWindow()
    {
        if(IsEnabled == false) return;
        _notifyIcon.Visible = true;
        MainWindow.Hide();
    }

    private static void ExitApplication()
    {
        _notifyIcon.Dispose();
        Application.Current.Shutdown();
    }
}