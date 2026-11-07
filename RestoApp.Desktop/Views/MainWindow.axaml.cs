using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RestoApp.Desktop.ViewModels;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace RestoApp.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(SeleccionarRutaBackupAsync);
    }

    private async Task<string?> SeleccionarRutaBackupAsync()
    {
        var archivo = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Guardar copia de seguridad",
            SuggestedFileName = $"RestoApp_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
            DefaultExtension = "bak",
            FileTypeChoices =
            [
                new FilePickerFileType("Copia de seguridad de SQL Server")
                {
                    Patterns = ["*.bak"]
                }
            ]
        });

        return archivo?.Path.LocalPath;
    }

    private void MenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Removemos la clase 'active' de todos los botones del menú de forma segura
        BtnInicio?.Classes.Remove("active");
        BtnReservas?.Classes.Remove("active");
        BtnPersonal?.Classes.Remove("active");
        BtnCaja?.Classes.Remove("active");
        BtnEventos?.Classes.Remove("active");

        // 2. Identificamos qué botón disparó el evento y le añadimos la clase active
        if (sender is Button clickedButton)
        {
            clickedButton.Classes.Add("active");
        }
    }

    private void CerrarSesion(object? sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        this.Close();
    }
}