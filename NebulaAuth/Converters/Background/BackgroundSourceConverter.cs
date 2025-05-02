using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using NebulaAuth.Model;

namespace NebulaAuth.Converters.Background;

public class BackgroundSourceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BackgroundMode.Custom)
        {
            if (File.Exists("Background.png"))
                return new BitmapImage(new Uri(Path.GetFullPath("Background.png")));
        }


        return new BitmapImage(new Uri("pack://application:,,,/Theme/Background.jpg"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}