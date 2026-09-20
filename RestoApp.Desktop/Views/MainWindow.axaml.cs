using Avalonia.Controls;
using RestoApp.Desktop.ViewModels;
using Avalonia.Interactivity;

namespace RestoApp.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void MenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Removemos la clase 'active' de todos los botones del menú
        BtnInicio.Classes.Remove("active");
        BtnReservas.Classes.Remove("active");
        BtnPersonal.Classes.Remove("active");
        BtnCaja.Classes.Remove("active");
        BtnEventos.Classes.Remove("active");

        // 2. Identificamos qué botón disparó el evento y le añadimos la clase active
        if (sender is Button clickedButton)
        {
            clickedButton.Classes.Add("active");
        }
    }
    private void CerrarSesion(object? sender, RoutedEventArgs e )
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        this.Close();
    }
}