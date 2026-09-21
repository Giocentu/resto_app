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

        if(mainWindow != null)
        {
            await formWindow.ShowDialog(mainWindow);
            //if(DataContext is MesaItemViewModel viewModel)
            //{
            //    await viewModel.CargarMesasAsync()
            //}
        }
    }
    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MesaItemViewModel mesaSeleccionada)
        {
            // Aquí puedes re-utilizar NuevaMesaWindow pasándole el ID en el constructor, 
            // o crear una nueva EditarMesaWindow(mesaSeleccionada.IdMesa)
            
            var formEditar = new NuevaMesaWindow(mesaSeleccionada);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            
            if (mainWindow != null)
            {
                await formEditar.ShowDialog(mainWindow);
                // Recargar grilla tras editar
                if (DataContext is MesasViewModel viewModel) await viewModel.CargarMesasAsync();
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
                // CAMBIO 1: Usar bool? (nullable) por si la ventana se cierra con la 'X' superior
                bool? confirmados = await confirmacion.ShowDialog<bool?>(mainWindow);
                
                // CAMBIO 2: Validar estrictamente que el resultado sea true
                if (confirmados == true)
                {
                    // Aquí ejecutarás tu lógica de base de datos más adelante
                    // await _mesaService.DarDeBajaAsync(mesaSeleccionada.IdMesa);
                    
                    if (DataContext is MesasViewModel viewModel)
                    {
                        await viewModel.CargarMesasAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // CAMBIO 3: Si algo falla (ej. error de SQL), se atrapa aquí y la app NO se cierra
                Console.WriteLine($"Error crítico al intentar eliminar la mesa: {ex.Message}");
            }
        }
    }
}
}