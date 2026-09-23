using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;

namespace RestoApp.Desktop.Views;

public partial class ReservasView : UserControl
{
    public ReservasView()
    {
        InitializeComponent();
    }

    private async void BtnGestionClientes_Click(object? sender, RoutedEventArgs e)
    {
        var win = new GestionClientesWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            await win.ShowDialog(mainWindow);
        }
    }

    private async void BtnNuevaReserva_Click(object? sender, RoutedEventArgs e)
    {
        var formWindow = new NuevaReservaWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;

        if (mainWindow != null)
        {
            await formWindow.ShowDialog(mainWindow);
            if (formWindow.ReservaResult != null && DataContext is ReservasViewModel viewModel)
            {
                await viewModel.GuardarReservaAsync(formWindow.ReservaResult);
            }
        }
    }

    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ReservaItemViewModel reservaSeleccionada)
        {
            var formEditar = new NuevaReservaWindow(reservaSeleccionada);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                await formEditar.ShowDialog(mainWindow);
                if (formEditar.ReservaResult != null && DataContext is ReservasViewModel viewModel)
                {
                    await viewModel.GuardarReservaAsync(formEditar.ReservaResult);
                }
            }
        }
    }

    private async void BtnEliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ReservaItemViewModel reservaSeleccionada)
        {
            var confirmacion = new ConfirmacionWindow($"¿Estás seguro de que deseas cancelar la reserva a nombre de {reservaSeleccionada.ClienteNombre} (Mesa {reservaSeleccionada.NroMesa})?");
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                try
                {
                    bool? confirmados = await confirmacion.ShowDialog<bool?>(mainWindow);
                    if (confirmados == true && DataContext is ReservasViewModel viewModel)
                    {
                        await viewModel.CancelarReservaCommand.ExecuteAsync(reservaSeleccionada);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al intentar cancelar la reserva: {ex.Message}");
                }
            }
        }
    }
}