using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaAuth.View;


public partial class ProxyManagerView
{
    public ProxyManagerView()
    {
        InitializeComponent();
    }

    private void ProxyInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;
        var tb = sender as TextBox;
        tb?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        AddProxyBtn.Command.Execute(null);
    }
}