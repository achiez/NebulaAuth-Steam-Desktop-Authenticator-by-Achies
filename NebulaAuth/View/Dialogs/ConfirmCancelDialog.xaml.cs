namespace NebulaAuth.View.Dialogs;

/// <summary>
/// Логика взаимодействия для ConfirmCancelDialog.xaml
/// </summary>
public partial class ConfirmCancelDialog
{
    public ConfirmCancelDialog()
    {
        InitializeComponent();
    }

    public ConfirmCancelDialog(string msg)
    {
        InitializeComponent();
        ConfirmTextBlock.Text = msg;
    }
}