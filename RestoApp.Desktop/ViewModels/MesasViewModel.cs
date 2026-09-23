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
    private bool _esVistaPrincipal = true;

    [ObservableProperty]
    private bool _esVistaBajas = false;

    [RelayCommand]
    private void VerBajas()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = true;
        _ = CargarMesasBajasAsync();
    }

    [RelayCommand]
    private void VolverPrincipal()
    {
        EsVistaPrincipal = true;
        EsVistaBajas = false;
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
            MesasBajas.Remove(mesa);
            Mesas.Add(mesa);
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
            Mesas.Remove(mesa);
            MesasBajas.Add(mesa);
        }
    }

    private async Task CargarMesasBajasAsync()
    {
        if (_mesaService != null)
        {
            try
            {
                var inactivas = await _mesaService.ObtenerMesasAsync(soloActivas: false);
                var listaBajas = inactivas.Where(m => string.Equals(m.Estado, "INACTIVA", StringComparison.OrdinalIgnoreCase) || string.Equals(m.Estado, "BAJA", StringComparison.OrdinalIgnoreCase))
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

    public MesasViewModel(MesaService? mesaService, Action volverInicio)
    {
        _volverInicio = volverInicio;
        _mesaService = mesaService;
        _ = CargarMesasAsync();
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
    }

    public async Task GuardarMesaAsync(MesaItemViewModel mesa)
    {
        if (mesa == null) return;

        int idUbicacion = mesa.UbicacionDescripcion?.ToLower() switch
        {
            "terraza" => 1,
            "2dopiso" or "segundo piso" or "2do piso" => 2,
            "plantabaja" or "planta baja" => 3,
            "patio" => 4,
            _ => 1
        };

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

    private readonly Action _volverInicio;



    [RelayCommand]
    private void VolverInicio()
    {
        _volverInicio?.Invoke();
    }
}