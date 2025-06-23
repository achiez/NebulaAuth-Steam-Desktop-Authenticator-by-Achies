using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NebulaAuth.Converters;

public class PortableMaClientStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is false)
        {
            return new SolidColorBrush(Color.FromRgb(187, 224, 139));
        }


        return new SolidColorBrush(Color.FromRgb(224, 139, 139));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}