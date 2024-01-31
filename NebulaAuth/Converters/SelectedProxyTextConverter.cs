using System;
using System.Globalization;
using System.Windows.Data;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.Converters;

public class SelectedProxyTextConverter : IMultiValueConverter
{

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is not MaProxy proxy)
        {
            return string.Empty;
        }

        var proxyExist = (bool)values[1];
        return proxyExist ? $"{proxy.Id}: {proxy.Data.Address}" : "";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}