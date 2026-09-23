using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;

namespace RestoApp.Desktop.Views;

public partial class EmpleadosView : UserControl
{
    public EmpleadosView()
    {
        InitializeComponent();
    }

    private async void BtnNuevoEmpleado_Click(object? sender, RoutedEventArgs e)
    {
        var formWindow = new NuevoEmpleadoWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;

        if (mainWindow != null)
        {
            await formWindow.ShowDialog(mainWindow);
            if (formWindow.EmpleadoResult != null && DataContext is EmpleadosViewModel viewModel)
            {
                viewModel.AgregarOActualizarEmpleado(formWindow.EmpleadoResult);
            }
        }
    }

    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is EmpleadoItemViewModel empleadoSeleccionado)
        {
            var formEditar = new NuevoEmpleadoWindow(empleadoSeleccionado);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                await formEditar.ShowDialog(mainWindow);
                if (formEditar.EmpleadoResult != null && DataContext is EmpleadosViewModel viewModel)
                {
                    viewModel.AgregarOActualizarEmpleado(formEditar.EmpleadoResult);
                }
            }
        }
    }

    private async void BtnEliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is EmpleadoItemViewModel empleadoSeleccionado)
        {
            var confirmacion = new ConfirmacionWindow($"¿Estás seguro de que deseas dar de baja al empleado {empleadoSeleccionado.NombreCompleto} (DNI: {empleadoSeleccionado.DniEmpleado})?");
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                try
                {
                    bool? confirmados = await confirmacion.ShowDialog<bool?>(mainWindow);
                    if (confirmados == true && DataContext is EmpleadosViewModel viewModel)
                    {
                        await viewModel.DarBajaEmpleadoCommand.ExecuteAsync(empleadoSeleccionado);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al intentar eliminar el empleado: {ex.Message}");
                }
            }
        }
    }
}