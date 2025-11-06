using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace NebulaAuth.View;


public partial class SettingsView
{
    private readonly DialogHost? _dialogHost;
    private bool _applyBlurBackground;

    private BindingBase? _applyBlurBinding;
    private SolidColorBrush? _defaultDialogBackground;
    private SolidColorBrush? _defaultOverlayBackground;
    private BindingBase? _dialogBackgroundBinding;
    private BindingBase? _overlayBackgroundBinding;

    public SettingsView()
    {
        InitializeComponent();
    }

    public SettingsView(DialogHost? dialogHost)
    {
        _dialogHost = dialogHost;
        InitializeComponent();
    }

    private void Slider_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_dialogHost != null)
        {
            _applyBlurBinding = BindingOperations.GetBindingBase(_dialogHost, DialogHost.ApplyBlurBackgroundProperty);
            _dialogBackgroundBinding =
                BindingOperations.GetBindingBase(_dialogHost, DialogHost.DialogBackgroundProperty);
            _overlayBackgroundBinding =
                BindingOperations.GetBindingBase(_dialogHost, DialogHost.OverlayBackgroundProperty);


            _defaultDialogBackground = _dialogHost.DialogBackground as SolidColorBrush;
            _defaultOverlayBackground = _dialogHost.OverlayBackground as SolidColorBrush;
            _applyBlurBackground = _dialogHost.ApplyBlurBackground;


            _dialogHost.ClearValue(DialogHost.ApplyBlurBackgroundProperty);
            _dialogHost.ApplyBlurBackground = false;
            _dialogHost.DialogBackground = new SolidColorBrush(Colors.Transparent);
            _dialogHost.OverlayBackground = new SolidColorBrush(Colors.Transparent);
        }

        Opacity = 0.05;
    }

    private void Slider_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_dialogHost != null)
        {
            if (_applyBlurBinding != null)
                BindingOperations.SetBinding(_dialogHost, DialogHost.ApplyBlurBackgroundProperty, _applyBlurBinding);
            else
                _dialogHost.ApplyBlurBackground = _applyBlurBackground;

            if (_dialogBackgroundBinding != null)
                BindingOperations.SetBinding(_dialogHost, DialogHost.DialogBackgroundProperty,
                    _dialogBackgroundBinding);
            else
                _dialogHost.DialogBackground = _defaultDialogBackground;

            if (_overlayBackgroundBinding != null)
                BindingOperations.SetBinding(_dialogHost, DialogHost.OverlayBackgroundProperty,
                    _overlayBackgroundBinding);
            else
                _dialogHost.OverlayBackground = _defaultOverlayBackground ?? new SolidColorBrush(Colors.Transparent);
        }

        Opacity = 1.0;
    }
}