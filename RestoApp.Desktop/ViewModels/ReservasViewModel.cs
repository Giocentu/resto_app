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

public partial class ReservasViewModel : ObservableObject
{
    private readonly ReservaService? _reservaService;

    public bool PuedeAgregarEditar => SesionGlobal.RolActual == RolUsuario.Dueno
                        || SesionGlobal.RolActual == RolUsuario.Gerente
                        || SesionGlobal.RolActual == RolUsuario.Recepcion;
    [ObservableProperty]
    private ObservableCollection<ReservaItemViewModel> _reservas = new();

    [ObservableProperty]
    private ObservableCollection<ReservaItemViewModel> _reservasBajas = new();

    [ObservableProperty]
    private bool _esVistaPrincipal = true;

    [ObservableProperty]
    private bool _esVistaBajas = false;

    [RelayCommand]
    private void VerBajas()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = true;
        CargarReservasBajas();
    }

    [RelayCommand]
    private void VolverPrincipal()
    {
        EsVistaPrincipal = true;
        EsVistaBajas = false;
    }

    [RelayCommand]
    private void RestaurarReserva(ReservaItemViewModel reserva)
    {
        if (reserva != null)
        {
            ReservasBajas.Remove(reserva);
            Reservas.Add(reserva);
        }
    }

    private void CargarReservasBajas()
    {
        if (!ReservasBajas.Any())
        {
            ReservasBajas = new ObservableCollection<ReservaItemViewModel>
            {
                new ReservaItemViewModel { IdReserva = 999, FechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Cliente Cancelado", NroMesa = "Ninguna", CantidadPersonas = 2 }
            };
        }
    }

    public ReservasViewModel(ReservaService? reservaService = null)
    {
        _reservaService = reservaService;
        _ = CargarReservasAsync();
    }

    public async Task CargarReservasAsync()
    {
        var listaMapeada = new List<ReservaItemViewModel>();

        if (_reservaService != null)
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                var listaEntidades = await _reservaService.ObtenerReservasAsync().WaitAsync(cts.Token);
                
                foreach (var r in listaEntidades)
                {
                    string nombreCliente = r.Cliente?.PersonaInfo?.Nombre ?? "Cliente General";
                    string mesasAsignadas = r.Mesas != null && r.Mesas.Any()
                        ? string.Join(", ", r.Mesas.Select(m => m.NroMesa))
                        : "Sin asignar";

                    listaMapeada.Add(new ReservaItemViewModel
                    {
                        IdReserva = r.IdReserva,
                        FechaHora = r.FechaReserva.ToString("dd/MM/yyyy HH:mm"), 
                        ClienteNombre = nombreCliente,
                        NroMesa = mesasAsignadas,
                        CantidadPersonas = r.CantPersonas
                    });
                }
            }
            catch
            {
            }
        }

        if (!listaMapeada.Any())
        {
            listaMapeada = new List<ReservaItemViewModel>
            {
                new ReservaItemViewModel { IdReserva = 101, FechaHora = DateTime.Now.AddHours(2).ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Roberto Gómez", NroMesa = "Mesa 7", CantidadPersonas = 4 },
                new ReservaItemViewModel { IdReserva = 102, FechaHora = DateTime.Now.AddHours(4).ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Laura Fernández", NroMesa = "Mesa 10 (VIP)", CantidadPersonas = 2 },
                new ReservaItemViewModel { IdReserva = 103, FechaHora = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Empresa ACME", NroMesa = "Mesa 11, 12", CantidadPersonas = 8 }
            };
        }

        Reservas = new ObservableCollection<ReservaItemViewModel>(listaMapeada);
    }
}