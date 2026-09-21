using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RestoApp.Desktop.Views;

public partial class NuevaReservaWindow : Window
{
    public NuevaReservaWindow()
    {
        InitializeComponent();
    }

    public NuevaReservaWindow(RestoApp.Desktop.ViewModels.ReservaItemViewModel reserva)
    {
        InitializeComponent();
        TxtFechaHora.Text = reserva.FechaHora;
        TxtCliente.Text = reserva.ClienteNombre;
        TxtMesa.Text = reserva.NroMesa;
        TxtPersonas.Text = reserva.CantidadPersonas.ToString();
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        // Aquí lógica para guardar reserva
        Close(true);
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}

