namespace NebulaAuth.View.Dialogs;

public partial class ConfirmCancelDialog
{
    public ConfirmCancelDialog()
    {
        InitializeComponent();
    }

    public ConfirmCancelDialog(string msg)
    {
        InitializeComponent();
        ConfirmHint.Text = msg;
    }
}