namespace RestoApp.Desktop.ViewModels;

public class EmpleadoItemViewModel
{
    public long DniEmpleado { get; set; } 
    public string NombreCompleto { get; set; } = string.Empty;
    public string RolCargo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool PuedeEditar { get; set; }

    public string AvatarIcon => RolCargo.ToLower() switch
    {
        var r when r.Contains("dueño") || r.Contains("dueno") => "👑",
        var r when r.Contains("gerente") => "👨‍💼",
        var r when r.Contains("cajero") => "💵",
        var r when r.Contains("mozo") => "👨‍🍳",
        var r when r.Contains("recep") => "📋",
        _ => "👤"
    };

    public string RolBadgeBackground => RolCargo.ToLower() switch
    {
        var r when r.Contains("dueño") || r.Contains("dueno") => "#FADBD8",
        var r when r.Contains("gerente") => "#E8F8F5",
        var r when r.Contains("cajero") => "#FEF9E7",
        var r when r.Contains("mozo") => "#EAF2F8",
        _ => "#F2F4F4"
    };

    public string RolBadgeForeground => RolCargo.ToLower() switch
    {
        var r when r.Contains("dueño") || r.Contains("dueno") => "#7D6608",
        var r when r.Contains("gerente") => "#2980B9",
        var r when r.Contains("cajero") => "#D68910",
        var r when r.Contains("mozo") => "#27AE60",
        _ => "#34495E"
    };

    public string EstadoBadgeBackground => Estado == "Inactivo" ? "#FDEDEC" : "#E8F8F5";
    public string EstadoBadgeForeground => Estado == "Inactivo" ? "#E74C3C" : "#27AE60";
}