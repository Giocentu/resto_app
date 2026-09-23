using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RestoApp.Desktop.Views;

public partial class AlertaWindow : Window
{
    public AlertaWindow()
    {
        InitializeComponent();
    }

    public AlertaWindow(string mensaje)
    {
        InitializeComponent();
        TxtMensaje.Text = mensaje;
    }

    private void BtnAceptar_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
