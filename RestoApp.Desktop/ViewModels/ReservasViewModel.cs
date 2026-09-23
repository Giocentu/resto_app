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
    private async Task RestaurarReservaAsync(ReservaItemViewModel reserva)
    {
        if (reserva != null)
        {
            if (_reservaService != null)
            {
                try
                {
                    await _reservaService.CambiarEstadoReservaAsync(reserva.IdReserva, 1); // 1 = Confirmado
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RESERVAS RESTAURAR DB ERROR] {ex.Message}");
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }
            reserva.EstadoTexto = "Confirmada";
            ReservasBajas.Remove(reserva);
            await CargarReservasAsync();
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
                if (TryParseFechaHora(resItem.FechaHora, out DateTime parsed))
                {
                    dt = parsed;
                }

                int? idMesa = resItem.IdMesa > 0 ? resItem.IdMesa : (int.TryParse(resItem.NroMesa, out int parsedMesa) ? parsedMesa : null);

                if (resItem.IdReserva > 0)
                {
                    await _reservaService.EditarReservaAsync(resItem.IdReserva, dt, resItem.CantidadPersonas, 1);
                }
                else
                {
                    long dniCliente = resItem.DniCliente > 0
                        ? await _reservaService.ObtenerOCrearClientePorDniYNombreAsync(resItem.DniCliente, resItem.ClienteNombre)
                        : await _reservaService.ObtenerOCrearClientePorNombreAsync(resItem.ClienteNombre);

                    await _reservaService.CrearReservaAsync(
                        fechaReserva: dt,
                        cantPersonas: resItem.CantidadPersonas,
                        idEstado: 1,
                        dniCliente: dniCliente,
                        idEvento: resItem.IdEvento,
                        dniEmpleado: null,
                        idRol: null,
                        idMesa: idMesa
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESERVAS GUARDAR DB ERROR] {ex.Message}");
                string errorDetail = ex.InnerException?.Message ?? ex.Message;
                string mensaje = errorDetail.Contains("chk_reserva_fecha_futura")
                    ? "⚠️ No se pudo guardar la reserva: La fecha y hora de la reserva debe ser en el futuro."
                    : $"⚠️ Error al guardar reserva: {errorDetail}";
                _ = AlertaService.MostrarAlertaConexionAsync(mensaje);
            }
        }

        await CargarReservasAsync();
    }

    private static bool TryParseFechaHora(string? texto, out DateTime dt)
    {
        dt = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(texto)) return false;

        string[] formats = new[]
        {
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HH:mm:ss",
            "d/M/yyyy HH:mm",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "g",
            "G"
        };

        if (DateTime.TryParseExact(texto, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            return true;

        if (DateTime.TryParse(texto, System.Globalization.CultureInfo.GetCultureInfo("es-AR"), System.Globalization.DateTimeStyles.None, out dt))
            return true;

        return DateTime.TryParse(texto, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt);
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

                    int idMesaPrincipal = r.Mesas != null && r.Mesas.Any() ? r.Mesas.First().IdMesa : 0;

                    listaMapeada.Add(new ReservaItemViewModel
                    {
                        IdReserva = r.IdReserva,
                        FechaHora = r.FechaReserva.ToString("dd/MM/yyyy HH:mm"), 
                        ClienteNombre = nombreCliente,
                        DniCliente = r.DniCliente,
                        IdMesa = idMesaPrincipal,
                        NroMesa = mesasAsignadas,
                        CantidadPersonas = r.CantPersonas,
                        EstadoTexto = r.IdEstado == 2 ? "Cancelada" : "Confirmada"
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

        var activas = listaMapeada.Where(r => r.EstadoTexto != "Cancelada").ToList();
        var bajas = listaMapeada.Where(r => r.EstadoTexto == "Cancelada").ToList();

        Reservas = new ObservableCollection<ReservaItemViewModel>(activas);
        ReservasBajas = new ObservableCollection<ReservaItemViewModel>(bajas);
        CargarReservasBajas();
        AplicarFiltro();
    }
}