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
    private ObservableCollection<ReservaItemViewModel> _reservasFiltradas = new();

    [ObservableProperty]
    private ObservableCollection<ReservaItemViewModel> _reservasBajas = new();

    [ObservableProperty]
    private bool _esVistaPrincipal = true;

    [ObservableProperty]
    private bool _esVistaBajas = false;

    [ObservableProperty]
    private string _origenDatosTexto = "🟢 Base de datos";

    [ObservableProperty]
    private string _origenDatosColor = "#27AE60";

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private int _totalReservasCount;

    [ObservableProperty]
    private int _totalPersonasCount;

    [ObservableProperty]
    private int _bajasCount;

    partial void OnTextoBusquedaChanged(string value)
    {
        AplicarFiltro();
    }

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
            reserva.EstadoTexto = "Confirmada";
            ReservasBajas.Remove(reserva);
            Reservas.Add(reserva);
            AplicarFiltro();
        }
    }

    private void CargarReservasBajas()
    {
        if (!ReservasBajas.Any())
        {
            ReservasBajas = new ObservableCollection<ReservaItemViewModel>
            {
                new ReservaItemViewModel { IdReserva = 999, FechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Cliente Cancelado", NroMesa = "Ninguna", CantidadPersonas = 2, EstadoTexto = "Cancelada" }
            };
        }
        BajasCount = ReservasBajas.Count;
    }

    [RelayCommand]
    private async Task CancelarReservaAsync(ReservaItemViewModel reserva)
    {
        if (reserva != null)
        {
            if (_reservaService != null)
            {
                try
                {
                    await _reservaService.CambiarEstadoReservaAsync(reserva.IdReserva, 2); // 2 = Cancelado
                }
                catch { }
            }
            reserva.EstadoTexto = "Cancelada";
            Reservas.Remove(reserva);
            ReservasBajas.Add(reserva);
            AplicarFiltro();
        }
    }

    public void AgregarOActualizarReserva(ReservaItemViewModel nuevaReserva)
    {
        var existente = Reservas.FirstOrDefault(r => r.IdReserva == nuevaReserva.IdReserva && nuevaReserva.IdReserva > 0);
        if (existente != null)
        {
            existente.FechaHora = nuevaReserva.FechaHora;
            existente.ClienteNombre = nuevaReserva.ClienteNombre;
            existente.NroMesa = nuevaReserva.NroMesa;
            existente.CantidadPersonas = nuevaReserva.CantidadPersonas;
            existente.EstadoTexto = nuevaReserva.EstadoTexto;
        }
        else
        {
            if (nuevaReserva.IdReserva <= 0)
            {
                nuevaReserva.IdReserva = (Reservas.Max(r => (int?)r.IdReserva) ?? 100) + 1;
            }
            Reservas.Add(nuevaReserva);
        }
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        if (string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            ReservasFiltradas = new ObservableCollection<ReservaItemViewModel>(Reservas);
        }
        else
        {
            var q = TextoBusqueda.ToLower().Trim();
            var filtrados = Reservas.Where(r => 
                r.ClienteNombre.ToLower().Contains(q) ||
                r.NroMesa.ToLower().Contains(q) ||
                r.FechaHora.ToLower().Contains(q)
            );
            ReservasFiltradas = new ObservableCollection<ReservaItemViewModel>(filtrados);
        }

        ActualizarEstadisticas();
    }

    private void ActualizarEstadisticas()
    {
        TotalReservasCount = Reservas.Count;
        TotalPersonasCount = Reservas.Sum(r => r.CantidadPersonas);
        BajasCount = ReservasBajas.Count;
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
                var listaEntidades = await _reservaService.ObtenerReservasAsync();
                
                foreach (var r in listaEntidades)
                {
                    string nombreCliente = r.Cliente?.PersonaInfo != null
                        ? $"{r.Cliente.PersonaInfo.Nombre} {r.Cliente.PersonaInfo.Apellido}".Trim()
                        : "Cliente General";

                    string mesasAsignadas = r.Mesas != null && r.Mesas.Any()
                        ? string.Join(", ", r.Mesas.Select(m => m.NroMesa))
                        : "Sin asignar";

                    listaMapeada.Add(new ReservaItemViewModel
                    {
                        IdReserva = r.IdReserva,
                        FechaHora = r.FechaReserva.ToString("dd/MM/yyyy HH:mm"), 
                        ClienteNombre = nombreCliente,
                        NroMesa = mesasAsignadas,
                        CantidadPersonas = r.CantPersonas,
                        EstadoTexto = "Confirmada"
                    });
                }
            }
            catch
            {
            }
        }

        if (!listaMapeada.Any())
        {
            OrigenDatosTexto = "🟠 Mock";
            OrigenDatosColor = "#E67E22";
            listaMapeada = new List<ReservaItemViewModel>
            {
                new ReservaItemViewModel { IdReserva = 101, FechaHora = DateTime.Now.AddHours(2).ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Roberto Gómez", NroMesa = "Mesa 7", CantidadPersonas = 4, EstadoTexto = "Confirmada" },
                new ReservaItemViewModel { IdReserva = 102, FechaHora = DateTime.Now.AddHours(4).ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Laura Fernández", NroMesa = "Mesa 10 (VIP)", CantidadPersonas = 2, EstadoTexto = "Confirmada" },
                new ReservaItemViewModel { IdReserva = 103, FechaHora = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy HH:mm"), ClienteNombre = "Empresa ACME", NroMesa = "Mesa 11, 12", CantidadPersonas = 8, EstadoTexto = "Confirmada" }
            };
        }
        else
        {
            OrigenDatosTexto = "🟢 Base de datos";
            OrigenDatosColor = "#27AE60";
        }

        Reservas = new ObservableCollection<ReservaItemViewModel>(listaMapeada);
        CargarReservasBajas();
        AplicarFiltro();
    }
}