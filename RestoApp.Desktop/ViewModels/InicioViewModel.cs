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

public partial class InicioViewModel : ObservableObject
{
    private readonly MesaService? _mesaService;
    private readonly UbicacionService? _ubicacionService;
    private readonly PagoService? _pagoService;
    private readonly Action? _navigateAMesasAction;

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesas = new();

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesasFiltradas = new();

    [ObservableProperty]
    private ObservableCollection<string> _ubicaciones = new() { "Todas" };

    [ObservableProperty]
    private ObservableCollection<string> _sectores = new() { "Todas" };

    [ObservableProperty]
    private VisualMesaItemViewModel? _selectedMesa;

    public bool PuedeLiberarMesa =>
        SelectedMesa != null &&
        (SelectedMesa.Estado.Equals("RESERVADA", StringComparison.OrdinalIgnoreCase) ||
         SelectedMesa.Estado.Equals("EN LIMPIEZA", StringComparison.OrdinalIgnoreCase));

    public bool PuedeReservarMesa =>
        SelectedMesa != null &&
        SelectedMesa.Estado.Equals("LIBRE", StringComparison.OrdinalIgnoreCase);

    public bool EsMesaOcupada =>
        SelectedMesa != null &&
        SelectedMesa.Estado.Equals("OCUPADA", StringComparison.OrdinalIgnoreCase);

    partial void OnSelectedMesaChanged(VisualMesaItemViewModel? value)
    {
        OnPropertyChanged(nameof(PuedeLiberarMesa));
        OnPropertyChanged(nameof(PuedeReservarMesa));
        OnPropertyChanged(nameof(EsMesaOcupada));
    }

    [ObservableProperty]
    private string _ubicacionSeleccionada = "Todas";

    [ObservableProperty]
    private string _sectorSeleccionado = "Todas";

    [ObservableProperty]
    private int _libresCount;

    [ObservableProperty]
    private int _reservadasCount;

    [ObservableProperty]
    private int _ocupadasCount;

    [ObservableProperty]
    private int _limpiezaCount;

    public InicioViewModel(
        MesaService? mesaService = null,
        Action? navigateAMesasAction = null,
        UbicacionService? ubicacionService = null,
        PagoService? pagoService = null)
    {
        _mesaService = mesaService;
        _navigateAMesasAction = navigateAMesasAction;
        _ubicacionService = ubicacionService ?? App.Services?.GetService(typeof(UbicacionService)) as UbicacionService;
        _pagoService = pagoService ?? App.Services?.GetService(typeof(PagoService)) as PagoService;
        _ = CargarMesasAsync();
    }

