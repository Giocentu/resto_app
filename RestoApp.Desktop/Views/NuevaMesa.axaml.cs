using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RestoApp.Desktop.Views;

public partial class NuevaMesaWindow : Window
{
    public NuevaMesaWindow()
    {
        InitializeComponent();
    }

    public NuevaMesaWindow(RestoApp.Desktop.ViewModels.MesaItemViewModel mesa)
    {
        InitializeComponent();
        TxtNroMesa.Text = mesa.NroMesa.ToString();
        TxtCapacidad.Text = mesa.Capacidad.ToString();
        TxtUbicacion.Text = mesa.UbicacionDescripcion;
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close(); // Cierra la ventana sin hacer nada
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        // Aquí agregaremos la lógica para guardar en base de datos más adelante
        Close();
    }
}