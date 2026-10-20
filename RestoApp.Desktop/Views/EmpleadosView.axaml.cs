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

    private async void BtnGestionClientes_Click(object? sender, RoutedEventArgs e)
    {
        var win = new GestionClientesWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            await win.ShowDialog(mainWindow);
        }
    }

    private async void BtnGestionTurnos_Click(object? sender, RoutedEventArgs e)
    {
        var win = new GestionTurnosWindow();
        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            await win.ShowDialog(mainWindow);
        }
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
                await viewModel.GuardarEmpleadoAsync(formWindow.EmpleadoResult);
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
                    await viewModel.GuardarEmpleadoAsync(formEditar.EmpleadoResult);
                }
            }
        }
    }
}