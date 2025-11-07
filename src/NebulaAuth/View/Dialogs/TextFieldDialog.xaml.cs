using MaterialDesignThemes.Wpf;

namespace NebulaAuth.View.Dialogs;

public partial class TextFieldDialog
{
    public TextFieldDialog()
    {
        InitializeComponent();
    }

    public TextFieldDialog(string? title, string? msg)
    {
        InitializeComponent();

        if (!string.IsNullOrWhiteSpace(title))
            TitleTextBlock.Text = title;

        if (!string.IsNullOrEmpty(msg))
            HintAssist.SetHint(TextField, msg);
    }
}