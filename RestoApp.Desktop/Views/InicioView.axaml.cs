using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;

namespace RestoApp.Desktop.Views;

public partial class InicioView : UserControl
{
    public InicioView()
    {
        InitializeComponent();
    }

    private async void BtnAgregarProducto_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not InicioViewModel vm || vm.SelectedMesa == null || !vm.EsMesaOcupada)
            return;

        var cuentaVm = await vm.ObtenerOCrearCuentaParaMesaSeleccionadaAsync();
        if (cuentaVm == null)
            return;

        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            var win = new AgregarProductosWindow(cuentaVm);
            bool? res = await win.ShowDialog<bool?>(mainWindow);
            if (res == true)
            {
                await vm.CargarMesasAsync();
            }
        }
    }
}
