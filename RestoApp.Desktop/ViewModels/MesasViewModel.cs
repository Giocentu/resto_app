using CommunityToolkit.Mvvm.ComponentModel;
using RestoApp.Business.Services;
using RestoApp.Entities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class MesasViewModel : ObservableObject
{
    private readonly MesaService _mesaService;


    // Propiedad que Avalonia leerá para el botón principal
    
    public bool PuedeAgregar => SesionGlobal.RolActual == RolUsuario.Dueno;
    public bool PuedeEditar => SesionGlobal.RolActual == RolUsuario.Dueno
                        || SesionGlobal.RolActual == RolUsuario.Gerente
                        || SesionGlobal.RolActual == RolUsuario.Recepcion
                        || SesionGlobal.RolActual == RolUsuario.Cajero;

    [ObservableProperty]
    private ObservableCollection<MesaItemViewModel> _mesas = new();

    public MesasViewModel(MesaService mesaService)
    {
        _mesaService = mesaService;
        _ = CargarMesasAsync();
    }

    public async Task CargarMesasAsync()
{
    var listaEntidades = await _mesaService.ObtenerMesasAsync();
    var listaMapeada = new ObservableCollection<MesaItemViewModel>();
    
    foreach (var m in listaEntidades)
    {
        listaMapeada.Add(new MesaItemViewModel
        {
            IdMesa = m.IdMesa,
            NroMesa = m.NroMesa,
            Capacidad = m.Capacidad,
            UbicacionDescripcion = m.Ubicacion?.Ubicacion ?? "Sin ubicación",
            
            // Aquí inyectamos el permiso global a cada fila individualmente

        });
    }
    Mesas = listaMapeada;
}
}