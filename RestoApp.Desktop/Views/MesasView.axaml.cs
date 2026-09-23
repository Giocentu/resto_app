using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using RestoApp.Entities;
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
        if (DataContext is not MesasViewModel viewModel) return;

        var formWindow = new NuevaMesaWindow(viewModel.Ubicaciones);
        var mainWindow = TopLevel.GetTopLevel(this) as Window;

        if (mainWindow != null)
        {
            await formWindow.ShowDialog(mainWindow);
            if (formWindow.MesaResult != null)
            {
                await viewModel.GuardarMesaAsync(formWindow.MesaResult, formWindow.SelectedUbicacionId);
            }
        }
    }

    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MesaItemViewModel mesaSeleccionada && DataContext is MesasViewModel viewModel)
        {
            var formEditar = new NuevaMesaWindow(mesaSeleccionada, viewModel.Ubicaciones);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                await formEditar.ShowDialog(mainWindow);
                if (formEditar.MesaResult != null)
                {
                    await viewModel.GuardarMesaAsync(formEditar.MesaResult, formEditar.SelectedUbicacionId);
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

    private async void BtnNuevaUbicacion_Click(object? sender, RoutedEventArgs e)
    {
        var formWindow = new NuevaUbicacionWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;

        if (mainWindow != null)
        {
            await formWindow.ShowDialog(mainWindow);
            if (formWindow.UbicacionResult != null && DataContext is MesasViewModel viewModel)
            {
                await viewModel.GuardarUbicacionAsync(formWindow.UbicacionResult);
            }
        }
    }

    private async void BtnEditarUbicacion_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UbicacionMesa ubicacionSeleccionada)
        {
            var formEditar = new NuevaUbicacionWindow(ubicacionSeleccionada);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;

            if (mainWindow != null)
            {
                await formEditar.ShowDialog(mainWindow);
                if (formEditar.UbicacionResult != null && DataContext is MesasViewModel viewModel)
                {
                    await viewModel.GuardarUbicacionAsync(formEditar.UbicacionResult);
                }
            }
        }
    }

    private async void BtnEliminarUbicacion_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UbicacionMesa ubicacionSeleccionada)
        {
            var confirmacion = new ConfirmacionWindow($"¿Estás seguro de que deseas dar de baja / archivar la ubicación '{ubicacionSeleccionada.Ubicacion}'?");
            var mainWindow = TopLevel.GetTopLevel(this) as Window;

            if (mainWindow != null)
            {
                try
                {
                    bool? confirmados = await confirmacion.ShowDialog<bool?>(mainWindow);
                    if (confirmados == true && DataContext is MesasViewModel viewModel)
                    {
                        await viewModel.DarBajaUbicacionCommand.ExecuteAsync(ubicacionSeleccionada);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al dar de baja la ubicación: {ex.Message}");
                }
            }
        }
    }
}