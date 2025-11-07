using System;
using System.Globalization;
using System.Windows.Data;

namespace NebulaAuth.Converters;

public class BoolToValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b) return Binding.DoNothing;
        if (parameter is Array)
        {
            var arr = (Array)parameter;
            if (arr.Length == 0) return Binding.DoNothing;
            var first = arr.GetValue(0);
            var second = arr.Length > 1 ? arr.GetValue(1) : null;
            return b ? first : second;
        }

        return b ? parameter : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}