using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public interface IReporteRepository
{
    Task<ReporteKpiDto> ObtenerKpisAsync(DateTime inicio, DateTime fin);
    Task<IEnumerable<ReporteTopPlatoDto>> ObtenerTopPlatosAsync(DateTime inicio, DateTime fin, int top = 10);
    Task<IEnumerable<ReporteMetodoPagoDto>> ObtenerVentasMetodoPagoAsync(DateTime inicio, DateTime fin);
    Task<IEnumerable<ReporteMozoDto>> ObtenerRendimientoMozosAsync(DateTime inicio, DateTime fin);
}

