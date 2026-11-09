using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;

namespace RestoApp.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }

    private async void BtnIngresar_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            if (vm.EstaCargando) return;

            bool exito = await vm.IniciarSesionAsync();
            
            if (exito)
            {
                // Instanciamos y mostramos la ventana principal
                var mainWindow = new MainWindow();
                mainWindow.Show();
                
                // Cerramos la ventana actual de login
                this.Close();
            }
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            BtnIngresar_Click(sender, e);
            e.Handled = true;
        }
    }
}
