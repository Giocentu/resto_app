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
                catch (Exception ex)
                {
                    Console.WriteLine($"[RESERVAS DB ERROR] {ex.Message}");
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }
            reserva.EstadoTexto = "Cancelada";
            Reservas.Remove(reserva);
            ReservasBajas.Add(reserva);
            AplicarFiltro();
        }
    }

    public async Task GuardarReservaAsync(ReservaItemViewModel resItem)
    {
        if (resItem == null) return;

        if (_reservaService != null)
        {
            try
            {
                DateTime dt = DateTime.Now.AddHours(2);
                if (DateTime.TryParse(resItem.FechaHora, out DateTime parsed))
                {
                    dt = parsed;
                }

                int? nroMesa = null;
                if (int.TryParse(resItem.NroMesa, out int parsedMesa))
                {
                    nroMesa = parsedMesa;
                }

                var listaExistente = await _reservaService.ObtenerReservasAsync();
                long dniCliente = listaExistente.FirstOrDefault(r => r.DniCliente > 0)?.DniCliente ?? 46452703;

                await _reservaService.CrearReservaAsync(
                    fechaReserva: dt,
                    cantPersonas: resItem.CantidadPersonas,
                    idEstado: 1,
                    dniCliente: dniCliente,
                    idEvento: null,
                    dniEmpleado: null,
                    idRol: null,
                    idMesa: nroMesa
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESERVAS GUARDAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarReservasAsync();
    }

    public void AgregarOActualizarReserva(ReservaItemViewModel nuevaReserva)
    {
        _ = GuardarReservaAsync(nuevaReserva);
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
            catch (Exception ex)
            {
                Console.WriteLine($"[RESERVAS DB ERROR] Error al cargar reservas: {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Reservas = new ObservableCollection<ReservaItemViewModel>(listaMapeada);
        CargarReservasBajas();
        AplicarFiltro();
    }
}