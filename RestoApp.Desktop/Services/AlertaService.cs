using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using RestoApp.Desktop.Views;

namespace RestoApp.Desktop.Services;

public static class AlertaService
{
    private static bool _mostrandoAlerta = false;

    public static async Task MostrarAlertaConexionAsync(string? mensaje = null)
    {
        mensaje ??= "No se pudo conectar a la base de datos. Por favor, verifique que la base de datos esté en ejecución y la conexión sea correcta.";

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_mostrandoAlerta) return;
            _mostrandoAlerta = true;

            try
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var activeWindow = desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
                    var alertaWin = new AlertaWindow(mensaje);
                    if (activeWindow != null && activeWindow.IsVisible)
                    {
                        await alertaWin.ShowDialog(activeWindow);
                    }
                    else
                    {
                        alertaWin.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ALERTA DB ERROR] {ex.Message}");
            }
            finally
            {
                _mostrandoAlerta = false;
            }
        });
    }
}
