using RestoApp.Entities;

namespace RestoApp.Desktop;

public static class SesionGlobal
{
    // Usamos el Enum para que sea 100% claro qué rol está activo
    public static RolUsuario RolActual { get; set; } = RolUsuario.Dueno;

    public static string UsuarioActual { get; set; } = string.Empty;
    public static string NombreCompletoActual { get; set; } = string.Empty;
    public static long? DniActual { get; set; }
}