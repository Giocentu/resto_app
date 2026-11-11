using System;

namespace RestoApp.Entities;

public class CuentaPagoInfoDto
{
    public int CuentaId { get; set; }
    public int IdMesa { get; set; }
    public int NroMesa { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string AtendidoPor { get; set; } = string.Empty;
}

