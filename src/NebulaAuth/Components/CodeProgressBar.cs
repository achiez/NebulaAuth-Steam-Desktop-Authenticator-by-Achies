using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NebulaAuth;

public class CodeProgressBar : ProgressBar
{
    public static readonly DependencyProperty TimeRemainingProperty =
        DependencyProperty.Register(nameof(TimeRemaining), typeof(double), typeof(CodeProgressBar),
            new PropertyMetadata(-1.0, OnTimeRemainingChanged));

    public static readonly DependencyProperty MaxTimeProperty =
        DependencyProperty.Register(nameof(MaxTime), typeof(double), typeof(CodeProgressBar),
            new PropertyMetadata(30.0));

    public double TimeRemaining
    {
        get => (double) GetValue(TimeRemainingProperty);
        set => SetValue(TimeRemainingProperty, value);
    }

    public double MaxTime
    {
        get => (double) GetValue(MaxTimeProperty);
        set => SetValue(MaxTimeProperty, value);
    }

    private static void OnTimeRemainingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CodeProgressBar progressBar)
        {
            var newValue = (double) e.NewValue;
            progressBar.StartProgressAnimation(newValue);
        }
    }

    private void StartProgressAnimation(double timeRemaining)
    {
        if (timeRemaining <= 0 || MaxTime <= 0) return;

        var progress = (1 - timeRemaining / MaxTime) * 100;
        Value = 0;
        Value = 100;
        var animation = new DoubleAnimation
        {
            From = progress,
            To = 100,
            Duration = TimeSpan.FromSeconds(timeRemaining),
            AccelerationRatio = 0,
            DecelerationRatio = 0
        };


        BeginAnimation(ValueProperty, animation);
    }
}