namespace RestoApp.Desktop.ViewModels;

public class EmpleadoItemViewModel
{
    // Cambiamos 'int IdEmpleado' por 'long DniEmpleado' para que coincida con tu BD
    public long DniEmpleado { get; set; } 
    
    public string NombreCompleto { get; set; } = string.Empty;
    public string RolCargo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool PuedeEditar { get; set; }
}