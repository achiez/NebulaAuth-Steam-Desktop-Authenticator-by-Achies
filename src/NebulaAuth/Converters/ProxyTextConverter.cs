using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.Converters;

public class ProxyTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MaProxy p)
        {
            return string.Empty;
        }

        return $"{p.Id}: {p.Data.Address}:{p.Data.Port}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ProxyDataTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ProxyData p)
        {
            return string.Empty;
        }

        return $"{p.Protocol.ToString().ToLowerInvariant()}://{p.Address}:{p.Port}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ProxyDataTextMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var value = values[0] as ProxyData;
        var displayProtocol = values.Length > 1 && values[1] is bool dp && dp;
        var displayCredentials = values.Length > 2 && values[2] is bool dc && dc;
        if (value == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        if (displayProtocol)
        {
            sb.Append(value.Protocol.ToString().ToLowerInvariant());
            sb.Append("://");
        }

        sb.Append(value.Address);
        sb.Append(':');
        sb.Append(value.Port);
        if (displayCredentials && value.AuthEnabled)
        {
            sb.Append(':');
            sb.Append(value.Username);
            sb.Append(':');
            sb.Append(value.Password);
        }

        return sb.ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}