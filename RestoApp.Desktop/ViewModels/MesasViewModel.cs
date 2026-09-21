using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
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

    [ObservableProperty]
    private string _origenDatosTexto = "🟢 Base de datos";

    [ObservableProperty]
    private string _origenDatosColor = "#27AE60";

    [RelayCommand]
    private void VerBajas()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = true;
        CargarMesasBajas();
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
                catch { }
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
                catch { }
            }
            Mesas.Remove(mesa);
            MesasBajas.Add(mesa);
        }
    }

    private void CargarMesasBajas()
    {
        if (!MesasBajas.Any())
        {
            MesasBajas = new ObservableCollection<MesaItemViewModel>
            {
                new MesaItemViewModel { IdMesa = 99, NroMesa = 99, Capacidad = 4, UbicacionDescripcion = "Depósito" }
            };
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
            }
        }

        if (!listaMapeada.Any())
        {
            OrigenDatosTexto = "🟠 Mock";
            OrigenDatosColor = "#E67E22";
            listaMapeada = new List<MesaItemViewModel>
            {
                new MesaItemViewModel { IdMesa = 1, NroMesa = 1, Capacidad = 2, UbicacionDescripcion = "Salón Principal" },
                new MesaItemViewModel { IdMesa = 2, NroMesa = 2, Capacidad = 4, UbicacionDescripcion = "Salón Principal" },
                new MesaItemViewModel { IdMesa = 3, NroMesa = 3, Capacidad = 4, UbicacionDescripcion = "Salón Principal" },
                new MesaItemViewModel { IdMesa = 4, NroMesa = 4, Capacidad = 6, UbicacionDescripcion = "Salón Principal" },
                new MesaItemViewModel { IdMesa = 5, NroMesa = 5, Capacidad = 2, UbicacionDescripcion = "Terraza" },
                new MesaItemViewModel { IdMesa = 6, NroMesa = 6, Capacidad = 4, UbicacionDescripcion = "Terraza" },
                new MesaItemViewModel { IdMesa = 7, NroMesa = 7, Capacidad = 4, UbicacionDescripcion = "Terraza" },
                new MesaItemViewModel { IdMesa = 8, NroMesa = 8, Capacidad = 2, UbicacionDescripcion = "Barra" },
                new MesaItemViewModel { IdMesa = 9, NroMesa = 9, Capacidad = 2, UbicacionDescripcion = "Barra" },
                new MesaItemViewModel { IdMesa = 10, NroMesa = 10, Capacidad = 8, UbicacionDescripcion = "VIP" },
                new MesaItemViewModel { IdMesa = 11, NroMesa = 11, Capacidad = 6, UbicacionDescripcion = "VIP" }
            };
        }
        else
        {
            OrigenDatosTexto = "🟢 Base de datos";
            OrigenDatosColor = "#27AE60";
        }

        Mesas = new ObservableCollection<MesaItemViewModel>(listaMapeada);
    }

    private readonly Action _volverInicio;



    [RelayCommand]
    private void VolverInicio()
    {
        _volverInicio?.Invoke();
    }
}