using Avalonia.Controls;
using RestoApp.Desktop.ViewModels;

namespace RestoApp.Desktop.Views;

public partial class AgregarProductosWindow : Window
{
    public AgregarProductosWindow()
    {
        InitializeComponent();
    }

    public AgregarProductosWindow(CuentaPendienteViewModel cuenta) : this()
    {
        var vm = new AgregarProductosViewModel(cuenta);
        vm.RequestClose += (resultado) =>
        {
            Close(resultado);
        };
        DataContext = vm;
    }
}

