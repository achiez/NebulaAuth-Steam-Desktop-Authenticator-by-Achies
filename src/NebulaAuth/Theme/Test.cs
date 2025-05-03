using System.Windows.Media.Animation;
using System.Windows;
using System;
using System.Windows.Controls;

namespace NebulaAuth.Theme;

public class ProgressBarSmoother
{
    public static double GetSmoothValue(DependencyObject obj)
    {
        return (double)obj.GetValue(SmoothValueProperty);
    }

    public static void SetSmoothValue(DependencyObject obj, double value)
    {
        obj.SetValue(SmoothValueProperty, value);
    }

    public static readonly DependencyProperty SmoothValueProperty =
        DependencyProperty.RegisterAttached("SmoothValue", typeof(double), typeof(ProgressBarSmoother), new PropertyMetadata(0.0, changing));

    private static void changing(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var anim = new DoubleAnimation((double)e.OldValue, (double)e.NewValue, new TimeSpan(0, 0, 0, 0, 250));
        (d as ProgressBar).BeginAnimation(ProgressBar.ValueProperty, anim, HandoffBehavior.Compose);
    }
}