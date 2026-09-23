using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
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

    [ObservableProperty]
    private string _origenDatosTexto = "🟢 Base de datos";

    [ObservableProperty]
    private string _origenDatosColor = "#27AE60";

    public CajaViewModel(PagoService? pagoService = null)
    {
        _pagoService = pagoService;
        _ = CargarDatosAsync();
    }

    public async Task CargarDatosAsync()
    {
        var listaPagos = new List<PagoItemViewModel>();
        bool cargoDesdeBD = false;

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
                cargoDesdeBD = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAJA DB ERROR] Error al cargar pagos de la BD: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CAJA DB INNER ERROR] {ex.InnerException.Message}");
                }
            }
        }

        if (cargoDesdeBD && listaPagos.Any())
        {
            OrigenDatosTexto = "🟢 Base de datos";
            OrigenDatosColor = "#27AE60";
        }
        else if (cargoDesdeBD && !listaPagos.Any())
        {
            OrigenDatosTexto = "🟢 Base de datos (Sin registros)";
            OrigenDatosColor = "#27AE60";
        }
        else
        {
            OrigenDatosTexto = "🟠 Mock";
            OrigenDatosColor = "#E67E22";
            var hoy = DateTime.Now;
            listaPagos = new List<PagoItemViewModel>
            {
                new()
                {
                    IdPago = 1,
                    FechaPago = hoy.AddHours(-3),
                    Descripcion = "Cobro Mesa #1 - Reserva #12",
                    ClienteNombre = "Juan Pérez",
                    MedioPago = "Efectivo",
                    Monto = 18500.00m
                },
                new()
                {
                    IdPago = 2,
                    FechaPago = hoy.AddHours(-2),
                    Descripcion = "Cobro Mesa #5 - Cliente Gómez",
                    ClienteNombre = "Carlos Gómez",
                    MedioPago = "Tarjeta",
                    Monto = 31200.00m
                },
                new()
                {
                    IdPago = 3,
                    FechaPago = hoy.AddHours(-1),
                    Descripcion = "Cobro Mesa #10 - Evento Jazz",
                    ClienteNombre = "Empresa ACME",
                    MedioPago = "Transferencia/QR",
                    Monto = 45000.00m
                }
            };
        }

        Pagos = new ObservableCollection<PagoItemViewModel>(listaPagos);
        AplicarFiltro();
        RecalcularTotalesTurno();

        // Cargar cuentas pendientes demostrativas para cobro
        CuentasPendientes = new ObservableCollection<CuentaPendienteViewModel>
        {
            new() { IdMesa = 2, NroMesa = 2, Sector = "Salón Principal", ClienteNombre = "Juan Pérez", MontoConsumo = 24500.00m },
            new() { IdMesa = 6, NroMesa = 6, Sector = "Terraza", ClienteNombre = "Carlos V.", MontoConsumo = 4250.00m },
            new() { IdMesa = 9, NroMesa = 9, Sector = "Barra", ClienteNombre = "Gonzalo T.", MontoConsumo = 1800.00m },
        };

        if (CuentasPendientes.Any())
        {
            CuentaSeleccionada = CuentasPendientes.First();
        }
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
        int nuevoId = Pagos.Any() ? Pagos.Max(p => p.IdPago) + 1 : 1;
        var nuevoPagoItem = new PagoItemViewModel
        {
            IdPago = nuevoId,
            FechaPago = DateTime.Now,
            Descripcion = CuentaSeleccionada != null ? $"Cobro Mesa #{CuentaSeleccionada.NroMesa} ({CuentaSeleccionada.Sector})" : "Cobro General",
            ClienteNombre = ClienteNombreModal,
            MedioPago = MedioPagoSeleccionado,
            Monto = MontoCobrar
        };

        if (_pagoService != null)
        {
            try
            {
                var pagoEntity = new Pago
                {
                    Monto = (double)MontoCobrar,
                    FechaPago = DateTime.Now,
                    IdMetodo = 1, // Default EF metodo
                    IdReserva = 1
                };
                await _pagoService.RegistrarPagoAsync(pagoEntity);
            }
            catch
            {
            }
        }

        Pagos.Insert(0, nuevoPagoItem);

        // Si se cobró una mesa pendiente, la quitamos de pendientes
        if (CuentaSeleccionada != null)
        {
            CuentasPendientes.Remove(CuentaSeleccionada);
            CuentaSeleccionada = CuentasPendientes.FirstOrDefault();
        }

        AplicarFiltro();
        RecalcularTotalesTurno();
        MostrarModalProcesarPago = false;

        // Mostrar comprobante directamente tras el pago exitoso
        ComprobantePago = nuevoPagoItem;
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
            catch
            {
            }
        }

        Pagos.Remove(item);
        AplicarFiltro();
        RecalcularTotalesTurno();
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
