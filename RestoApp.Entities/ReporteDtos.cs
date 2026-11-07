namespace RestoApp.Entities;

public class ReporteKpiDto
{
    public double TotalVentas { get; set; }
    public int CuentasCerradas { get; set; }
    public double TicketPromedio { get; set; }
    public int TotalReservas { get; set; }
    public int TotalItemsVendidos { get; set; }
}

public class ReporteTopPlatoDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public double TotalRecaudado { get; set; }
}

public class ReporteMetodoPagoDto
{
    public string MetodoPago { get; set; } = string.Empty;
    public double TotalCobrado { get; set; }
    public int CantidadOperaciones { get; set; }
    public double PorcentajeDelTotal { get; set; }
}

public class ReporteMozoDto
{
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public int MesasAtendidas { get; set; }
    public double FacturacionTotal { get; set; }
    public double PromedioPorMesa { get; set; }
}

