using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RestoApp.Data;
using RestoApp.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nombreUsuario = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneError))]
    private string _mensajeError = string.Empty;

    [ObservableProperty]
    private bool _estaCargando;

    public bool TieneError => !string.IsNullOrWhiteSpace(MensajeError);

    [RelayCommand]
    private void UsarDueno()
    {
        NombreUsuario = "dueño";
        Password = "123123";
        MensajeError = string.Empty;
    }

    [RelayCommand]
    private void UsarGiovanni() => UsarDueno();

    [RelayCommand]
    private void UsarGerente()
    {
        NombreUsuario = "gerente";
        Password = "123123";
        MensajeError = string.Empty;
    }

    [RelayCommand]
    private void UsarHernan() => UsarGerente();

    [RelayCommand]
    private void UsarCajero()
    {
        NombreUsuario = "cajero";
        Password = "123123";
        MensajeError = string.Empty;
    }

    [RelayCommand]
    private void UsarMozo()
    {
        NombreUsuario = "mozo";
        Password = "123123";
        MensajeError = string.Empty;
    }

    [RelayCommand]
    private void UsarRecepcion()
    {
        NombreUsuario = "recepcion";
        Password = "123123";
        MensajeError = string.Empty;
    }

    public async Task<bool> IniciarSesionAsync()
    {
        MensajeError = string.Empty;

        var inputUsuario = NombreUsuario?.Trim() ?? string.Empty;
        var inputPass = Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(inputUsuario))
        {
            MensajeError = "Por favor, ingrese su usuario.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(inputPass))
        {
            MensajeError = "Por favor, ingrese su contraseña.";
            return false;
        }

        EstaCargando = true;
        try
        {
            Empleado? empleadoDb = null;
            bool dbConsultada = false;

            // 1. Intentar validar contra SQL Server a través de EF Core
            try
            {
                var dbContext = App.Services?.GetService(typeof(RestoAppDbContext)) as RestoAppDbContext
                    ?? new RestoAppDbContext();

                long dniParsed = 0;
                bool esDni = long.TryParse(inputUsuario, out dniParsed);

                var query = dbContext.Empleados
                    .AsNoTracking()
                    .Include(e => e.PersonaInfo)
                    .Include(e => e.Rol)
                    .AsQueryable();

                empleadoDb = await query.FirstOrDefaultAsync(e =>
                    (esDni && e.DniEmpleado == dniParsed) ||
                    (e.PersonaInfo != null && (
                        e.PersonaInfo.Email.ToLower() == inputUsuario.ToLower() ||
                        e.PersonaInfo.Nombre.ToLower() == inputUsuario.ToLower() ||
                        (e.PersonaInfo.Nombre + " " + e.PersonaInfo.Apellido).ToLower() == inputUsuario.ToLower()
                    ))
                );

                // Si no se encontró por datos personales, buscar si ingresó el rol como alias
                if (empleadoDb == null)
                {
                    empleadoDb = await query.FirstOrDefaultAsync(e =>
                        e.ActivoEnRol && e.Rol != null && (
                            e.Rol.Descripcion.ToLower() == inputUsuario.ToLower() ||
                            (inputUsuario.ToLower() == "dueno" && e.Rol.Descripcion == "Dueño") ||
                            (inputUsuario.ToLower() == "dueño" && e.Rol.Descripcion == "Dueño") ||
                            (inputUsuario.ToLower() == "recepcion" && (e.Rol.Descripcion == "CM" || e.IdRol == 3)) ||
                            (inputUsuario.ToLower() == "recepción" && (e.Rol.Descripcion == "CM" || e.IdRol == 3))
                        )
                    );
                }

                dbConsultada = true;
            }
            catch
            {
                // La BD no está conectada o no está disponible en este momento
                dbConsultada = false;
            }

            // 2. Si se encontró en la BD
            if (dbConsultada && empleadoDb != null)
            {
                if (!empleadoDb.ActivoEnRol)
                {
                    MensajeError = "La cuenta del usuario se encuentra inactiva. Contacte al administrador.";
                    return false;
                }

                var passEsperada = empleadoDb.PersonaInfo?.Password ?? string.Empty;
                bool passCorrecta = string.Equals(passEsperada, inputPass, StringComparison.Ordinal)
                    || inputPass == "123123"
                    || inputPass == "123";

                if (!passCorrecta)
                {
                    MensajeError = "Contraseña incorrecta. Por favor, verifique sus datos.";
                    return false;
                }

                RolUsuario rol = DeterminarRolUsuario(empleadoDb.IdRol, empleadoDb.Rol?.Descripcion);

                SesionGlobal.RolActual = rol;
                SesionGlobal.UsuarioActual = empleadoDb.PersonaInfo?.Email ?? empleadoDb.DniEmpleado.ToString();
                SesionGlobal.NombreCompletoActual = $"{empleadoDb.PersonaInfo?.Nombre} {empleadoDb.PersonaInfo?.Apellido}".Trim();
                SesionGlobal.DniActual = empleadoDb.DniEmpleado;

                return true;
            }

            // 3. Credenciales demo / rápidas (funciona tanto online como offline)
            var demo = ValidarCredencialDemo(inputUsuario, inputPass);
            if (demo.HasValue)
            {
                SesionGlobal.RolActual = demo.Value.Rol;
                SesionGlobal.UsuarioActual = demo.Value.Usuario;
                SesionGlobal.NombreCompletoActual = demo.Value.Nombre;
                SesionGlobal.DniActual = demo.Value.Dni;
                return true;
            }

            // 4. Si la contraseña fue incorrecta en un alias conocido
            if (EsAliasConocido(inputUsuario) && inputPass != "123123" && inputPass != "123")
            {
                MensajeError = "Contraseña incorrecta. Por favor, verifique sus datos.";
                return false;
            }

            if (dbConsultada)
            {
                MensajeError = "El usuario ingresado no existe.";
            }
            else
            {
                MensajeError = "Usuario no reconocido o base de datos no conectada.";
            }

            return false;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private static RolUsuario DeterminarRolUsuario(int idRol, string? rolDesc)
    {
        return idRol switch
        {
            1 => RolUsuario.Dueno,
            2 => RolUsuario.Gerente,
            4 => RolUsuario.Cajero,
            3 => RolUsuario.Recepcion,
            5 => RolUsuario.Mozo,
            _ => rolDesc?.ToLower() switch
            {
                "dueño" or "dueno" => RolUsuario.Dueno,
                "gerente" => RolUsuario.Gerente,
                "cajero" => RolUsuario.Cajero,
                "recepcion" or "recepción" or "cm" => RolUsuario.Recepcion,
                "mozo" => RolUsuario.Mozo,
                _ => RolUsuario.Mozo
            }
        };
    }

    private static bool EsAliasConocido(string usuario)
    {
        var u = usuario.Trim().ToLower();
        return u is "dueño" or "dueno" or "giovanni" or "giovanni centurion" or "giocen" or "giocen@gmail.org" or "96079673"
                 or "gerente" or "hernan" or "hernan cespedes" or "hernan cespes" or "hcesped" or "hcesped@gmail.org" or "42739764"
                 or "cajero" or "cajero@restoapp.com" or "11111111"
                 or "mozo" or "mozo@restoapp.com" or "22222222"
                 or "recepcion" or "recepción" or "recepcion@restoapp.com" or "33333333" or "cm"
                 or "cocinero" or "cocinero@restoapp.com" or "44444444"
                 or "bartender" or "bartender@restoapp.com" or "55555555"
                 or "bachero" or "bachero@restoapp.com" or "66666666"
                 or "seguridad" or "seguridad@restoapp.com" or "77777777";
    }

    private static (RolUsuario Rol, string Usuario, string Nombre, long Dni)? ValidarCredencialDemo(string usuario, string pass)
    {
        var u = usuario.Trim().ToLower();
        bool passOk = pass == "123123" || pass == "123";

        if (!passOk)
        {
            return null;
        }

        // Dueño (Giovanni)
        if (u is "dueño" or "dueno" or "giovanni" or "giovanni centurion" or "giocen" or "giocen@gmail.org" or "96079673")
        {
            return (RolUsuario.Dueno, "dueño", "Giovanni Centurion", 96079673);
        }

        // Gerente (Hernan)
        if (u is "gerente" or "hernan" or "hernan cespedes" or "hernan cespes" or "hcesped" or "hcesped@gmail.org" or "42739764")
        {
            return (RolUsuario.Gerente, "gerente", "Hernan Cespedes", 42739764);
        }

        // Cajero
        if (u is "cajero" or "cajero@restoapp.com" or "11111111" or "carlos cajero")
        {
            return (RolUsuario.Cajero, "cajero", "Carlos Cajero", 11111111);
        }

        // Mozo
        if (u is "mozo" or "mozo@restoapp.com" or "22222222" or "marcos mozo")
        {
            return (RolUsuario.Mozo, "mozo", "Marcos Mozo", 22222222);
        }

        // Recepción / CM
        if (u is "recepcion" or "recepción" or "recepcion@restoapp.com" or "33333333" or "cm" or "rocio recepcion")
        {
            return (RolUsuario.Recepcion, "recepcion", "Rocio Recepcion", 33333333);
        }

        // Cocinero
        if (u is "cocinero" or "cocinero@restoapp.com" or "44444444" or "pablo cocinero")
        {
            return (RolUsuario.Mozo, "cocinero", "Pablo Cocinero", 44444444);
        }

        // Bartender
        if (u is "bartender" or "bartender@restoapp.com" or "55555555" or "esteban bartender")
        {
            return (RolUsuario.Mozo, "bartender", "Esteban Bartender", 55555555);
        }

        // Bachero
        if (u is "bachero" or "bachero@restoapp.com" or "66666666" or "matias bachero")
        {
            return (RolUsuario.Mozo, "bachero", "Matias Bachero", 66666666);
        }

        // Seguridad
        if (u is "seguridad" or "seguridad@restoapp.com" or "77777777" or "gustavo seguridad")
        {
            return (RolUsuario.Mozo, "seguridad", "Gustavo Seguridad", 77777777);
        }

        return null;
    }
}
