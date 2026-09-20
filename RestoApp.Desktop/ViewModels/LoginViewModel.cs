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
        // TODO: Reemplazar esto con una consulta real a tu base de datos mediante un servicio
        // Ejemplo: var usuarioDb = await _usuarioService.ValidarLoginAsync(NombreUsuario, Password);
        
        await Task.Delay(500); // Simulamos el tiempo de carga de la BD

        if (NombreUsuario == "admin" && Password == "123")
        {
            SesionGlobal.RolActual = RolUsuario.Dueno;
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