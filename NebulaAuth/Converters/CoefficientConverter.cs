using System;
using System.Globalization;
using System.Windows.Data;

namespace NebulaAuth.Converters;

[ValueConversion(typeof(double), typeof(double))]
public class CoefficientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (double)value / (double)parameter;
    }
}