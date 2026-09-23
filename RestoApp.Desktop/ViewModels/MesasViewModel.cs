using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Desktop.Services;
using RestoApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class MesasViewModel : ObservableObject
{
    private readonly MesaService? _mesaService;
    private readonly UbicacionService? _ubicacionService;

    public bool PuedeAgregar => SesionGlobal.RolActual == RolUsuario.Dueno;
    public bool PuedeEditar => SesionGlobal.RolActual == RolUsuario.Dueno
                        || SesionGlobal.RolActual == RolUsuario.Gerente
                        || SesionGlobal.RolActual == RolUsuario.Recepcion
                        || SesionGlobal.RolActual == RolUsuario.Cajero;

    [ObservableProperty]
    private ObservableCollection<MesaItemViewModel> _mesas = new();

    [ObservableProperty]
    private ObservableCollection<MesaItemViewModel> _mesasBajas = new();

    [ObservableProperty]
    private ObservableCollection<MesaItemViewModel> _mesasFiltradas = new();

    [ObservableProperty]
    private ObservableCollection<string> _filtroUbicaciones = new() { "Todas" };

    [ObservableProperty]
    private string _filtroUbicacionSeleccionada = "Todas";

    partial void OnFiltroUbicacionSeleccionadaChanged(string value)
    {
        AplicarFiltro();
    }

    [ObservableProperty]
    private ObservableCollection<UbicacionMesa> _ubicaciones = new();

    [ObservableProperty]
    private ObservableCollection<UbicacionMesa> _ubicacionesBajas = new();

    [ObservableProperty]
    private bool _esVistaPrincipal = true;

    [ObservableProperty]
    private bool _esVistaBajas = false;

    [ObservableProperty]
    private bool _esVistaUbicaciones = false;

    [ObservableProperty]
    private bool _esVistaUbicacionesBajas = false;

    public MesasViewModel(MesaService? mesaService, Action volverInicio, UbicacionService? ubicacionService = null)
    {
        _volverInicio = volverInicio;
        _mesaService = mesaService;
        _ubicacionService = ubicacionService ?? App.Services?.GetService(typeof(UbicacionService)) as UbicacionService;
        _ = CargarMesasAsync();
        _ = CargarUbicacionesAsync();
    }

    [RelayCommand]
    private void VerBajas()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = true;
        EsVistaUbicaciones = false;
        EsVistaUbicacionesBajas = false;
        _ = CargarMesasBajasAsync();
    }

    [RelayCommand]
    private void VolverPrincipal()
    {
        EsVistaPrincipal = true;
        EsVistaBajas = false;
        EsVistaUbicaciones = false;
        EsVistaUbicacionesBajas = false;
    }

    [RelayCommand]
    private void VerUbicacionesSection()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = false;
        EsVistaUbicaciones = true;
        EsVistaUbicacionesBajas = false;
        _ = CargarUbicacionesAsync();
    }

    [RelayCommand]
    private void VerUbicacionesBajasSection()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = false;
        EsVistaUbicaciones = false;
        EsVistaUbicacionesBajas = true;
        _ = CargarUbicacionesBajasAsync();
    }

    [RelayCommand]
    private async Task RestaurarMesaAsync(MesaItemViewModel mesa)
    {
        if (mesa != null)
        {
            if (_mesaService != null)
            {
                try
                {
                    await _mesaService.RestaurarMesaAsync(mesa.IdMesa);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MESAS DB ERROR] {ex.Message}");
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }
            var itemBaja = MesasBajas.FirstOrDefault(m => m.IdMesa == mesa.IdMesa);
            if (itemBaja != null)
            {
                MesasBajas.Remove(itemBaja);
            }
            await CargarMesasAsync();
            await CargarMesasBajasAsync();
        }
    }

    [RelayCommand]
    private async Task DarBajaMesaAsync(MesaItemViewModel mesa)
    {
        if (mesa != null)
        {
            if (_mesaService != null)
            {
                try
                {
                    await _mesaService.BajaLogicaMesaAsync(mesa.IdMesa);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MESAS DB ERROR] {ex.Message}");
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }
            var itemMesa = Mesas.FirstOrDefault(m => m.IdMesa == mesa.IdMesa);
            if (itemMesa != null)
            {
                Mesas.Remove(itemMesa);
            }
            await CargarMesasAsync();
            await CargarMesasBajasAsync();
        }
    }

    public async Task CargarMesasBajasAsync()
    {
        if (_mesaService != null)
        {
            try
            {
                var inactivas = await _mesaService.ObtenerMesasAsync(soloActivas: false);
                var listaBajas = inactivas
                    .Where(m => !m.EsActivo || string.Equals(m.Estado, "INACTIVA", StringComparison.OrdinalIgnoreCase) || string.Equals(m.Estado, "BAJA", StringComparison.OrdinalIgnoreCase))
                    .Select(m => new MesaItemViewModel
                    {
                        IdMesa = m.IdMesa,
                        NroMesa = m.NroMesa,
                        Capacidad = m.Capacidad,
                        UbicacionDescripcion = m.Ubicacion?.Ubicacion ?? "Sin ubicación"
                    });
                MesasBajas = new ObservableCollection<MesaItemViewModel>(listaBajas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MESAS BAJAS DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
    }

    public void AplicarFiltro()
    {
        if (string.IsNullOrEmpty(FiltroUbicacionSeleccionada) || FiltroUbicacionSeleccionada == "Todas")
        {
            MesasFiltradas = new ObservableCollection<MesaItemViewModel>(Mesas);
        }
        else
        {
            MesasFiltradas = new ObservableCollection<MesaItemViewModel>(
                Mesas.Where(m => string.Equals(m.UbicacionDescripcion, FiltroUbicacionSeleccionada, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    public async Task CargarFiltroUbicacionesAsync()
    {
        var lista = new List<string> { "Todas" };
        if (_ubicacionService != null)
        {
            try
            {
                var uList = await _ubicacionService.ObtenerUbicacionesAsync(soloActivas: true);
                foreach (var u in uList)
                {
                    if (!string.IsNullOrWhiteSpace(u.Ubicacion) && !lista.Contains(u.Ubicacion))
                    {
                        lista.Add(u.Ubicacion);
                    }
                }
            }
            catch { }
        }
        foreach (var m in Mesas)
        {
            if (!string.IsNullOrWhiteSpace(m.UbicacionDescripcion) && !lista.Contains(m.UbicacionDescripcion))
            {
                lista.Add(m.UbicacionDescripcion);
            }
        }
        FiltroUbicaciones = new ObservableCollection<string>(lista);
        if (!FiltroUbicaciones.Contains(FiltroUbicacionSeleccionada))
        {
            FiltroUbicacionSeleccionada = "Todas";
        }
        AplicarFiltro();
    }

    public async Task CargarMesasAsync()
    {
        var listaMapeada = new List<MesaItemViewModel>();

        if (_mesaService != null)
        {
            try
            {
                var listaEntidades = await _mesaService.ObtenerMesasAsync(soloActivas: true);
                
                foreach (var m in listaEntidades)
                {
                    listaMapeada.Add(new MesaItemViewModel
                    {
                        IdMesa = m.IdMesa,
                        NroMesa = m.NroMesa,
                        Capacidad = m.Capacidad,
                        UbicacionDescripcion = m.Ubicacion?.Ubicacion ?? "Sin ubicación"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MESAS DB ERROR] Error al cargar mesas de la BD: {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Mesas = new ObservableCollection<MesaItemViewModel>(listaMapeada);
        await CargarFiltroUbicacionesAsync();
    }

    public async Task GuardarMesaAsync(MesaItemViewModel mesa, int? idUbicacionEspecifica = null)
    {
        if (mesa == null) return;

        int idUbicacion = idUbicacionEspecifica ?? (mesa.UbicacionDescripcion?.ToLower() switch
        {
            "terraza" => 1,
            "2dopiso" or "segundo piso" or "2do piso" => 2,
            "plantabaja" or "planta baja" => 3,
            "patio" => 4,
            _ => 1
        });

        if (_mesaService != null)
        {
            try
            {
                if (mesa.IdMesa > 0)
                {
                    await _mesaService.EditarMesaAsync(mesa.IdMesa, mesa.NroMesa, mesa.Capacidad, idUbicacion, "LIBRE");
                }
                else
                {
                    await _mesaService.CrearMesaAsync(mesa.NroMesa, mesa.Capacidad, idUbicacion, "LIBRE");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MESAS GUARDAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarMesasAsync();
    }

    // Ubicaciones Logic
    public async Task CargarUbicacionesAsync()
    {
        if (_ubicacionService != null)
        {
            try
            {
                var uList = await _ubicacionService.ObtenerUbicacionesAsync(soloActivas: true);
                Ubicaciones = new ObservableCollection<UbicacionMesa>(uList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UBICACIONES DB ERROR] {ex.Message}");
            }
        }
    }

    public async Task CargarUbicacionesBajasAsync()
    {
        if (_ubicacionService != null)
        {
            try
            {
                var uList = await _ubicacionService.ObtenerUbicacionesAsync(soloActivas: false);
                UbicacionesBajas = new ObservableCollection<UbicacionMesa>(uList.Where(u => !u.EsActivo));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UBICACIONES BAJAS DB ERROR] {ex.Message}");
            }
        }
    }

    public async Task GuardarUbicacionAsync(UbicacionMesa u)
    {
        if (u == null || string.IsNullOrWhiteSpace(u.Ubicacion)) return;

        if (_ubicacionService != null)
        {
            try
            {
                if (u.IdUbicacion > 0)
                {
                    await _ubicacionService.EditarUbicacionAsync(u.IdUbicacion, u.Ubicacion);
                }
                else
                {
                    await _ubicacionService.CrearUbicacionAsync(u.Ubicacion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UBICACION GUARDAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarUbicacionesAsync();
        await CargarMesasAsync();
    }

    [RelayCommand]
    private async Task DarBajaUbicacionAsync(UbicacionMesa u)
    {
        if (u == null) return;
        if (_ubicacionService != null)
        {
            try
            {
                await _ubicacionService.BajaLogicaUbicacionAsync(u.IdUbicacion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UBICACION BAJA DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        await CargarUbicacionesAsync();
        await CargarUbicacionesBajasAsync();
    }

    [RelayCommand]
    private async Task RestaurarUbicacionAsync(UbicacionMesa u)
    {
        if (u == null) return;
        if (_ubicacionService != null)
        {
            try
            {
                await _ubicacionService.RestaurarUbicacionAsync(u.IdUbicacion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UBICACION RESTAURAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        await CargarUbicacionesAsync();
        await CargarUbicacionesBajasAsync();
    }

    private readonly Action _volverInicio;

    [RelayCommand]
    private void VolverInicio()
    {
        _volverInicio?.Invoke();
    }
}