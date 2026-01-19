using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NebulaAuth.View;

public partial class MafileExporterView : UserControl
{
    public MafileExporterView()
    {
        InitializeComponent();
    }

    private void PathTB_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void ExportTB_GotFocus(object sender, RoutedEventArgs e)
    {
        TemplateExpander.IsExpanded = false;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => { EditTemplateNameTextBox.Focus(); }, DispatcherPriority.Input);
    }
}