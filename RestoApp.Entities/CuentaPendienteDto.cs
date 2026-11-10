namespace RestoApp.Entities;

public class CuentaPendienteDto
{
    public int CuentaId { get; set; }
    public int IdMesa { get; set; }
    public int NroMesa { get; set; }
    public string Sector { get; set; } = string.Empty;
    public double MontoConsumo { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
}