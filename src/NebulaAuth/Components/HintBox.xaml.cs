using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace NebulaAuth;

public partial class HintBox : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(HintBox));

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(nameof(Severity), typeof(HintBoxSeverity), typeof(HintBox),
            new PropertyMetadata(HintBoxSeverity.Info, OnSeverityChanged));

    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(HintBox),
            new PropertyMetadata(false));


    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(HintBox),
            new PropertyMetadata(null));

    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public HintBoxSeverity Severity
    {
        get => (HintBoxSeverity) GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public ICommand CloseCommand
    {
        get => (ICommand) GetValue(CloseCommandProperty) ?? InternalCloseCommand;
        set => SetValue(CloseCommandProperty, value);
    }

    public bool ShowCloseButton
    {
        get => (bool) GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    private ICommand InternalCloseCommand { get; }

    public PackIconKind IconKind { get; private set; }
    public Brush IconBrush { get; private set; }

    public HintBox()
    {
        InitializeComponent();
        ApplySeverityVisuals();

        InternalCloseCommand = new RelayCommand(Close);
    }

    public event RoutedEventHandler Closed;

    private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((HintBox) d).ApplySeverityVisuals();
    }

    private void ApplySeverityVisuals()
    {
        switch (Severity)
        {
            case HintBoxSeverity.Error:
                IconKind = PackIconKind.ErrorOutline;
                IconBrush = (Brush) Application.Current.FindResource("ErrorBrush")!;
                break;
            default:
                IconKind = PackIconKind.InfoCircleOutline;
                IconBrush = (Brush) Application.Current.FindResource("InfoBrush")!;
                break;
        }
    }

    private void Close()
    {
        Visibility = Visibility.Collapsed;
        Closed?.Invoke(this, new RoutedEventArgs());
    }
}

public enum HintBoxSeverity
{
    Info,
    Error
}