namespace RestoApp.Desktop.ViewModels;

public class ReservaItemViewModel
{
    public int IdReserva { get; set; }
    public string FechaHora { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public long DniCliente { get; set; }
    public int IdMesa { get; set; }
    public int? IdEvento { get; set; }
    public string NroMesa { get; set; } = string.Empty;
    public int CantidadPersonas { get; set; }
    public string EstadoTexto { get; set; } = "Confirmada";

    public string PersonasBadge => $"👥 {CantidadPersonas} pers.";
    public string MesaBadge => NroMesa.StartsWith("Mesa") ? NroMesa : $"Mesa {NroMesa}";
    public string EstadoBadgeBackground => EstadoTexto == "Cancelada" || EstadoTexto == "Inactiva" ? "#FDEDEC" : "#E8F8F5";
    public string EstadoBadgeForeground => EstadoTexto == "Cancelada" || EstadoTexto == "Inactiva" ? "#E74C3C" : "#27AE60";
}