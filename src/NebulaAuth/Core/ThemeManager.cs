using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using NebulaAuth.Model;
using Color = System.Drawing.Color;

namespace NebulaAuth.Core;

public static class ThemeManager
{
    private static readonly Window MainWindow = Application.Current.MainWindow!;

    static ThemeManager()
    {
        Settings.Instance.PropertyChanged += SettingsOnPropertyChanged;
    }

    private static void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.IconColor))
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
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var brush = new SolidBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
            graphics.FillEllipse(brush, 0, 0, diameter, diameter);
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            MainWindow.TaskbarItemInfo = new TaskbarItemInfo();
            MainWindow.TaskbarItemInfo.Overlay = bitmapSource;
        }
    }


    public static void ApplyTheme(string themeName)
    {
        var colorDict = new ResourceDictionary
        {
            Source = new Uri($"Theme/Themes/{themeName}.xaml", UriKind.Relative)
        };

        var brushDict = new ResourceDictionary
        {
            Source = new Uri("Theme/Brushes.xaml", UriKind.Relative)
        };

        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;


        var toRemove = mergedDictionaries
            .Where(d => d.Source?.OriginalString.Contains("Theme/Themes/") == true ||
                        d.Source?.OriginalString.EndsWith("Brushes.xaml") == true)
            .ToList();

        foreach (var dict in toRemove)
        {
            mergedDictionaries.Remove(dict);
        }

        mergedDictionaries.Insert(0, colorDict);
        mergedDictionaries.Insert(0, brushDict);
    }


    public static void InitializeTheme()
    {
        UpdateIcon();
        ApplyTheme(Settings.Instance.GetTheme());
    }
}