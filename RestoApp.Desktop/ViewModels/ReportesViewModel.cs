using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class ReportesViewModel : ObservableObject
{
    private readonly ReporteService? _reporteService;

    [ObservableProperty]
    private DateTimeOffset _fechaInicio = DateTimeOffset.Now.AddDays(-7);

    [ObservableProperty]
    private DateTimeOffset _fechaFin = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _cargando;

    // Métricas / KPIs
    [ObservableProperty]
    private double _totalVentas;

    [ObservableProperty]
    private int _cuentasCerradas;

    [ObservableProperty]
    private double _ticketPromedio;

    [ObservableProperty]
    private int _totalReservas;

    [ObservableProperty]
    private int _totalItemsVendidos;

    // Colecciones de Tablas
    [ObservableProperty]
    private ObservableCollection<ReporteTopPlatoDto> _topPlatos = new();

    [ObservableProperty]
    private ObservableCollection<ReporteMetodoPagoDto> _ventasPorMetodo = new();

    [ObservableProperty]
    private ObservableCollection<ReporteMozoDto> _rendimientoMozos = new();

    // Pestaña activa
    [ObservableProperty]
    private string _pestañaSeleccionada = "Platos";

    public bool EsTabPlatos => PestañaSeleccionada == "Platos";
    public bool EsTabPagos => PestañaSeleccionada == "Pagos";
    public bool EsTabMozos => PestañaSeleccionada == "Mozos";

    public ReportesViewModel(ReporteService? reporteService)
    {
        _reporteService = reporteService;
        _ = GenerarReporteAsync();
    }

    [RelayCommand]
    public async Task GenerarReporteAsync()
    {
        if (_reporteService == null) return;
        Cargando = true;

        try
        {
            var inicio = FechaInicio.DateTime.Date;
            var fin = FechaFin.DateTime.Date.AddDays(1).AddTicks(-1);

            var kpis = await _reporteService.ObtenerKpisAsync(inicio, fin);
            TotalVentas = kpis.TotalVentas;
            CuentasCerradas = kpis.CuentasCerradas;
            TicketPromedio = kpis.TicketPromedio;
            TotalReservas = kpis.TotalReservas;
            TotalItemsVendidos = kpis.TotalItemsVendidos;

            var platos = await _reporteService.ObtenerTopPlatosAsync(inicio, fin, top: 10);
            TopPlatos = new ObservableCollection<ReporteTopPlatoDto>(platos);

            var pagos = await _reporteService.ObtenerVentasMetodoPagoAsync(inicio, fin);
            VentasPorMetodo = new ObservableCollection<ReporteMetodoPagoDto>(pagos);

            var mozos = await _reporteService.ObtenerRendimientoMozosAsync(inicio, fin);
            RendimientoMozos = new ObservableCollection<ReporteMozoDto>(mozos);
        }
        finally
        {
            Cargando = false;
        }
    }

    [RelayCommand]
    private void EstablecerPresetHoy()
    {
        FechaInicio = DateTimeOffset.Now.Date;
        FechaFin = DateTimeOffset.Now.Date;
        _ = GenerarReporteAsync();
    }

    [RelayCommand]
    private void EstablecerPresetSemana()
    {
        FechaInicio = DateTimeOffset.Now.Date.AddDays(-7);
        FechaFin = DateTimeOffset.Now.Date;
        _ = GenerarReporteAsync();
    }

    [RelayCommand]
    private void EstablecerPresetMes()
    {
        FechaInicio = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        FechaFin = DateTimeOffset.Now.Date;
        _ = GenerarReporteAsync();
    }

    [RelayCommand]
    private void CambiarTab(string tab)
    {
        PestañaSeleccionada = tab;
        OnPropertyChanged(nameof(EsTabPlatos));
        OnPropertyChanged(nameof(EsTabPagos));
        OnPropertyChanged(nameof(EsTabMozos));
    }
}

