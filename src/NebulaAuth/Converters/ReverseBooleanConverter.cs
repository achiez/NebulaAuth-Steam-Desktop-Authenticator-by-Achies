using System;
using System.Globalization;
using System.Windows.Data;

namespace NebulaAuth.Converters;

public class ReverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        throw new ArgumentException("Value must be of type bool", nameof(value));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        throw new ArgumentException("Value must be of type bool", nameof(value));
    }
}