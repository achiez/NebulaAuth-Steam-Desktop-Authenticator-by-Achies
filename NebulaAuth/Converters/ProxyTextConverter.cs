using System;
using System.Globalization;
using System.Windows.Data;
using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.Converters;

public class ProxyTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MaProxy p)
        {
            return string.Empty;
        }

        return $"{p.Id}: {p.Data.Address}:{p.Data.Port}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ProxyDataTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ProxyData p)
        {
            return string.Empty;
        }

        return $"{p.Address}:{p.Port}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}