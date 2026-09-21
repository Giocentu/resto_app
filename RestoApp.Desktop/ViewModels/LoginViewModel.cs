using CommunityToolkit.Mvvm.ComponentModel;
using RestoApp.Entities;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nombreUsuario = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _mensajeError = string.Empty;

    public async Task<bool> IniciarSesionAsync()
    {
        
        await Task.Delay(500); 

        if (NombreUsuario == "dueño" && Password == "123")
        {
            SesionGlobal.RolActual = RolUsuario.Dueno;
            return true;
        }
        if (NombreUsuario == "gerente" && Password == "123")
        {
            SesionGlobal.RolActual = RolUsuario.Gerente;
            return true;
        }
        if (NombreUsuario == "cajero" && Password == "123")
        {
            SesionGlobal.RolActual = RolUsuario.Cajero;
            return true;
        }
        if (NombreUsuario == "recepcion" && Password == "123")
        {
            SesionGlobal.RolActual = RolUsuario.Recepcion;
            return true;
        }
        if (NombreUsuario == "mozo" && Password == "123")
        {
            SesionGlobal.RolActual = RolUsuario.Mozo;
            return true;
        }

        MensajeError = "Credenciales incorrectas.";
        return false;
    }
}