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
    private void RestaurarMesa(MesaItemViewModel mesa)
    {
        if (mesa != null)
        {
            MesasBajas.Remove(mesa);
            Mesas.Add(mesa);
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
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                var listaEntidades = await _mesaService.ObtenerMesasAsync().WaitAsync(cts.Token);
                
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
            catch
            {
            }
        }

        if (!listaMapeada.Any())
        {
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

        Mesas = new ObservableCollection<MesaItemViewModel>(listaMapeada);
    }
    private readonly Action _volverInicio;



    [RelayCommand]
    private void VolverInicio()
    {
        _volverInicio?.Invoke();
    }
}