using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;

namespace RestoApp.Desktop.Views;

public partial class AbrirMesaWindow : Window
{
    public AbrirMesaWindow()
    {
        InitializeComponent();
        DataContext = new AbrirMesaViewModel();
    }

    public AbrirMesaWindow(AbrirMesaViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AbrirMesaViewModel vm)
        {
            vm.DetenerReloj();
        }
        Close(false);
    }

    private async void BtnAbrirMesa_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AbrirMesaViewModel vm)
        {
            var res = await vm.ConfirmarAperturaAsync();
            if (res != null)
            {
                Close(true);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is AbrirMesaViewModel vm)
        {
            vm.DetenerReloj();
        }
    }
}

