using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class ReporteService
{
    private readonly IReporteRepository _reporteRepository;

    public ReporteService(IReporteRepository reporteRepository)
    {
        _reporteRepository = reporteRepository;
    }

    public Task<ReporteKpiDto> ObtenerKpisAsync(DateTime inicio, DateTime fin) 
        => _reporteRepository.ObtenerKpisAsync(inicio, fin);

    public Task<IEnumerable<ReporteTopPlatoDto>> ObtenerTopPlatosAsync(DateTime inicio, DateTime fin, int top = 10) 
        => _reporteRepository.ObtenerTopPlatosAsync(inicio, fin, top);

    public Task<IEnumerable<ReporteMetodoPagoDto>> ObtenerVentasMetodoPagoAsync(DateTime inicio, DateTime fin) 
        => _reporteRepository.ObtenerVentasMetodoPagoAsync(inicio, fin);

    public Task<IEnumerable<ReporteMozoDto>> ObtenerRendimientoMozosAsync(DateTime inicio, DateTime fin) 
        => _reporteRepository.ObtenerRendimientoMozosAsync(inicio, fin);
}

