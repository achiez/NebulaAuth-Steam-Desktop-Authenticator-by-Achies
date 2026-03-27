using System;
using System.Globalization;
using System.Windows.Data;

namespace NebulaAuth.Converters;

public class ObjectEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2)
        {
            return ReferenceEquals(values[0], values[1]);
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
