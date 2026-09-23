namespace RestoApp.Desktop.ViewModels;

public class TurnoItemViewModel
{
    public int IdTurno { get; set; }
    public string InicioTexto { get; set; } = string.Empty;
    public string FinTexto { get; set; } = string.Empty;
    public bool EsActivo { get; set; } = true;

    public string Descripcion => $"Turno #{IdTurno} ({InicioTexto} - {FinTexto})";
    public string EstadoBadge => EsActivo ? "Activo" : "Inactivo";
    public string EstadoBadgeBackground => EsActivo ? "#E8F8F5" : "#FDEDEC";
    public string EstadoBadgeForeground => EsActivo ? "#27AE60" : "#E74C3C";

    public string BotonTexto => EsActivo ? "🚫 Desactivar" : "✅ Activar";
    public string BotonBackground => EsActivo ? "#FDEDEC" : "#E8F8F5";
    public string BotonForeground => EsActivo ? "#E74C3C" : "#27AE60";
    public string BotonBorder => EsActivo ? "#E74C3C" : "#27AE60";
}
