using System;
using System.Globalization;
using System.Windows.Data;

namespace NebulaAuth.Converters;

public class OnOffBoolTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool b)
        {
            throw new InvalidCastException($"Can't cast value {value} to 'bool'");
        }

        return b ? "вкл" : "выкл";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}