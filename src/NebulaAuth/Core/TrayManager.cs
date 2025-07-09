using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using NebulaAuth.Model;
using Application = System.Windows.Application;

namespace NebulaAuth.Core;

public static class TrayManager
{
    private static NotifyIcon? _notifyIcon;
    private static readonly Window MainWindow = Application.Current.MainWindow!;
    private static bool IsEnabled => Settings.Instance.HideToTray;

    [MemberNotNullWhen(true, nameof(_notifyIcon))]
    private static bool Init { get; set; }

    public static void InitializeTray()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Text = "NebulaAuth";

        var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Theme/lock.ico"))!.Stream;
        _notifyIcon.Icon = new Icon(iconStream);

        _notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

        var contextMenu = new ContextMenuStrip();

        contextMenu.Items.Add("Выйти", null!, OnExitClick);

        _notifyIcon.ContextMenuStrip = contextMenu;

        MainWindow.StateChanged += MainWindow_StateChanged;
        Init = true;
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
        if (!Init) return;
        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        _notifyIcon.Visible = false;
    }

    private static void HideMainWindow()
    {
        if (!Init) return;
        if (IsEnabled == false) return;
        _notifyIcon.Visible = true;
        MainWindow.Hide();
    }

    private static void ExitApplication()
    {
        if (!Init) return;
        _notifyIcon.Dispose();
        Application.Current.Shutdown();
    }
}