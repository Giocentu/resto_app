using Avalonia.Controls;
using RestoApp.Desktop.ViewModels;
using Avalonia.Interactivity;
namespace RestoApp.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Asignamos el ViewModel como el contexto de datos de esta ventana
        DataContext = new MainViewModel();
    }
    // Evento disparado en C# al hacer clic en cualquier botón del menú
    private void MenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Removemos la clase 'active' de todos los botones
        BtnClientes.Classes.Remove("active");
        BtnEmpleados.Classes.Remove("active");
        BtnReservas.Classes.Remove("active");
        BtnMesas.Classes.Remove("active");

        // 2. Identificamos qué botón disparó el evento y le añadimos la clase
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