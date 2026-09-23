namespace RestoApp.Desktop.ViewModels;

public class ClienteItemViewModel
{
    public long DniCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long Telefono { get; set; }
    public bool EsActivo { get; set; } = true;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    public string EstadoTexto => EsActivo ? "Activo" : "Inactivo";
    public string EstadoBadgeBackground => EsActivo ? "#E8F8F5" : "#FDEDEC";
    public string EstadoBadgeForeground => EsActivo ? "#27AE60" : "#E74C3C";
}
