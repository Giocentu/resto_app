using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class AbrirMesaViewModel : ObservableObject
{
    private readonly MesaService _mesaService;
    private readonly PagoService _pagoService;
    private readonly UbicacionService? _ubicacionService;
    private readonly DispatcherTimer _clockTimer;

    // Fecha y hora en vivo
    [ObservableProperty]
    private string _fechaActualTexto = string.Empty;

    [ObservableProperty]
    private string _horaActualTexto = string.Empty;

    // Mesas y Sectores
    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesas = new();

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesasFiltradas = new();

    [ObservableProperty]
    private ObservableCollection<string> _ubicaciones = new() { "Todas" };

    [ObservableProperty]
    private string _ubicacionSeleccionada = "Todas";

    [ObservableProperty]
    private VisualMesaItemViewModel? _mesaSeleccionada;

    // Contadores
    [ObservableProperty]
    private int _libresCount;

    [ObservableProperty]
    private int _ocupadasCount;

    [ObservableProperty]
    private int _reservadasCount;

    // Mozos
    [ObservableProperty]
    private ObservableCollection<MozoItemDto> _mozos = new();

    [ObservableProperty]
    private MozoItemDto? _mozoSeleccionado;

    // Mensaje de Validación / Error
    [ObservableProperty]
    private string _mensajeError = string.Empty;

    public AbrirMesaViewModel(
        MesaService? mesaService = null,
        PagoService? pagoService = null,
        UbicacionService? ubicacionService = null)
    {
        _mesaService = mesaService 
            ?? App.Services?.GetService(typeof(MesaService)) as MesaService
            ?? new MesaService(new MesaRepository(new RestoAppDbContext()));

        _pagoService = pagoService 
            ?? App.Services?.GetService(typeof(PagoService)) as PagoService
            ?? new PagoService(new PagoRepository(new RestoAppDbContext()));

        _ubicacionService = ubicacionService
            ?? App.Services?.GetService(typeof(UbicacionService)) as UbicacionService
            ?? new UbicacionService(new UbicacionRepository(new RestoAppDbContext()));

        ActualizarReloj();
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (s, e) => ActualizarReloj();
        _clockTimer.Start();

        _ = CargarDatosAsync();
    }

    private void ActualizarReloj()
    {
        var now = DateTime.Now;
        var culture = CultureInfo.GetCultureInfo("es-AR");
        var fecha = now.ToString("dddd, dd 'de' MMMM 'de' yyyy", culture);
        // Capitalizar primera letra
        if (!string.IsNullOrEmpty(fecha))
        {
            FechaActualTexto = char.ToUpper(fecha[0]) + fecha.Substring(1);
        }
        HoraActualTexto = now.ToString("HH:mm:ss");
    }

    public async Task CargarDatosAsync()
    {
        await CargarMesasAsync();
        await CargarMozosAsync();
    }

    public async Task CargarMesasAsync()
    {
        var lista = new List<VisualMesaItemViewModel>();
        try
        {
            var entidades = await _mesaService.ObtenerMesasAsync(soloActivas: true);
            foreach (var m in entidades)
            {
                lista.Add(new VisualMesaItemViewModel
                {
                    IdMesa = m.IdMesa,
                    NroMesa = m.NroMesa,
                    Capacidad = m.Capacidad,
                    IdUbicacion = m.IdUbicacion,
                    UbicacionDescripcion = m.Ubicacion?.Ubicacion ?? "Salón Principal",
                    Estado = string.IsNullOrWhiteSpace(m.Estado) ? "LIBRE" : m.Estado.ToUpper()
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ABRIR MESA CARGAR MESAS ERROR] {ex.Message}");
        }

        Mesas = new ObservableCollection<VisualMesaItemViewModel>(lista);

        // Contadores
        LibresCount = Mesas.Count(m => m.Estado == "LIBRE");
        OcupadasCount = Mesas.Count(m => m.Estado == "OCUPADA");
        ReservadasCount = Mesas.Count(m => m.Estado == "RESERVADA");

        // Sectores
        var listaUbicaciones = new List<string> { "Todas" };
        if (_ubicacionService != null)
        {
            try
            {
                var ubs = await _ubicacionService.ObtenerUbicacionesAsync();
                listaUbicaciones.AddRange(ubs.Select(u => u.Ubicacion));
            }
            catch { }
        }

        Ubicaciones = new ObservableCollection<string>(listaUbicaciones.Distinct());
        AplicarFiltroUbicacion();
    }

    public async Task CargarMozosAsync()
    {
        var listaMozos = new List<MozoItemDto>();
        try
        {
            var context = App.Services?.GetService(typeof(RestoAppDbContext)) as RestoAppDbContext
                ?? new RestoAppDbContext();

            // Filtrar exclusivamente empleados con rol Mozo (id_rol = 5 o descripción Mozo)
            var mozosQuery = await context.Empleados
                .AsNoTracking()
                .Include(e => e.PersonaInfo)
                .Include(e => e.Rol)
                .Where(e => e.ActivoEnRol && (e.IdRol == 5 || (e.Rol != null && e.Rol.Descripcion.ToLower().Contains("mozo"))))
                .OrderBy(e => e.PersonaInfo != null ? e.PersonaInfo.Nombre : "")
                .ToListAsync();

            var empleados = mozosQuery.Count > 0
                ? mozosQuery
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

                string rol = emp.Rol?.Descripcion ?? "Mozo";

                listaMozos.Add(new MozoItemDto
                {
                    Dni = emp.DniEmpleado,
                    IdRol = emp.IdRol,
                    NombreCompleto = nombre,
                    RolDescripcion = rol
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ABRIR MESA CARGAR MOZOS ERROR] {ex.Message}");
        }

        Mozos = new ObservableCollection<MozoItemDto>(listaMozos);
        MozoSeleccionado = Mozos.FirstOrDefault();
    }

    [RelayCommand]
    public void SeleccionarMesa(VisualMesaItemViewModel? mesa)
    {
        if (mesa == null) return;

        MensajeError = string.Empty;

        // Desmarcar todas las anteriores
        foreach (var m in Mesas)
        {
            m.IsSelected = false;
        }

        mesa.IsSelected = true;
        MesaSeleccionada = mesa;

        if (mesa.Estado != "LIBRE")
        {
            MensajeError = $"Atención: La Mesa #{mesa.NroMesa} se encuentra en estado '{mesa.Estado}'. Solo se pueden abrir mesas libres.";
        }
    }

    [RelayCommand]
    public void FiltrarPorUbicacion(string ubicacion)
    {
        UbicacionSeleccionada = ubicacion;
        AplicarFiltroUbicacion();
    }

    private void AplicarFiltroUbicacion()
    {
        if (string.IsNullOrWhiteSpace(UbicacionSeleccionada) || UbicacionSeleccionada == "Todas")
        {
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(Mesas);
        }
        else
        {
            var filtradas = Mesas.Where(m =>
                string.Equals(m.UbicacionDescripcion, UbicacionSeleccionada, StringComparison.OrdinalIgnoreCase));
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(filtradas);
        }
    }

    public async Task<int?> ConfirmarAperturaAsync()
    {
        MensajeError = string.Empty;

        if (MesaSeleccionada == null)
        {
            MensajeError = "⚠️ Por favor, seleccione una mesa haciendo clic sobre su tarjeta.";
            return null;
        }

        if (MesaSeleccionada.Estado != "LIBRE")
        {
            MensajeError = $"⚠️ La Mesa #{MesaSeleccionada.NroMesa} no está disponible (Estado actual: {MesaSeleccionada.Estado}). Solo puede abrir mesas en estado LIBRE.";
            return null;
        }

        if (MozoSeleccionado == null)
        {
            MensajeError = "⚠️ Por favor, seleccione el mozo a cargo de la mesa.";
            return null;
        }

        try
        {
            int nuevaCuentaId = await _pagoService.CrearCuentaAsync(
                MesaSeleccionada.IdMesa,
                MozoSeleccionado.Dni,
                MozoSeleccionado.IdRol);

            _clockTimer.Stop();
            return nuevaCuentaId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ABRIR MESA ERROR] {ex.Message}");
            MensajeError = $"Error al abrir la mesa: {ex.Message}";
            return null;
        }
    }

    public void DetenerReloj()
    {
        _clockTimer.Stop();
    }
}

