using CommunityToolkit.Mvvm.ComponentModel;
using RestoApp.Business.Services;
using RestoApp.Entities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace RestoApp.Desktop.ViewModels;

public partial class ReservasViewModel : ObservableObject
{


    private readonly ReservaService _reservaService;


    public bool PuedeAgregarEditar => SesionGlobal.RolActual == RolUsuario.Dueno
                        || SesionGlobal.RolActual == RolUsuario.Gerente
                        || SesionGlobal.RolActual == RolUsuario.Recepcion
                        || SesionGlobal.RolActual == RolUsuario.Cajero;
    [ObservableProperty]
    private ObservableCollection<ReservaItemViewModel> _reservas = new();

    public ReservasViewModel(ReservaService reservaService)
    {
        _reservaService = reservaService;
        _ = CargarReservasAsync();
    }

// Reemplaza tu método CargarReservasAsync actual con este:
public async Task CargarReservasAsync()
{
    var listaEntidades = await _reservaService.ObtenerReservasAsync();
    var listaMapeada = new ObservableCollection<ReservaItemViewModel>();
    
    foreach (var r in listaEntidades)
    {
        // 1. Extraemos el nombre navegando hasta PersonaInfo
        // (Asegúrate de que la propiedad de texto se llame 'Nombre' o cámbiala por la correcta)
        string nombreCliente = r.Cliente?.PersonaInfo?.Nombre ?? "Consumidor Final";

        // 2. Extraemos las mesas. Como 'Mesas' es una colección, unimos los números con comas (Ej: "4, 5")
        string mesasAsignadas = r.Mesas != null && r.Mesas.Any()
            ? string.Join(", ", r.Mesas.Select(m => m.NroMesa))
            : "Sin asignar";

        listaMapeada.Add(new ReservaItemViewModel
        {
            IdReserva = r.IdReserva,
            FechaHora = r.FechaReserva.ToString("dd/MM/yyyy HH:mm"), 
            ClienteNombre = nombreCliente,     // Usamos la variable calculada
            NroMesa = mesasAsignadas,          // Usamos la variable calculada
            CantidadPersonas = r.CantPersonas,
            
            
        });
    }

    Reservas = listaMapeada;
}
}