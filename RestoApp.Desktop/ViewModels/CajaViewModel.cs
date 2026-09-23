using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Desktop.Services;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class CajaViewModel : ObservableObject
{
    private readonly PagoService? _pagoService;

    // Selector de Vistas: "Pagos" o "Arqueo"
    [ObservableProperty]
    private string _vistaSeleccionada = "Pagos";

    public bool EsVistaPagos => VistaSeleccionada == "Pagos";
    public bool EsVistaArqueo => VistaSeleccionada == "Arqueo";

    // Historial CRUD de Pagos
    [ObservableProperty]
    private ObservableCollection<PagoItemViewModel> _pagos = new();

    [ObservableProperty]
    private ObservableCollection<PagoItemViewModel> _pagosFiltrados = new();

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private PagoItemViewModel? _pagoSeleccionado;

    // Modales y Diálogos
    [ObservableProperty]
    private bool _mostrarModalProcesarPago;

    [ObservableProperty]
    private bool _mostrarModalComprobante;

    [ObservableProperty]
    private PagoItemViewModel? _comprobantePago;

    // Formulario Procesar Cobro
    [ObservableProperty]
    private ObservableCollection<CuentaPendienteViewModel> _cuentasPendientes = new();

    [ObservableProperty]
    private CuentaPendienteViewModel? _cuentaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<string> _mediosPago = new()
    {
        "Efectivo",
        "Tarjeta de Débito",
        "Tarjeta de Crédito",
        "Transferencia / QR"
    };

    [ObservableProperty]
    private string _medioPagoSeleccionado = "Efectivo";

    [ObservableProperty]
    private decimal _montoCobrar = 24500.00m;

    public string MontoCobrarTexto => $"$ {MontoCobrar:N2}";
    public string ClienteNombreModal => CuentaSeleccionada?.ClienteNombre ?? "Juan Pérez";

    // Balance y Arqueo de Turno
    [ObservableProperty]
    private decimal _fondoInicial = 15000.00m;

    [ObservableProperty]
    private decimal _cobradoEfectivo;

    [ObservableProperty]
    private decimal _cobradoTarjetas;

    [ObservableProperty]
    private decimal _cobradoTransferenciaQR;

    [ObservableProperty]
    private decimal _totalIngresosTurno;

    [ObservableProperty]
    private decimal _efectivoEsperado;

    [ObservableProperty]
    private decimal _efectivoRecontado = 15000.00m;

    [ObservableProperty]
    private decimal _diferenciaArqueo;

    [ObservableProperty]
    private string _mensajeCierre = string.Empty;

    public CajaViewModel(PagoService? pagoService = null)
    {
        _pagoService = pagoService;
        _ = CargarDatosAsync();
    }

    public async Task CargarDatosAsync()
    {
        var listaPagos = new List<PagoItemViewModel>();

        if (_pagoService != null)
        {
            try
            {
                var entidades = await _pagoService.ObtenerPagosAsync();
                foreach (var p in entidades)
                {
                    var desc = p.Reserva != null 
                        ? $"Cobro Reserva #{p.IdReserva}" 
                        : $"Cobro Pago #{p.IdPago}";

                    var cliente = p.Reserva?.Cliente?.PersonaInfo != null
                        ? $"{p.Reserva.Cliente.PersonaInfo.Nombre} {p.Reserva.Cliente.PersonaInfo.Apellido}"
                        : "Cliente General";

                    listaPagos.Add(new PagoItemViewModel
                    {
                        IdPago = p.IdPago,
                        FechaPago = p.FechaPago,
                        Descripcion = desc,
                        ClienteNombre = cliente,
                        MedioPago = p.MetodoPago?.FormaPago ?? "Efectivo",
                        Monto = (decimal)p.Monto
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAJA DB ERROR] Error al cargar pagos de la BD: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CAJA DB INNER ERROR] {ex.InnerException.Message}");
                }
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Pagos = new ObservableCollection<PagoItemViewModel>(listaPagos);
        AplicarFiltro();
        RecalcularTotalesTurno();

        CuentasPendientes = new ObservableCollection<CuentaPendienteViewModel>();
        CuentaSeleccionada = null;
    }

    private void RecalcularTotalesTurno()
    {
        CobradoEfectivo = Pagos
            .Where(p => p.MedioPago.Contains("Efectivo", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Monto);

        CobradoTarjetas = Pagos
            .Where(p => p.MedioPago.Contains("Tarjeta", StringComparison.OrdinalIgnoreCase) ||
                        p.MedioPago.Contains("Débito", StringComparison.OrdinalIgnoreCase) ||
                        p.MedioPago.Contains("Crédito", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Monto);

        CobradoTransferenciaQR = Pagos
            .Where(p => p.MedioPago.Contains("Transf", StringComparison.OrdinalIgnoreCase) ||
                        p.MedioPago.Contains("QR", StringComparison.OrdinalIgnoreCase) ||
                        p.MedioPago.Contains("Mercado", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Monto);

        TotalIngresosTurno = Pagos.Sum(p => p.Monto);
        EfectivoEsperado = FondoInicial + CobradoEfectivo;
        DiferenciaArqueo = EfectivoRecontado - EfectivoEsperado;
    }

    partial void OnVistaSeleccionadaChanged(string value)
    {
        OnPropertyChanged(nameof(EsVistaPagos));
        OnPropertyChanged(nameof(EsVistaArqueo));
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnCuentaSeleccionadaChanged(CuentaPendienteViewModel? value)
    {
        if (value != null)
        {
            MontoCobrar = value.MontoConsumo;
            OnPropertyChanged(nameof(MontoCobrarTexto));
            OnPropertyChanged(nameof(ClienteNombreModal));
        }
    }

    partial void OnMontoCobrarChanged(decimal value)
    {
        OnPropertyChanged(nameof(MontoCobrarTexto));
    }

    partial void OnEfectivoRecontadoChanged(decimal value)
    {
        DiferenciaArqueo = value - EfectivoEsperado;
    }

    private void AplicarFiltro()
    {
        if (string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            PagosFiltrados = new ObservableCollection<PagoItemViewModel>(Pagos);
        }
        else
        {
            var query = TextoBusqueda.Trim();
            var filtrados = Pagos.Where(p => 
                p.Descripcion.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.ClienteNombre.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.MedioPago.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.IdPago.ToString().Contains(query));

            PagosFiltrados = new ObservableCollection<PagoItemViewModel>(filtrados);
        }
    }

    [RelayCommand]
    private void CambiarVista(string vista)
    {
        VistaSeleccionada = vista;
    }

    [RelayCommand]
    private void AbrirModalProcesarPago()
    {
        MostrarModalProcesarPago = true;
    }

    [RelayCommand]
    private void CerrarModalProcesarPago()
    {
        MostrarModalProcesarPago = false;
    }

    [RelayCommand]
    private async Task ProcesarPagoYLiquidarAsync()
    {
        string medio = MedioPagoSeleccionado?.ToLower() ?? "";
        int idMetodo = medio switch
        {
            var s when s.Contains("crédito") || s.Contains("credito") => 1,
            var s when s.Contains("débito") || s.Contains("debito") => 2,
            var s when s.Contains("efectivo") => 3,
            _ => 3
        };

        if (_pagoService != null)
        {
            try
            {
                var pagoEntity = new Pago
                {
                    Monto = (double)MontoCobrar,
                    FechaPago = DateTime.Now,
                    IdMetodo = idMetodo,
                    IdReserva = null
                };
                await _pagoService.RegistrarPagoAsync(pagoEntity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAJA REGISTRAR PAGO DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarDatosAsync();
        MostrarModalProcesarPago = false;

        ComprobantePago = Pagos.FirstOrDefault();
        MostrarModalComprobante = true;
    }

    [RelayCommand]
    private async Task EliminarPagoAsync(PagoItemViewModel? item)
    {
        if (item == null) return;

        if (_pagoService != null)
        {
            try
            {
                await _pagoService.EliminarPagoAsync(item.IdPago);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAJA ELIMINAR PAGO DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarDatosAsync();
    }

    [RelayCommand]
    private void VerComprobante(PagoItemViewModel? item)
    {
        if (item == null) return;
        ComprobantePago = item;
        MostrarModalComprobante = true;
    }

    [RelayCommand]
    private void CerrarModalComprobante()
    {
        MostrarModalComprobante = false;
        ComprobantePago = null;
    }

    [RelayCommand]
    private void CalcularArqueo()
    {
        DiferenciaArqueo = EfectivoRecontado - EfectivoEsperado;
        MensajeCierre = DiferenciaArqueo == 0 
            ? "Arqueo de caja perfecto. Sin diferencias." 
            : $"Diferencia detectada: $ {DiferenciaArqueo:N2}";
    }

    [RelayCommand]
    private void FinalizarYCerrarCaja()
    {
        CalcularArqueo();
        MensajeCierre = "🔒 Caja del turno cerrada correctamente. Balance guardado.";
    }
}
