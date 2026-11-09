using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RestoApp.Desktop.ViewModels;
using Avalonia.Interactivity;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace RestoApp.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(
            SeleccionarRutaBackupAsync,
            SeleccionarRutaRestoreAsync,
            SolicitarAutorizacionRestoreAsync);
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

        return archivo?.TryGetLocalPath() ?? archivo?.Path.LocalPath;
    }

    private async Task<string?> SeleccionarRutaRestoreAsync()
    {
        var archivos = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleccionar copia de seguridad (.bak) para restaurar",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Copia de seguridad de SQL Server (*.bak)")
                {
                    Patterns = ["*.bak"]
                }
            ]
        });

        var archivo = archivos?.Count > 0 ? archivos[0] : null;
        return archivo?.TryGetLocalPath() ?? archivo?.Path.LocalPath;
    }

    private async Task<bool> SolicitarAutorizacionRestoreAsync()
    {
        var empleadoService = App.Services?.GetService(typeof(EmpleadoService)) as EmpleadoService
            ?? new EmpleadoService(new EmpleadoRepository(new RestoAppDbContext()));

        var usuarios = await empleadoService.ObtenerUsuariosAutorizantesAsync();
        var dialog = new AutorizacionRestoreWindow(usuarios);
        return await dialog.ShowDialog<bool>(this);
    }

    private void MenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Removemos la clase 'active' de todos los botones del menú de forma segura
        BtnInicio?.Classes.Remove("active");
        BtnMenu?.Classes.Remove("active");
        BtnReservas?.Classes.Remove("active");
        BtnPersonal?.Classes.Remove("active");
        BtnCaja?.Classes.Remove("active");
        BtnEventos?.Classes.Remove("active");
        BtnReportes?.Classes.Remove("active");
        BtnBackup?.Classes.Remove("active");

        // 2. Identificamos qué botón disparó el evento y le añadimos la clase active
        if (sender is Button clickedButton)
        {
            clickedButton.Classes.Add("active");
        }
    }

    private void CerrarSesion(object? sender, RoutedEventArgs e)
    {
        SesionGlobal.UsuarioActual = string.Empty;
        SesionGlobal.NombreCompletoActual = string.Empty;
        SesionGlobal.DniActual = null;

        var loginWindow = new LoginWindow();
        loginWindow.Show();

        this.Close();
    }
}