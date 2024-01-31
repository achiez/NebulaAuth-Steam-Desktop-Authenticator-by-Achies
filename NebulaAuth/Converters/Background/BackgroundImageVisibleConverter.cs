using NebulaAuth.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NebulaAuth.Converters.Background;

public class BackgroundImageVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not BackgroundMode.Color ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}