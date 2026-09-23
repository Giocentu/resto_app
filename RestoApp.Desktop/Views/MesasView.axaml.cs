using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;
namespace RestoApp.Desktop.Views;

public partial class MesasView : UserControl
{
    public MesasView()
    {
        InitializeComponent();
    }
    private async void BtnNuevaMesa_Click(object? sender, RoutedEventArgs e)
    {
        var formWindow = new NuevaMesaWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;

        if (mainWindow != null)
        {
            await formWindow.ShowDialog(mainWindow);
            if (formWindow.MesaResult != null && DataContext is MesasViewModel viewModel)
            {
                await viewModel.GuardarMesaAsync(formWindow.MesaResult);
            }
        }
    }

    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MesaItemViewModel mesaSeleccionada)
        {
            var formEditar = new NuevaMesaWindow(mesaSeleccionada);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                await formEditar.ShowDialog(mainWindow);
                if (formEditar.MesaResult != null && DataContext is MesasViewModel viewModel)
                {
                    await viewModel.GuardarMesaAsync(formEditar.MesaResult);
                }
            }
        }
    }

    private async void BtnEliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MesaItemViewModel mesaSeleccionada)
        {
            var confirmacion = new ConfirmacionWindow($"¿Estás seguro de que deseas dar de baja la Mesa Nro {mesaSeleccionada.NroMesa}?");
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                try
                {
                    bool? confirmados = await confirmacion.ShowDialog<bool?>(mainWindow);
                    if (confirmados == true && DataContext is MesasViewModel viewModel)
                    {
                        await viewModel.DarBajaMesaCommand.ExecuteAsync(mesaSeleccionada);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error crítico al intentar eliminar la mesa: {ex.Message}");
                }
            }
        }
    }
}