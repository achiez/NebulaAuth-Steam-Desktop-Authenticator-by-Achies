using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NebulaAuth.View
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ColorPicker_OnColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Debug.WriteLine(e.NewValue);
        }
    }
}
