using RestoApp.Entities;

namespace RestoApp.Desktop;

public static class SesionGlobal
{
    // Usamos el Enum para que sea 100% claro qué rol está activo
    public static RolUsuario RolActual { get; set; } = RolUsuario.Dueno; 
}