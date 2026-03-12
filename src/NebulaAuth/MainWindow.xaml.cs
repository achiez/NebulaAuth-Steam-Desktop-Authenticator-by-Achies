using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.View.Dialogs;
using NebulaAuth.ViewModel;
using NebulaAuth.ViewModel.Other;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NebulaAuth;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainVM();
        Application.Current.MainWindow = this;
        TrayManager.InitializeTray();
        ThemeManager.InitializeTheme();
        Title = Title + " " + Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        Loaded += OnApplicationStarted;
        UpdateManager.PendingUpdateDetected += OnPendingUpdateDetected;
    }

    private void OnPendingUpdateDetected()
    {
        Dispatcher.BeginInvoke(StartUpdateBlink);
    }

    private void StartUpdateBlink()
    {
        UpdateBadgeIcon.Visibility = Visibility.Visible;
        var baseColor = FindResource("AccentBrush") is SolidColorBrush accent
            ? accent.Color
            : ByAchiesBrush.Color;
        var duration = new Duration(TimeSpan.FromSeconds(6));
        var sb = new Storyboard();
        sb.Children.Add(BuildBlinkAnimation(duration, "UpdateBadgeBrush"));
        sb.Children.Add(BuildBlinkAnimation(duration, "ByAchiesBrush"));
        sb.Completed += (_, _) =>
        {
            UpdateBadgeIcon.Visibility = Visibility.Collapsed;
            ByAchiesBrush.Color = baseColor;
        };
        sb.Begin(this);
    }

    private static ColorAnimationUsingKeyFrames BuildBlinkAnimation(Duration duration, string targetName)
    {
        var anim = new ColorAnimationUsingKeyFrames { Duration = duration };
        for (var i = 0; i <= 6; i++)
        {
            var color = i % 2 == 0 ? Colors.DodgerBlue : Colors.OrangeRed;
            anim.KeyFrames.Add(new DiscreteColorKeyFrame(color, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(i))));
        }
        Storyboard.SetTargetName(anim, targetName);
        Storyboard.SetTargetProperty(anim, new PropertyPath(SolidColorBrush.ColorProperty));
        return anim;
    }

    private async void OnApplicationStarted(object? sender, EventArgs e)
    {
        ((MainVM) DataContext).CurrentDialogHost = DialogHostInstance;
        if (!Settings.Instance.IsPasswordSet) return;
        Topmost = false;
        await Dispatcher.InvokeAsync(ShowSetPasswordDialog, DispatcherPriority.ContextIdle);
    }


    private async Task ShowSetPasswordDialog()
    {
        var vm = new SetEncryptPasswordVM();
        var dialog = new SetCryptPasswordDialog
        {
            DataContext = vm
        };

        var result = await DialogHost.Show(dialog);
        var pass = vm.Password;
        if (result is true && !string.IsNullOrWhiteSpace(pass))
        {
            PHandler.SetPassword(pass);
        }
    }

    private void SearchField_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !SearchField.IsFocused) return;
        Keyboard.ClearFocus();
    }

    private void MafileListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;
        if (ItemsControl.ContainerFromElement((ListBox) sender, (DependencyObject) e.OriginalSource) is ListBoxItem
            {
                IsSelected: true
            })
        {
            e.Handled = true;
        }
    }

    private void FocusSearchBox(object sender, ExecutedRoutedEventArgs e)
    {
        SearchField.Focus();
    }

    #region Dran'n'Drop

    private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!int.TryParse(e.Text, out _)) e.Handled = true;
    }

    private void Rectangle_DragEnter(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Handled = true;
            return;
        }

        DragNDropPanel.Visibility = Visibility.Visible;
        DragNDropOverlay.Visibility = Visibility.Visible;
    }

    private async void Rectangle_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var filePaths = (string[]) e.Data.GetData(DataFormats.FileDrop)!;
        if (filePaths.Length == 0) return;
        if (DataContext is MainVM mainVm)
        {
            await mainVm.AddMafile(filePaths);
        }

        DragNDropPanel.Visibility = Visibility.Hidden;
        DragNDropOverlay.Visibility = Visibility.Hidden;
    }

    private void Rectangle_DragLeave(object sender, DragEventArgs e)
    {
        DragNDropPanel.Visibility = Visibility.Hidden;
        DragNDropOverlay.Visibility = Visibility.Hidden;
    }


    private void DialogHost_DialogOpened(object sender, DialogOpenedEventArgs eventArgs)
    {
        DragNDropBorder.AllowDrop = false;
    }

    private void DialogHost_DialogClosed(object sender, DialogClosedEventArgs eventArgs)
    {
        DragNDropBorder.AllowDrop = true;
    }

    #endregion
}