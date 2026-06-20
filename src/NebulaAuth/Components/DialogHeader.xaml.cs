using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace NebulaAuth;

public partial class DialogHeader : UserControl
{
    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(PackIconKind), typeof(DialogHeader),
            new PropertyMetadata(PackIconKind.None));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(DialogHeader),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TitleAccentProperty =
        DependencyProperty.Register(nameof(TitleAccent), typeof(string), typeof(DialogHeader),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CloseParameterProperty =
        DependencyProperty.Register(nameof(CloseParameter), typeof(object), typeof(DialogHeader),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsCloseEnabledProperty =
        DependencyProperty.Register(nameof(IsCloseEnabled), typeof(bool), typeof(DialogHeader),
            new PropertyMetadata(true));

    public PackIconKind IconKind
    {
        get => (PackIconKind) GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public string? Title
    {
        get => (string?) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? TitleAccent
    {
        get => (string?) GetValue(TitleAccentProperty);
        set => SetValue(TitleAccentProperty, value);
    }

    public object? CloseParameter
    {
        get => GetValue(CloseParameterProperty);
        set => SetValue(CloseParameterProperty, value);
    }

    public bool IsCloseEnabled
    {
        get => (bool) GetValue(IsCloseEnabledProperty);
        set => SetValue(IsCloseEnabledProperty, value);
    }

    public DialogHeader()
    {
        InitializeComponent();
    }
}