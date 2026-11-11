using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.Services;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class CajaViewModel : ObservableObject
{
    private readonly PagoService? _pagoService;
    private readonly MesaService? _mesaService;
    private readonly MenuService? _menuService;

    // Selector de Vistas: "Cuentas", "Pagos", "Arqueo"
    [ObservableProperty]
    private string _vistaSeleccionada = "Cuentas";

    public bool EsVistaCuentas => VistaSeleccionada == "Cuentas";
    public bool EsVistaPagos => VistaSeleccionada == "Pagos";
    public bool EsVistaArqueo => VistaSeleccionada == "Arqueo";

    // ========================================================
    // SECCIÓN 1: CUENTAS DE MESA Y COMANDAS (COBRO DIRECTO)
    // ========================================================
    [ObservableProperty]
    private ObservableCollection<CuentaPendienteViewModel> _cuentasPendientes = new();

    [ObservableProperty]
    private CuentaPendienteViewModel? _cuentaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<CuentaItemDetalleDto> _detalleItemsCuenta = new();

    [ObservableProperty]
    private bool _tieneCuentaSeleccionada;

    [ObservableProperty]
    private decimal _montoCobrar;

    public string MontoCobrarTexto => $"$ {MontoCobrar:N2}";

    [ObservableProperty]
    private decimal _montoRecibido;

    [ObservableProperty]
    private decimal _vueltoCalculado;

    public string VueltoCalculadoTexto => $"$ {VueltoCalculado:N2}";

    [ObservableProperty]
    private ObservableCollection<MetodoPago> _metodosPago = new();

    [ObservableProperty]
    private MetodoPago? _metodoPagoSeleccionado;

    public bool EsPagoEfectivo => MetodoPagoSeleccionado?.FormaPago?.Contains("Efectivo", StringComparison.OrdinalIgnoreCase) == true;

    [ObservableProperty]
    private string _mensajeCaja = string.Empty;

    // ========================================================
    // GESTIÓN DE MOZO A CARGO DE LA MESA
    // ========================================================
    [ObservableProperty]
    private ObservableCollection<MozoItemDto> _mozosDisponibles = new();

    [ObservableProperty]
    private MozoItemDto? _mozoAsignadoSeleccionado;

    // ========================================================
    // SECCIÓN 2: HISTORIAL CRUD DE PAGOS
    // ========================================================
    [ObservableProperty]
    private ObservableCollection<PagoItemViewModel> _pagos = new();

    [ObservableProperty]
    private ObservableCollection<PagoItemViewModel> _pagosFiltrados = new();

    [ObservableProperty]
    private string _filtroPeriodo = "Todos";

    public bool EsFiltroHoy => FiltroPeriodo == "Hoy";
    public bool EsFiltroTodos => FiltroPeriodo == "Todos";
    public bool EsDuenoOGerente => SesionGlobal.RolActual == RolUsuario.Dueno || SesionGlobal.RolActual == RolUsuario.Gerente;

    [RelayCommand]
    private void CambiarFiltroPeriodo(string periodo)
    {
        FiltroPeriodo = periodo;
        OnPropertyChanged(nameof(EsFiltroHoy));
        OnPropertyChanged(nameof(EsFiltroTodos));
        AplicarFiltro();
    }

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private PagoItemViewModel? _pagoSeleccionado;

    // ========================================================
    // SECCIÓN 3: ARQUEO Y BALANCE DE TURNO
    // ========================================================
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

    // ========================================================
    // MODALES Y DIÁLOGOS
    // ========================================================
    [ObservableProperty]
    private bool _mostrarModalComprobante;

    [ObservableProperty]
    private PagoItemViewModel? _comprobantePago;

    [ObservableProperty]
    private ObservableCollection<CuentaItemDetalleDto> _itemsComprobante = new();

    // Modal Abrir Cuenta
    [ObservableProperty]
    private bool _mostrarModalNuevaCuenta;

    [ObservableProperty]
    private ObservableCollection<Mesa> _mesasLibres = new();

    [ObservableProperty]
    private Mesa? _mesaNuevaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<MenuItem> _menuActivo = new();

    [ObservableProperty]
    private MenuItem? _itemInicialSeleccionado;

    [ObservableProperty]
    private decimal? _cantidadInicial = 1m;

    [ObservableProperty]
    private string _mensajeNuevaCuenta = string.Empty;

    // Modal Agregar Ítem
    [ObservableProperty]
    private bool _mostrarModalAgregarItem;

    [ObservableProperty]
    private CuentaPendienteViewModel? _cuentaParaAgregarItem;

    [ObservableProperty]
    private MenuItem? _itemParaAgregar;

    [ObservableProperty]
    private decimal? _cantidadItemAgregar = 1m;

    [ObservableProperty]
    private string _mensajeAgregarItem = string.Empty;

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

    public async Task CargarDatosAsync()
    {
        await CargarMozosDisponiblesAsync();
        await CargarCuentasPendientesAsync();
        await CargarPagosAsync();
        await CargarMetodosPagoAsync();
        RecalcularTotalesTurno();
    }

    public async Task CargarMozosDisponiblesAsync()
    {
        var lista = new List<MozoItemDto>();
        try
        {
            var context = App.Services?.GetService(typeof(RestoAppDbContext)) as RestoAppDbContext
                ?? new RestoAppDbContext();

            var query = await context.Empleados
                .AsNoTracking()
                .Include(e => e.PersonaInfo)
                .Include(e => e.Rol)
                .Where(e => e.ActivoEnRol && (e.IdRol == 5 || (e.Rol != null && e.Rol.Descripcion.ToLower().Contains("mozo"))))
                .OrderBy(e => e.PersonaInfo != null ? e.PersonaInfo.Nombre : "")
                .ToListAsync();

            var empleados = query.Count > 0
                ? query
                : await context.Empleados
                    .AsNoTracking()
                    .Include(e => e.PersonaInfo)
                    .Include(e => e.Rol)
                    .Where(e => e.ActivoEnRol)
                    .OrderBy(e => e.PersonaInfo != null ? e.PersonaInfo.Nombre : "")
                    .ToListAsync();

            foreach (var emp in empleados)
            {
                string nombre = emp.PersonaInfo != null
                    ? $"{emp.PersonaInfo.Nombre} {emp.PersonaInfo.Apellido}".Trim()
                    : $"Empleado DNI {emp.DniEmpleado}";

                lista.Add(new MozoItemDto
                {
                    Dni = emp.DniEmpleado,
                    IdRol = emp.IdRol,
                    NombreCompleto = nombre,
                    RolDescripcion = emp.Rol?.Descripcion ?? "Mozo"
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CARGAR MOZOS ERROR] {ex.Message}");
        }

        MozosDisponibles = new ObservableCollection<MozoItemDto>(lista);
    }

    public async Task CargarCuentasPendientesAsync()
    {
        if (_pagoService == null) return;

        try
        {
            // Guardar ID de cuenta seleccionada para restaurar la selección tras refrescar
            int? cuentaIdPrevia = CuentaSeleccionada?.CuentaId;

            var cuentas = await _pagoService.ObtenerCuentasPendientesAsync();
            var lista = cuentas.Select(c => new CuentaPendienteViewModel
            {
                CuentaId = c.CuentaId,
                IdMesa = c.IdMesa,
                NroMesa = c.NroMesa,
                Sector = string.IsNullOrWhiteSpace(c.Sector) ? "Salón Principal" : c.Sector,
                MontoConsumo = (decimal)c.MontoConsumo,
                CantidadItems = c.CantidadItems,
                FechaApertura = c.FechaApertura,
                AtendidoPor = string.IsNullOrWhiteSpace(c.AtendidoPor) ? "Mozo de Turno" : c.AtendidoPor,
                ClienteNombre = string.IsNullOrWhiteSpace(c.ClienteNombre) ? "Cliente Particular" : c.ClienteNombre
            }).ToList();

            CuentasPendientes = new ObservableCollection<CuentaPendienteViewModel>(lista);

            // Re-seleccionar la cuenta que estaba previamente activa (ej. Mesa 6)
            CuentaPendienteViewModel? cuentaRestaurar = null;
            if (cuentaIdPrevia.HasValue)
            {
                cuentaRestaurar = CuentasPendientes.FirstOrDefault(c => c.CuentaId == cuentaIdPrevia.Value);
            }

            if (cuentaRestaurar == null && CuentasPendientes.Count > 0)
            {
                cuentaRestaurar = CuentasPendientes.First();
            }

            if (cuentaRestaurar != null)
            {
                await SeleccionarCuentaAsync(cuentaRestaurar);
            }
            else
            {
                CuentaSeleccionada = null;
                await CargarDetalleCuentaSeleccionadaAsync(null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CARGAR CUENTAS ERROR] {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task CargarPagosAsync()
    {
        if (_pagoService == null) return;

        var listaPagos = new List<PagoItemViewModel>();
        bool cargoExitoso = false;

        try
        {
            var entidades = await _pagoService.ObtenerPagosAsync();

            Dictionary<int, CuentaPagoInfoDto> infoCuentas = new();
            try
            {
                infoCuentas = await _pagoService.ObtenerInfoCuentasPagosAsync();
            }
            catch (Exception exInfo)
            {
                Console.WriteLine($"[CAJA INFO CUENTAS ERROR] {exInfo.Message}");
            }

            foreach (var p in entidades)
            {
                string desc;
                string cliente;
                DateTime fechaMostrar = p.FechaPago;

                if (p.IdCuenta.HasValue && infoCuentas.TryGetValue(p.IdCuenta.Value, out var ctaInfo))
                {
                    desc = $"Mesa #{ctaInfo.NroMesa} (Cuenta #{p.IdCuenta.Value})";
                    cliente = !string.IsNullOrWhiteSpace(ctaInfo.AtendidoPor)
                        ? $"Mozo: {ctaInfo.AtendidoPor}"
                        : "Cliente Salón";

                    if (ctaInfo.FechaCierre.HasValue)
                    {
                        fechaMostrar = ctaInfo.FechaCierre.Value;
                    }
                }
                else if (p.IdCuenta.HasValue)
                {
                    desc = $"Cuenta #{p.IdCuenta.Value}";
                    cliente = "Cliente Salón";
                }
                else if (p.Reserva != null)
                {
                    var mesas = p.Reserva.Mesas?.Any() == true
                        ? string.Join(", ", p.Reserva.Mesas.Select(m => $"Mesa {m.NroMesa}"))
                        : $"Reserva #{p.IdReserva}";
                    desc = $"Cobro {mesas}";

                    cliente = p.Reserva.Cliente?.PersonaInfo != null
                        ? $"{p.Reserva.Cliente.PersonaInfo.Nombre} {p.Reserva.Cliente.PersonaInfo.Apellido}".Trim()
                        : "Cliente Particular";

                    fechaMostrar = p.Reserva.FechaReserva;
                }
                else
                {
                    desc = $"Cobro de Salón #{p.IdPago}";
                    cliente = "Cliente Particular";
                }

                listaPagos.Add(new PagoItemViewModel
                {
                    IdPago = p.IdPago,
                    FechaPago = fechaMostrar,
                    Descripcion = desc,
                    ClienteNombre = cliente,
                    MedioPago = p.MetodoPago?.FormaPago ?? "Efectivo",
                    Monto = (decimal)p.Monto,
                    IdCuenta = p.IdCuenta
                });
            }

            cargoExitoso = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA DB ERROR] {ex.Message}");
        }

        // Si la carga fue exitosa, actualizamos siempre
        if (cargoExitoso)
        {
            Pagos = new ObservableCollection<PagoItemViewModel>(listaPagos);
            AplicarFiltro();
            RecalcularTotalesTurno();
        }
        else if (Pagos.Count == 0)
        {
            // Si no teníamos nada y falló, reintentamos una vez con contexto aislado
            try
            {
                await Task.Delay(250);
                using var freshCtx = new RestoAppDbContext();
                var fallbackRepo = new PagoRepository(freshCtx);
                var fallbackService = new PagoService(fallbackRepo);
                var fallbackEntidades = await fallbackService.ObtenerPagosAsync();
                var fallbackInfo = await fallbackService.ObtenerInfoCuentasPagosAsync();

                foreach (var p in fallbackEntidades)
                {
                    string desc = p.IdCuenta.HasValue && fallbackInfo.TryGetValue(p.IdCuenta.Value, out var ctaInfo)
                        ? $"Mesa #{ctaInfo.NroMesa} (Cuenta #{p.IdCuenta.Value})"
                        : (p.IdCuenta.HasValue ? $"Cuenta #{p.IdCuenta.Value}" : $"Cobro #{p.IdPago}");

                    string cliente = p.IdCuenta.HasValue && fallbackInfo.TryGetValue(p.IdCuenta.Value, out var ctaInfo2) && !string.IsNullOrWhiteSpace(ctaInfo2.AtendidoPor)
                        ? $"Mozo: {ctaInfo2.AtendidoPor}"
                        : "Cliente Salón";

                    listaPagos.Add(new PagoItemViewModel
                    {
                        IdPago = p.IdPago,
                        FechaPago = p.FechaPago,
                        Descripcion = desc,
                        ClienteNombre = cliente,
                        MedioPago = p.MetodoPago?.FormaPago ?? "Efectivo",
                        Monto = (decimal)p.Monto,
                        IdCuenta = p.IdCuenta
                    });
                }

                Pagos = new ObservableCollection<PagoItemViewModel>(listaPagos);
                AplicarFiltro();
                RecalcularTotalesTurno();
            }
            catch (Exception exFallback)
            {
                Console.WriteLine($"[CAJA DB FALLBACK ERROR] {exFallback.Message}");
            }
        }
    }

    public async Task CargarMetodosPagoAsync()
    {
        if (_pagoService == null) return;
        try
        {
            var metodos = await _pagoService.ObtenerMetodosPagoAsync();
            MetodosPago = new ObservableCollection<MetodoPago>(metodos);
            MetodoPagoSeleccionado = MetodosPago.FirstOrDefault(
                m => m.FormaPago.Contains("Efectivo", StringComparison.OrdinalIgnoreCase))
                ?? MetodosPago.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA METODOS PAGO ERROR] {ex.Message}");
        }
    }

    async partial void OnCuentaSeleccionadaChanged(CuentaPendienteViewModel? value)
    {
        await CargarDetalleCuentaSeleccionadaAsync(value);
    }

    public async Task CargarDetalleCuentaSeleccionadaAsync(CuentaPendienteViewModel? cuenta)
    {
        if (cuenta == null || _pagoService == null)
        {
            TieneCuentaSeleccionada = false;
            DetalleItemsCuenta.Clear();
            MontoCobrar = 0m;
            MontoRecibido = 0m;
            VueltoCalculado = 0m;
            return;
        }

        TieneCuentaSeleccionada = true;
        MontoCobrar = cuenta.MontoConsumo;
        MontoRecibido = MontoCobrar;
        CalcularCambio();

        try
        {
            var items = await _pagoService.ObtenerDetalleCuentaAsync(cuenta.CuentaId);
            DetalleItemsCuenta = new ObservableCollection<CuentaItemDetalleDto>(items);

            // Sincronizar el selector de mozo con el mozo asignado a esta cuenta
            if (MozosDisponibles.Count > 0)
            {
                MozoAsignadoSeleccionado = MozosDisponibles.FirstOrDefault(m =>
                    string.Equals(m.NombreCompleto, cuenta.AtendidoPor, StringComparison.OrdinalIgnoreCase))
                    ?? MozosDisponibles.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CARGAR DETALLE ITEMS ERROR] {ex.Message}");
            DetalleItemsCuenta = new ObservableCollection<CuentaItemDetalleDto>();
        }
    }

    [RelayCommand]
    public async Task SeleccionarCuentaAsync(CuentaPendienteViewModel? cuenta)
    {
        if (CuentaSeleccionada != cuenta)
        {
            CuentaSeleccionada = cuenta;
        }
        else
        {
            await CargarDetalleCuentaSeleccionadaAsync(cuenta);
        }
    }

    [RelayCommand]
    public async Task CambiarMozoCuentaAsync()
    {
        if (CuentaSeleccionada == null || MozoAsignadoSeleccionado == null || _pagoService == null)
            return;

        try
        {
            await _pagoService.CambiarMozoCuentaAsync(
                CuentaSeleccionada.CuentaId,
                MozoAsignadoSeleccionado.Dni,
                MozoAsignadoSeleccionado.IdRol);

            CuentaSeleccionada.AtendidoPor = MozoAsignadoSeleccionado.NombreCompleto;
            MensajeCaja = $"✓ Mozo actualizado: {MozoAsignadoSeleccionado.NombreCompleto} asignado a Mesa #{CuentaSeleccionada.NroMesa}.";

            // Actualizar columna en la lista de cuentas
            var itemLista = CuentasPendientes.FirstOrDefault(c => c.CuentaId == CuentaSeleccionada.CuentaId);
            if (itemLista != null)
            {
                itemLista.AtendidoPor = MozoAsignadoSeleccionado.NombreCompleto;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAMBIAR MOZO ERROR] {ex.Message}");
            MensajeCaja = $"Error al cambiar mozo: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CancelarMesaVaciaAsync()
    {
        MensajeCaja = string.Empty;

        if (!EsDuenoOGerente)
        {
            MensajeCaja = "Acción restringida: solo el Dueño o Gerente pueden cerrar una mesa sin cobro.";
            return;
        }

        if (CuentaSeleccionada == null || _pagoService == null)
        {
            MensajeCaja = "Por favor seleccione una mesa abierta para cerrar.";
            return;
        }

        if (DetalleItemsCuenta.Count > 0)
        {
            MensajeCaja = "Esta mesa contiene consumos cargados. Para cerrarla debe liquidar el cobro correspondiente o retirar los productos primero.";
            return;
        }

        try
        {
            var mesaNro = CuentaSeleccionada.NroMesa;
            var cuentaId = CuentaSeleccionada.CuentaId;

            await _pagoService.CancelarCuentaAsync(cuentaId);
            await CargarCuentasPendientesAsync();

            MensajeCaja = $"Se cerró la comanda vacía de Mesa #{mesaNro}. La mesa ha vuelto al estado LIBRE.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CANCELAR MESA ERROR] {ex.Message}");
            MensajeCaja = $"Error al cerrar mesa vacía: {ex.Message}";
        }
    }

    partial void OnMetodoPagoSeleccionadoChanged(MetodoPago? value)
    {
        OnPropertyChanged(nameof(EsPagoEfectivo));
        CalcularCambio();
    }

    partial void OnMontoRecibidoChanged(decimal value)
    {
        CalcularCambio();
    }

    partial void OnMontoCobrarChanged(decimal value)
    {
        OnPropertyChanged(nameof(MontoCobrarTexto));
        CalcularCambio();
    }

    private void CalcularCambio()
    {
        if (EsPagoEfectivo && MontoRecibido >= MontoCobrar)
        {
            VueltoCalculado = MontoRecibido - MontoCobrar;
        }
        else
        {
            VueltoCalculado = 0m;
        }
        OnPropertyChanged(nameof(VueltoCalculadoTexto));
    }

    [RelayCommand]
    private async Task QuitarItemDeCuentaAsync(CuentaItemDetalleDto? item)
    {
        if (item == null || CuentaSeleccionada == null || _pagoService == null) return;

        try
        {
            await _pagoService.QuitarItemDeCuentaAsync(CuentaSeleccionada.CuentaId, item.IdItem, 1);
            MensajeCaja = $"Se quitó 1 unidad de {item.DescripcionItem}.";

            // Refrescar cuentas y detalle
            await CargarCuentasPendientesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA QUITAR ITEM ERROR] {ex.Message}");
            MensajeCaja = $"Error al quitar ítem: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ProcesarPagoYLiquidarAsync()
    {
        MensajeCaja = string.Empty;

        if (CuentaSeleccionada == null)
        {
            MensajeCaja = "Por favor seleccione una mesa con cuenta abierta.";
            return;
        }

        if (MetodoPagoSeleccionado == null)
        {
            MensajeCaja = "Por favor seleccione un medio de pago.";
            return;
        }

        if (_pagoService == null)
        {
            MensajeCaja = "El servicio de caja no está disponible.";
            return;
        }

        try
        {
            var cuentaId = CuentaSeleccionada.CuentaId;
            var mesaNro = CuentaSeleccionada.NroMesa;
            var itemsCopia = DetalleItemsCuenta.ToList();

            var nuevoPagoId = await _pagoService.CerrarCuentaAsync(
                cuentaId,
                MetodoPagoSeleccionado.IdMetodo);

            await CargarDatosAsync();

            // Preparar comprobante
            var comprobante = Pagos.FirstOrDefault(p => p.IdPago == nuevoPagoId)
                ?? new PagoItemViewModel
                {
                    IdPago = nuevoPagoId,
                    FechaPago = DateTime.Now,
                    Descripcion = $"Cuenta Mesa #{mesaNro}",
                    ClienteNombre = "Cliente Salón",
                    MedioPago = MetodoPagoSeleccionado.FormaPago,
                    Monto = MontoCobrar,
                    IdCuenta = cuentaId
                };

            ComprobantePago = comprobante;
            ItemsComprobante = new ObservableCollection<CuentaItemDetalleDto>(itemsCopia);
            MostrarModalComprobante = true;
            MensajeCaja = $"¡Cobro de Mesa #{mesaNro} completado exitosamente! Pago registrado #{nuevoPagoId}.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CERRAR CUENTA ERROR] {ex.Message}");
            MensajeCaja = $"No se pudo liquidar la cuenta: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VerComprobanteAsync(PagoItemViewModel? item)
    {
        if (item == null) return;
        ComprobantePago = item;

        if (item.IdCuenta.HasValue && _pagoService != null)
        {
            try
            {
                var items = await _pagoService.ObtenerDetalleCuentaAsync(item.IdCuenta.Value);
                ItemsComprobante = new ObservableCollection<CuentaItemDetalleDto>(items);
            }
            catch
            {
                ItemsComprobante = new ObservableCollection<CuentaItemDetalleDto>();
            }
        }
        else
        {
            ItemsComprobante = new ObservableCollection<CuentaItemDetalleDto>();
        }

        MostrarModalComprobante = true;
    }

    [RelayCommand]
    private void CerrarModalComprobante()
    {
        MostrarModalComprobante = false;
        ComprobantePago = null;
        ItemsComprobante.Clear();
    }

    [RelayCommand]
    private async Task EliminarPagoAsync(PagoItemViewModel? item)
    {
        if (item == null || _pagoService == null) return;

        try
        {
            await _pagoService.EliminarPagoAsync(item.IdPago);
            await CargarDatosAsync();
            MensajeCaja = $"Pago #{item.IdPago} eliminado del historial.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA ELIMINAR PAGO ERROR] {ex.Message}");
            _ = AlertaService.MostrarAlertaConexionAsync($"No se pudo anular el pago: {ex.Message}");
        }
    }

    // ========================================================
    // MODALES Y COMPLEMENTOS
    // ========================================================
    [RelayCommand]
    private async Task AbrirModalNuevaCuentaAsync()
    {
        MensajeNuevaCuenta = string.Empty;
        if (_mesaService == null || _menuService == null) return;

        try
        {
            var mesas = await _mesaService.ObtenerMesasAsync();
            MesasLibres = new ObservableCollection<Mesa>(
                mesas.Where(m => m.EsActivo && string.Equals(m.Estado, "LIBRE", StringComparison.OrdinalIgnoreCase)));

            var productos = await _menuService.ObtenerMenuAsync(soloActivos: true);
            MenuActivo = new ObservableCollection<MenuItem>(productos.Where(p => p.Activo));

            MesaNuevaSeleccionada = MesasLibres.FirstOrDefault();
            ItemInicialSeleccionado = MenuActivo.FirstOrDefault();
            CantidadInicial = 1;
            MostrarModalNuevaCuenta = true;

            if (MesasLibres.Count == 0)
                MensajeNuevaCuenta = "No hay mesas en estado LIBRE actualmente.";

            if (MenuActivo.Count == 0)
                MensajeNuevaCuenta = "No se encontraron productos activos en el menú.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA ABRIR CUENTA ERROR] {ex.Message}");
            MensajeNuevaCuenta = $"Error al cargar datos: {ex.Message}";
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
            MensajeNuevaCuenta = "Seleccione un producto inicial.";
            return;
        }

        if (!CantidadInicial.HasValue || CantidadInicial.Value <= 0)
        {
            MensajeNuevaCuenta = "La cantidad debe ser al menos 1.";
            return;
        }

        if (_pagoService == null) return;

        try
        {
            var nuevaCuentaId = await _pagoService.CrearCuentaConItemAsync(
                MesaNuevaSeleccionada.IdMesa,
                ItemInicialSeleccionado.IdItem,
                (int)CantidadInicial.Value);

            await CargarCuentasPendientesAsync();
            MostrarModalNuevaCuenta = false;
            MensajeCaja = $"Se abrió comanda en Mesa #{MesaNuevaSeleccionada.NroMesa} con {CantidadInicial}x {ItemInicialSeleccionado.Descripcion}.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA CREAR CUENTA ERROR] {ex.Message}");
            MensajeNuevaCuenta = $"No se pudo abrir la cuenta: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AbrirModalAgregarItemAsync()
    {
        MensajeAgregarItem = string.Empty;
        if (_menuService == null || CuentaSeleccionada == null)
        {
            MensajeCaja = "Seleccione una cuenta abierta para agregar productos.";
            return;
        }

        try
        {
            var productos = await _menuService.ObtenerMenuAsync(soloActivos: true);
            MenuActivo = new ObservableCollection<MenuItem>(productos.Where(p => p.Activo));

            CuentaParaAgregarItem = CuentaSeleccionada;
            ItemParaAgregar = MenuActivo.FirstOrDefault();
            CantidadItemAgregar = 1m;
            MostrarModalAgregarItem = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA AGREGAR ITEM MODAL ERROR] {ex.Message}");
            MensajeAgregarItem = $"Error: {ex.Message}";
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
        MensajeAgregarItem = string.Empty;

        if (CuentaParaAgregarItem == null)
        {
            MensajeAgregarItem = "⚠️ No hay una cuenta seleccionada.";
            return;
        }

        if (ItemParaAgregar == null)
        {
            MensajeAgregarItem = "⚠️ Por favor, seleccione un producto del menú.";
            return;
        }

        if (!CantidadItemAgregar.HasValue)
        {
            MensajeAgregarItem = "⚠️ Debe ingresar una cantidad numérica válida (mínimo 1).";
            return;
        }

        if (CantidadItemAgregar.Value <= 0)
        {
            MensajeAgregarItem = "⚠️ La cantidad debe ser mayor que cero.";
            return;
        }

        if (CantidadItemAgregar.Value > 99)
        {
            MensajeAgregarItem = "⚠️ La cantidad no puede exceder 99 unidades por pedido.";
            return;
        }

        if (_pagoService == null) return;

        int cantInt = (int)Math.Floor(CantidadItemAgregar.Value);

        try
        {
            await _pagoService.AgregarItemACuentaAsync(
                CuentaParaAgregarItem.CuentaId,
                ItemParaAgregar.IdItem,
                cantInt);

            await CargarCuentasPendientesAsync();
            MostrarModalAgregarItem = false;
            MensajeCaja = $"Se agregaron {cantInt}x {ItemParaAgregar.Descripcion} a la Mesa #{CuentaParaAgregarItem.NroMesa}.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CAJA AGREGAR ITEM ERROR] {ex.Message}");
            MensajeAgregarItem = $"Error al agregar producto: {ex.Message}";
        }
    }

    // ========================================================
    // SECCIÓN ARQUEO
    // ========================================================
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
        OnPropertyChanged(nameof(EsVistaCuentas));
        OnPropertyChanged(nameof(EsVistaPagos));
        OnPropertyChanged(nameof(EsVistaArqueo));

        if (value == "Pagos" || value == "Arqueo")
        {
            _ = CargarPagosAsync();
        }
        else if (value == "Cuentas")
        {
            _ = CargarCuentasPendientesAsync();
        }
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnEfectivoRecontadoChanged(decimal value)
    {
        DiferenciaArqueo = value - EfectivoEsperado;
    }

    private void AplicarFiltro()
    {
        var items = Pagos.AsEnumerable();

        if (FiltroPeriodo == "Hoy")
        {
            var hoy = DateTime.Today;
            items = items.Where(p => p.FechaPago.Date == hoy);
        }

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            var query = TextoBusqueda.Trim();
            items = items.Where(p =>
                p.Descripcion.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.ClienteNombre.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.MedioPago.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.IdPago.ToString().Contains(query));
        }

        PagosFiltrados = new ObservableCollection<PagoItemViewModel>(items);
    }

    [RelayCommand]
    private void CambiarVista(string vista)
    {
        VistaSeleccionada = vista;
    }

    [RelayCommand]
    private void CalcularArqueo()
    {
        DiferenciaArqueo = EfectivoRecontado - EfectivoEsperado;
        MensajeCierre = DiferenciaArqueo == 0
            ? "Arqueo de caja perfecto. Sin diferencias detectadas."
            : $"Diferencia de caja detectada: $ {DiferenciaArqueo:N2}";
    }

    [RelayCommand]
    private void FinalizarYCerrarCaja()
    {
        CalcularArqueo();
        MensajeCierre = "🔒 Caja del turno cerrada correctamente. Arqueo completado.";
    }
}
