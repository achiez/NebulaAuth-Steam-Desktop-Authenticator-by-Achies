using NebulaAuth.Model;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace NebulaAuth.Core;

public static class ThemeManager
{
    public static System.Windows.Media.Color DefaultBackgroundColor = System.Windows.Media.Color.FromRgb(30, 32, 37);
    private static readonly Window MainWindow = Application.Current.MainWindow!;
    static ThemeManager()
    {
        Settings.Instance.PropertyChanged += SettingsOnPropertyChanged;
    }

    private static void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.BackgroundColor))
        {
            UpdateBackground();
        }
        else if (e.PropertyName == nameof(Settings.IconColor))
        {
            UpdateIcon();
        }
    }

    private static void UpdateIcon()
    {
        var color = Settings.Instance.IconColor;
        if (color == null)
        {
            MainWindow.TaskbarItemInfo = null;
        }
        else
        {
            var c = color.Value;
            var diameter = 14;
            var bitmap = new Bitmap(diameter, diameter);
            var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var brush = new SolidBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
            graphics.FillEllipse(brush, 0, 0, diameter, diameter);
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            MainWindow.TaskbarItemInfo = new();
            MainWindow.TaskbarItemInfo.Overlay = bitmapSource;
        }
    }

    private static void UpdateBackground()
    {
        var color = Settings.Instance.BackgroundColor ?? DefaultBackgroundColor;
        Application.Current.Resources["WindowBackground"] = new SolidColorBrush(color);

    }

    public static void InitializeTheme()
    {
        UpdateIcon();
        UpdateBackground();
    }
}