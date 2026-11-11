namespace RestoApp.Entities;

public class CuentaPendienteDto
{
    public int CuentaId { get; set; }
    public int IdMesa { get; set; }
    public int NroMesa { get; set; }
    public string Sector { get; set; } = string.Empty;
    public double MontoConsumo { get; set; }
    public int CantidadItems { get; set; }
    public DateTime FechaApertura { get; set; }
    public string AtendidoPor { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
}