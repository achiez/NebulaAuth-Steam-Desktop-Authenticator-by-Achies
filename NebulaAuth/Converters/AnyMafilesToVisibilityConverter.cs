using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NebulaAuth.Converters;

public class AnyMafilesToVisibilityConverter : IValueConverter
{
    private static bool _everAnyMafiles;
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (_everAnyMafiles)
        {
            return Visibility.Collapsed;
        }
        if (value is 0)
        {
            return Visibility.Visible;
        }

        _everAnyMafiles = true;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}