    public async Task CargarMesasAsync()
    {
        var lista = new List<VisualMesaItemViewModel>();

        if (_mesaService != null)
        {
            try
            {
                var entidades = await _mesaService.ObtenerMesasAsync(soloActivas: true);

                Dictionary<int, CuentaPendienteDto> cuentasPorMesa = new();
                if (_pagoService != null)
                {
                    try
                    {
                        var cuentas = await _pagoService.ObtenerCuentasPendientesAsync();
                        foreach (var c in cuentas)
                        {
                            cuentasPorMesa[c.IdMesa] = c;
                        }
                    }
                    catch (Exception exCuentas)
                    {
                        Console.WriteLine($"[INICIO CUENTAS ERROR] {exCuentas.Message}");
                    }
                }

                foreach (var m in entidades)
                {
                    cuentasPorMesa.TryGetValue(m.IdMesa, out var cta);

                    lista.Add(new VisualMesaItemViewModel
                    {
                        IdMesa = m.IdMesa,
                        NroMesa = m.NroMesa,
                        Capacidad = m.Capacidad,
                        IdUbicacion = m.IdUbicacion,
                        UbicacionDescripcion = m.Ubicacion?.Ubicacion ?? "Salón Principal",
                        Estado = string.IsNullOrWhiteSpace(m.Estado) ? "LIBRE" : m.Estado,
                        CuentaId = cta?.CuentaId,
                        ConsumoActual = cta != null ? (decimal)cta.MontoConsumo : 0m,
                        MozoNombre = cta?.AtendidoPor ?? string.Empty,
                        ClienteNombre = cta?.ClienteNombre ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INICIO DB ERROR] Error al cargar mesas: {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Mesas = new ObservableCollection<VisualMesaItemViewModel>(lista);

        var listaUbicaciones = new List<string> { "Todas" };
        if (_ubicacionService != null)
        {
            try
            {
                var uList = await _ubicacionService.ObtenerUbicacionesAsync(soloActivas: true);
                foreach (var u in uList)
                {
                    if (!string.IsNullOrWhiteSpace(u.Ubicacion) && !listaUbicaciones.Contains(u.Ubicacion))
                    {
                        listaUbicaciones.Add(u.Ubicacion);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INICIO UBICACIONES DB ERROR] {ex.Message}");
            }
        }
        foreach (var m in Mesas)
        {
            if (!string.IsNullOrWhiteSpace(m.UbicacionDescripcion) && !listaUbicaciones.Contains(m.UbicacionDescripcion))
            {
                listaUbicaciones.Add(m.UbicacionDescripcion);
            }
        }

        Ubicaciones = new ObservableCollection<string>(listaUbicaciones.Distinct());
        Sectores = Ubicaciones;

        ActualizarConteos();
        AplicarFiltroUbicacion();

        if (MesasFiltradas.Any())
        {
            SeleccionarMesa(MesasFiltradas.First());
        }
        else
        {
            SelectedMesa = null;
        }
    }

    private void ActualizarConteos()
    {
        LibresCount = Mesas.Count(m => string.Equals(m.Estado, "LIBRE", StringComparison.OrdinalIgnoreCase));
        ReservadasCount = Mesas.Count(m => string.Equals(m.Estado, "RESERVADA", StringComparison.OrdinalIgnoreCase));
        OcupadasCount = Mesas.Count(m => string.Equals(m.Estado, "OCUPADA", StringComparison.OrdinalIgnoreCase));
        LimpiezaCount = Mesas.Count(m => string.Equals(m.Estado, "EN LIMPIEZA", StringComparison.OrdinalIgnoreCase));
    }

    [RelayCommand]
    private void FiltrarPorUbicacion(string ubicacion)
    {
        UbicacionSeleccionada = ubicacion;
        SectorSeleccionado = ubicacion;
        AplicarFiltroUbicacion();
    }

    [RelayCommand]
    private void FiltrarPorSector(string sector) => FiltrarPorUbicacion(sector);

    private void AplicarFiltroUbicacion()
    {
        if (string.IsNullOrEmpty(UbicacionSeleccionada) || UbicacionSeleccionada == "Todas" || UbicacionSeleccionada == "Todos")
        {
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(Mesas);
        }
        else
        {
            var filtradas = Mesas.Where(m => string.Equals(m.UbicacionDescripcion, UbicacionSeleccionada, StringComparison.OrdinalIgnoreCase));
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(filtradas);
        }
    }

    [RelayCommand]
    private void SeleccionarMesa(VisualMesaItemViewModel? mesa)
    {
        if (mesa == null) return;
        foreach (var m in Mesas)
        {
            m.IsSelected = false;
        }
        mesa.IsSelected = true;
        SelectedMesa = mesa;
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync(string nuevoEstado)
    {
        if (SelectedMesa != null)
        {
            var estadoAnterior = SelectedMesa.Estado;
            SelectedMesa.Estado = nuevoEstado;

            if (_mesaService != null && SelectedMesa.IdMesa > 0)
            {
                try
                {
                    int idUbicacion = SelectedMesa.IdUbicacion > 0 ? SelectedMesa.IdUbicacion : 1;
                    await _mesaService.EditarMesaAsync(
                        SelectedMesa.IdMesa,
                        SelectedMesa.NroMesa,
                        SelectedMesa.Capacidad,
                        idUbicacion,
                        nuevoEstado
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CAMBIAR ESTADO DB ERROR] {ex.Message}");
                    SelectedMesa.Estado = estadoAnterior;
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }

            ActualizarConteos();
            OnPropertyChanged(nameof(SelectedMesa));
            OnPropertyChanged(nameof(PuedeLiberarMesa));
            OnPropertyChanged(nameof(PuedeReservarMesa));
        }
    }

    [RelayCommand]
    private void IrAMesas()
    {
        _navigateAMesasAction?.Invoke();
    }

    public async Task<CuentaPendienteViewModel?> ObtenerOCrearCuentaParaMesaSeleccionadaAsync()
    {
        if (SelectedMesa == null) return null;
        if (_pagoService == null) return null;

        try
        {
            int cuentaId = 0;
            if (SelectedMesa.CuentaId.HasValue && SelectedMesa.CuentaId.Value > 0)
            {
                cuentaId = SelectedMesa.CuentaId.Value;
            }
            else
            {
                // Buscar si existe una cuenta abierta para esta mesa
                var cuentas = await _pagoService.ObtenerCuentasPendientesAsync();
                var ctaExistente = cuentas.FirstOrDefault(c => c.IdMesa == SelectedMesa.IdMesa);
                if (ctaExistente != null)
                {
                    cuentaId = ctaExistente.CuentaId;
                }
                else
                {
                    // Si la mesa está ocupada pero aún no tenía comanda registrada, abrirla
                    long dni = SesionGlobal.DniActual ?? 22222222;
                    int idRol = SesionGlobal.RolActual switch
                    {
                        RolUsuario.Dueno => 1,
                        RolUsuario.Gerente => 2,
                        RolUsuario.Cajero => 4,
                        RolUsuario.Mozo => 5,
                        _ => 5
                    };
                    cuentaId = await _pagoService.CrearCuentaAsync(SelectedMesa.IdMesa, dni, idRol);
                }
            }

            return new CuentaPendienteViewModel
            {
                CuentaId = cuentaId,
                IdMesa = SelectedMesa.IdMesa,
                NroMesa = SelectedMesa.NroMesa,
                Sector = SelectedMesa.UbicacionDescripcion,
                MontoConsumo = SelectedMesa.ConsumoActual,
                AtendidoPor = string.IsNullOrWhiteSpace(SelectedMesa.MozoNombre) ? "Mozo de Turno" : SelectedMesa.MozoNombre,
                ClienteNombre = string.IsNullOrWhiteSpace(SelectedMesa.ClienteNombre) ? "Cliente Particular" : SelectedMesa.ClienteNombre
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[INICIO OBTENER CUENTA ERROR] {ex.Message}");
            return null;
        }
    }
}
