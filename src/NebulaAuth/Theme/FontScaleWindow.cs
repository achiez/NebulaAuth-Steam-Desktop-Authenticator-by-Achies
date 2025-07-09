using System;
using System.Collections.Generic;
using System.Windows;

namespace NebulaAuth.Theme;

public class FontScaleWindow : Window
{
    // Using a DependencyProperty as the backing store for ScaleCoefficient.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ScaleCoefficientProperty =
        DependencyProperty.Register("ScaleCoefficient", typeof(double), typeof(FontScaleWindow),
            new PropertyMetadata(1d));


    public static readonly DependencyProperty DefaultFontSizeProperty =
        DependencyProperty.Register("DefaultFontSize", typeof(double), typeof(FontScaleWindow),
            new PropertyMetadata(20d));


    private static readonly DependencyPropertyKey ParentScaleWindowKey
        = DependencyProperty.RegisterAttachedReadOnly(
            "ParentScaleWindow",
            typeof(FontScaleWindow), typeof(FontScaleWindow),
            new FrameworkPropertyMetadata(default(FontScaleWindow),
                FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty ParentScaleWindowProperty
        = ParentScaleWindowKey.DependencyProperty;


    public static readonly DependencyProperty ResizeFontProperty = DependencyProperty.RegisterAttached(
        "ResizeFont", typeof(bool), typeof(FontScaleWindow), new FrameworkPropertyMetadata(false, ResizeCallBack));
    //<-------------------Window-------------------->


    //<-------------------Scaling-------------------->

    public static readonly DependencyProperty ScaleProperty = DependencyProperty.RegisterAttached(
        "Scale", typeof(double), typeof(FontScaleWindow),
        new FrameworkPropertyMetadata(0.9999d, FrameworkPropertyMetadataOptions.Inherits, ScalePropertyCallback));


    //<-------------------Window-------------------->
    public double DefaultFontSize
    {
        get => (double) GetValue(DefaultFontSizeProperty);
        set => SetValue(DefaultFontSizeProperty, value);
    }

    public double ScaleCoefficient
    {
        get => (double) GetValue(ScaleCoefficientProperty);
        set => SetValue(ScaleCoefficientProperty, value);
    }

    private readonly HashSet<FrameworkElement> _cachedObjects = new();

    private readonly Func<double, double, double> _diagonal = (h, w) => Math.Sqrt(h * h + w * w);
    private double _currentDiagonal;
    private double _defaultDiagonal = 1;
    private bool _loaded;

    public FontScaleWindow()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var w = (FontScaleWindow) sender;
        w._defaultDiagonal = _diagonal(MinHeight, MinWidth);
        w.SizeChanged += OnSizeChanged;
        w.Loaded -= OnLoaded;
        _loaded = true;
        w.Width += 1;
        w.Width -= 1;
    }

    private static FontScaleWindow? GetParentScaleWindow(DependencyObject element)
    {
        return (FontScaleWindow) element.GetValue(ParentScaleWindowProperty);
    }

    private static void SetParentScaleWindow(DependencyObject element, FontScaleWindow value)
    {
        element.SetValue(ParentScaleWindowKey, value);
    }

    public static void SetResizeFont(DependencyObject element, bool value)
    {
        element.SetValue(ResizeFontProperty, value);
    }

    public static bool GetResizeFont(DependencyObject element)
    {
        return (bool) element.GetValue(ResizeFontProperty);
    }

    private static bool SetWindow(FrameworkElement el)
    {
        var w = GetWindow(el);
        if (w is not FontScaleWindow fsWindow) return false;
        SetParentScaleWindow(el, fsWindow);
        if (fsWindow._cachedObjects.Contains(el)) return true;
        fsWindow._cachedObjects.Add(el);
        el.Unloaded += ObjOnUnloaded;

        return true;
    }

    private static void ObjOnLoaded(object sender, RoutedEventArgs e)
    {
        var el = (FrameworkElement) sender;
        if (GetParentScaleWindow(el) == null)
        {
            if (!SetWindow(el))
            {
                el.Loaded -= ObjOnLoaded;
                return;
            }
        }

        ResizeCallBack(sender, new DependencyPropertyChangedEventArgs());
    }

    private static void ResizeCallBack(object? sender, DependencyPropertyChangedEventArgs eventArgs)
    {
        if (sender is not FrameworkElement obj) return;
        var window = GetParentScaleWindow(obj);
        if (window != null && !window._cachedObjects.Contains(obj))
        {
            if (!SetWindow(obj)) return;
        }
        else if (window == null)
        {
            obj.Loaded += ObjOnLoaded;
        }

        if (window is not {_loaded: true}) return;
        CalculateFontSizeResized(obj);
    }

    private static void ObjOnUnloaded(object sender, RoutedEventArgs e)
    {
        var obj = (FrameworkElement) sender;
        var w = GetParentScaleWindow(obj)!;
        w._cachedObjects.Remove(obj);
        obj.Unloaded -= ObjOnUnloaded;
        obj.Loaded -= ObjOnLoaded;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _currentDiagonal = _diagonal(e.NewSize.Width, e.NewSize.Height);
        foreach (var cached in _cachedObjects)
        {
            CalculateFontSizeResized(cached);
        }
    }

    public static void SetScale(DependencyObject element, double value)
    {
        element.SetValue(ScaleProperty, value);
    }

    public static double GetScale(DependencyObject element)
    {
        return (double) element.GetValue(ScaleProperty);
    }


    private static void ScalePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = GetParentScaleWindow(d);
        if (window == null)
        {
            var w = GetWindow(d);
            if (w is FontScaleWindow fsWindow)
            {
                SetParentScaleWindow(d, fsWindow);
            }
        }

        ResizeElement(d);
    }

    public static void ResizeElement(DependencyObject d)
    {
        var set = d.ReadLocalValue(ScaleProperty) != DependencyProperty.UnsetValue;
        if (set)
        {
            if (GetResizeFont(d))
            {
                CalculateFontSizeResized(d);
            }
            else
            {
                CalculateFontSize(d);
            }
        }
    }

    public static void CalculateFontSize(DependencyObject d)
    {
        var window = GetParentScaleWindow(d);
        if (window == null) return;
        var fontSize = GetScale(d) * window._defaultDiagonal;
        d.SetValue(FontSizeProperty, fontSize);
    }

    public static void CalculateFontSizeResized(DependencyObject d)
    {
        var window = GetParentScaleWindow(d);
        if (window == null) return;
        var windowsScale = Math.Pow(window._currentDiagonal / window._defaultDiagonal, window.ScaleCoefficient);
        var elScale = GetScale(d);
        var fontSize = window.DefaultFontSize * elScale * windowsScale;
        d.SetValue(FontSizeProperty, fontSize);
    }
}