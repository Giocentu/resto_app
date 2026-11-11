using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;

namespace RestoApp.Desktop.Views;

public partial class CajaView : UserControl
{
    public CajaView()
    {
        InitializeComponent();
    }

    private async void BtnAbrirMesa_Click(object? sender, RoutedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            var win = new AbrirMesaWindow();
            bool? res = await win.ShowDialog<bool?>(mainWindow);
            if (res == true && DataContext is CajaViewModel vm)
            {
                await vm.CargarCuentasPendientesAsync();
            }
        }
    }

    private async void BtnAgregarProducto_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CajaViewModel vm || vm.CuentaSeleccionada == null)
            return;

        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            var win = new AgregarProductosWindow(vm.CuentaSeleccionada);
            bool? res = await win.ShowDialog<bool?>(mainWindow);
            if (res == true)
            {
                await vm.CargarCuentasPendientesAsync();
            }
        }
    }
}
