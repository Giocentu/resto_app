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
    private readonly MesaService _mesaService;
    private readonly MenuService _menuService;

    [ObservableProperty]
    private ObservableCollection<Mesa> _mesasLibres = new();

    [ObservableProperty]
    private ObservableCollection<MenuItem> _menuActivo = new();

    [ObservableProperty]
    private Mesa? _mesaNuevaSeleccionada;

    [ObservableProperty]
    private MenuItem? _itemInicialSeleccionado;

    [ObservableProperty]
    private int _cantidadInicial = 1;

    [ObservableProperty]
    private bool _mostrarModalNuevaCuenta;

    [ObservableProperty]
    private string _mensajeNuevaCuenta = string.Empty;
    // Selector de Vistas: "Pagos" o "Arqueo"
    [ObservableProperty]
    private string _vistaSeleccionada = "Pagos";

    [ObservableProperty]
    private bool _mostrarModalAgregarItem;

    [ObservableProperty]
    private CuentaPendienteViewModel? _cuentaParaAgregarItem;

    [ObservableProperty]
    private MenuItem? _itemParaAgregar;

    [ObservableProperty]
    private int _cantidadItemAgregar = 1;

    [ObservableProperty]
    private string _mensajeAgregarItem = string.Empty;

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
    private ObservableCollection<MetodoPago> _metodosPago = new();

    [ObservableProperty]
    private MetodoPago? _metodoPagoSeleccionado;

    [ObservableProperty]
    private decimal _montoCobrar;

    public string MontoCobrarTexto => $"$ {MontoCobrar:N2}";
    public string ClienteNombreModal => CuentaSeleccionada?.ClienteNombre ?? "Seleccione una Cuenta";

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
    private string _mensajePago = string.Empty;

    public CajaViewModel(
        PagoService pagoService,
        MesaService mesaService,
        MenuService menuService)
    {
        _pagoService = pagoService;
        _mesaService = mesaService;
        _menuService = menuService;
        _ = CargarDatosAsync();
    }
    [RelayCommand]
    private async Task AbrirModalAgregarItemAsync()
    {
        MensajeAgregarItem = string.Empty;
    
        if (_pagoService == null)
        {
            MensajeAgregarItem = "El servicio de pagos no está disponible.";
            return;
        }
    
        try
        {
            var cuentas = await _pagoService.ObtenerCuentasPendientesAsync();
    
            CuentasPendientes = new ObservableCollection<CuentaPendienteViewModel>(
                cuentas.Select(c => new CuentaPendienteViewModel
                {
                    CuentaId = c.CuentaId,
                    IdMesa = c.IdMesa,
                    NroMesa = c.NroMesa,
                    Sector = c.Sector,
                    MontoConsumo = (decimal)c.MontoConsumo,
                    ClienteNombre = c.ClienteNombre
                }));
    
            var productos = await _menuService.ObtenerMenuAsync(soloActivos: true);
            MenuActivo = new ObservableCollection<MenuItem>(
                productos.Where(p => p.Activo));
    
            CuentaParaAgregarItem = CuentasPendientes.FirstOrDefault();
            ItemParaAgregar = MenuActivo.FirstOrDefault();
            CantidadItemAgregar = 1;
            MostrarModalAgregarItem = true;
    
            if (CuentasPendientes.Count == 0)
                MensajeAgregarItem = "No hay cuentas abiertas.";
    
            if (MenuActivo.Count == 0)
                MensajeAgregarItem = "No hay productos activos.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CARGAR CUENTAS/PRODUCTOS ERROR] {ex}");
            MensajeAgregarItem = $"No se pudieron cargar cuentas o productos: {ex.Message}";
            MostrarModalAgregarItem = true;
        }
    }
    [RelayCommand]
    private void CerrarModalAgregarItem()
    {
        MostrarModalAgregarItem = false;
        MensajeAgregarItem = string.Empty;
    }   

    [RelayCommand]
    private async Task AgregarItemACuentaAsync()
    {
        if (_pagoService == null)
        {
            MensajeAgregarItem = "El servicio de pagos no está disponible.";
            return;
        }

        if (CuentaParaAgregarItem == null)
        {
            MensajeAgregarItem = "Seleccione una cuenta abierta.";
            return;
        }

        if (ItemParaAgregar == null)
        {
            MensajeAgregarItem = "Seleccione un producto.";
            return;
        }

        if (CantidadItemAgregar <= 0)
        {
            MensajeAgregarItem = "La cantidad debe ser mayor que cero.";
            return;
        }

        var cuentaId = CuentaParaAgregarItem.CuentaId;

        try
        {
            await _pagoService.AgregarItemACuentaAsync(
                cuentaId,
                ItemParaAgregar.IdItem,
                CantidadItemAgregar);

            var cuentas = await _pagoService.ObtenerCuentasPendientesAsync();

            CuentasPendientes = new ObservableCollection<CuentaPendienteViewModel>(
                cuentas.Select(c => new CuentaPendienteViewModel
                {
                    CuentaId = c.CuentaId,
                    IdMesa = c.IdMesa,
                    NroMesa = c.NroMesa,
                    Sector = c.Sector,
                    MontoConsumo = (decimal)c.MontoConsumo,
                    ClienteNombre = c.ClienteNombre
                }));

            CuentaParaAgregarItem =
                CuentasPendientes.FirstOrDefault(c => c.CuentaId == cuentaId);

            MensajeAgregarItem = $"Producto agregado a la cuenta #{cuentaId}.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA AGREGAR ITEM ERROR] {ex}");
            MensajeAgregarItem = $"No se pudo agregar el producto: {ex.Message}";
        }
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

        var cuentas = await _pagoService.ObtenerCuentasPendientesAsync();

        CuentasPendientes = new ObservableCollection<CuentaPendienteViewModel>(
            cuentas.Select(c => new CuentaPendienteViewModel
            {
                CuentaId = c.CuentaId,
                IdMesa = c.IdMesa,
                NroMesa = c.NroMesa,
                Sector = c.Sector,
                MontoConsumo = (decimal)c.MontoConsumo,
                ClienteNombre = c.ClienteNombre
            }));

        var metodos = await _pagoService.ObtenerMetodosPagoAsync();
        MetodosPago = new ObservableCollection<MetodoPago>(metodos);

        MetodoPagoSeleccionado = MetodosPago.FirstOrDefault(
            m => m.FormaPago.Contains("Efectivo", StringComparison.OrdinalIgnoreCase))
            ?? MetodosPago.FirstOrDefault();

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
        MontoCobrar = value?.MontoConsumo ?? 0m;
        OnPropertyChanged(nameof(MontoCobrarTexto));
        OnPropertyChanged(nameof(ClienteNombreModal));
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
        MensajePago = string.Empty;

        if (CuentaSeleccionada == null)
        {
            MensajePago = "Seleccione una cuenta pendiente.";
            return;
        }

        if (MetodoPagoSeleccionado == null)
        {
            MensajePago = "Seleccione un medio de pago.";
            return;
        }

        if (_pagoService == null)
        {
            MensajePago = "El servicio de pagos no está disponible.";
            return;
        }

        try
        {
            var nuevoPagoId = await _pagoService.CerrarCuentaAsync(
                CuentaSeleccionada.CuentaId,
                MetodoPagoSeleccionado.IdMetodo);

            await CargarDatosAsync();
            MostrarModalProcesarPago = false;

            ComprobantePago = Pagos.FirstOrDefault(p => p.IdPago == nuevoPagoId);
            MostrarModalComprobante = ComprobantePago != null;

            if (ComprobantePago == null)
            {
                MensajePago = $"La cuenta se cerró, pero no se encontró el comprobante del pago #{nuevoPagoId}.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CERRAR CUENTA ERROR] {ex}");
            MensajePago = $"No se pudo cerrar la cuenta: {ex.Message}";
        }
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

    [RelayCommand]
    private async Task AbrirModalNuevaCuentaAsync()
    {
        MensajeNuevaCuenta = string.Empty;

        try
        {
            var mesas = await _mesaService.ObtenerMesasAsync();

            MesasLibres = new ObservableCollection<Mesa>(
                mesas.Where(m =>
                    m.EsActivo &&
                    string.Equals(
                        m.Estado,
                        "LIBRE",
                        StringComparison.OrdinalIgnoreCase)));

            var productos = await _menuService.ObtenerMenuAsync(soloActivos: true);

            MenuActivo = new ObservableCollection<MenuItem>(
                productos.Where(p => p.Activo));

            MesaNuevaSeleccionada = MesasLibres.FirstOrDefault();
            ItemInicialSeleccionado = MenuActivo.FirstOrDefault();
            CantidadInicial = 1;
            MostrarModalNuevaCuenta = true;

            if (MesasLibres.Count == 0)
                MensajeNuevaCuenta = "No hay mesas libres.";

            if (MenuActivo.Count == 0)
                MensajeNuevaCuenta = "No hay productos activos en el menú.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA ABRIR CUENTA ERROR] {ex}");
            MensajeNuevaCuenta = $"No se pudieron cargar mesas o productos: {ex.Message}";
            MostrarModalNuevaCuenta = true;
        }
    }
    [RelayCommand]
    private void CerrarModalNuevaCuenta()
    {
        MostrarModalNuevaCuenta = false;
        MensajeNuevaCuenta = string.Empty;
    }

    [RelayCommand]
    private async Task CrearCuentaConItemAsync()
    {
        if (MesaNuevaSeleccionada == null)
        {
            MensajeNuevaCuenta = "Seleccione una mesa libre.";
            return;
        }

        if (ItemInicialSeleccionado == null)
        {
            MensajeNuevaCuenta = "Seleccione un producto.";
            return;
        }

        if (CantidadInicial <= 0)
        {
            MensajeNuevaCuenta = "La cantidad debe ser mayor que cero.";
            return;
        }

        try
        {
            var cuentaId = await _pagoService.CrearCuentaConItemAsync(
                MesaNuevaSeleccionada.IdMesa,
                ItemInicialSeleccionado.IdItem,
                CantidadInicial);

            await CargarDatosAsync();

            MensajeNuevaCuenta =
                $"Cuenta #{cuentaId} abierta y producto agregado.";

            MostrarModalNuevaCuenta = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CREAR CUENTA ERROR] {ex}");
            MensajeNuevaCuenta = $"No se pudo abrir la cuenta: {ex.Message}";
        }
    }
}
