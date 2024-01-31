using System.Windows;

namespace NebulaAuth.Theme.WindowStyle
{
    /// <summary>
    /// Логика взаимодействия для WindowStyle.xaml
    /// </summary>
    partial class WindowStyle
    {
        public WindowStyle()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.Close();
        }

        private void OnMaximizeRestoreClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.WindowState = WindowState.Minimized;
        }
    }
}
