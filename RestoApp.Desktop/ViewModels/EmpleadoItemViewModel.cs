namespace RestoApp.Desktop.ViewModels;

public class EmpleadoItemViewModel
{
    public int IdEmpleado { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string RolCargo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    // Propiedad individual para habilitar/deshabilitar acciones fila por fila
}