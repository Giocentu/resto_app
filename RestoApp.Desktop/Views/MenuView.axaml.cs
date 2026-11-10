using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;
using System.Linq;

namespace RestoApp.Desktop.Views;

public partial class MenuView : UserControl
{
    public MenuView()
    {
        InitializeComponent();
    }

    private async void BtnNuevoItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MenuViewModel vm) return;

        // Pasar las categorías reales al formulario
        var categorias = vm.Categorias.Where(c => c.IdCategoria > 0);
        var formWindow = new NuevoMenuItemWindow(categorias);
        var mainWindow = TopLevel.GetTopLevel(this) as Window;

        if (mainWindow != null)
        {
            var result = await formWindow.ShowDialog<bool?>(mainWindow);
            if (result == true && formWindow.ResultDescripcion != null)
            {
                try
                {
                    await vm.GuardarDesdeVentanaAsync(
                        0, // idItem = 0 → nuevo
                        formWindow.ResultDescripcion,
                        formWindow.ResultPrecio,
                        formWindow.ResultIdCategoria,
                        formWindow.ResultActivo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear item del menú: {ex.Message}");
                }
            }
        }
    }

    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MenuItemViewModel itemSeleccionado)
        {
            if (DataContext is not MenuViewModel vm) return;

            var categorias = vm.Categorias.Where(c => c.IdCategoria > 0);
            var formEditar = new NuevoMenuItemWindow(itemSeleccionado, categorias);
            var mainWindow = TopLevel.GetTopLevel(this) as Window;

            if (mainWindow != null)
            {
                var result = await formEditar.ShowDialog<bool?>(mainWindow);
                if (result == true && formEditar.ResultDescripcion != null)
                {
                    try
                    {
                        await vm.GuardarDesdeVentanaAsync(
                            itemSeleccionado.IdItem,
                            formEditar.ResultDescripcion,
                            formEditar.ResultPrecio,
                            formEditar.ResultIdCategoria,
                            formEditar.ResultActivo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al editar item del menú: {ex.Message}");
                    }
                }
            }
        }
    }
}
