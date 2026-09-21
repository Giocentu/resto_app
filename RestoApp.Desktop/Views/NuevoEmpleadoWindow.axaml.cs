using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RestoApp.Desktop.Views;

public partial class NuevoEmpleadoWindow : Window
{
    public NuevoEmpleadoWindow()
    {
        InitializeComponent();
    }

    public NuevoEmpleadoWindow(RestoApp.Desktop.ViewModels.EmpleadoItemViewModel empleado)
    {
        InitializeComponent();
        TxtDni.Text = empleado.DniEmpleado.ToString();
        TxtNombre.Text = empleado.NombreCompleto;
        TxtRol.Text = empleado.RolCargo;
        TxtTelefono.Text = empleado.Telefono;
        TxtEstado.Text = empleado.Estado;
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        // Aquí lógica para guardar empleado
        Close(true);
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}

