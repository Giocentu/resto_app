using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class EventosViewModel : ObservableObject
{
    private readonly EventoService? _eventoService;

    [ObservableProperty]
    private ObservableCollection<EventoItemViewModel> _eventos = new();

    // Campos del Formulario "Nuevo Evento Especial"
    [ObservableProperty]
    private string _nuevoNombre = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _nuevaFecha = DateTimeOffset.Now.AddDays(14);

    [ObservableProperty]
    private string _nuevaDescripcion = string.Empty;

    [ObservableProperty]
    private string _origenDatosTexto = "🟢 Base de datos";

    [ObservableProperty]
    private string _origenDatosColor = "#27AE60";

    public EventosViewModel(EventoService? eventoService = null)
    {
        _eventoService = eventoService;
        _ = CargarEventosAsync();
    }

    public async Task CargarEventosAsync()
    {
        var lista = new List<EventoItemViewModel>();

        if (_eventoService != null)
        {
            try
            {
                var entidades = await _eventoService.ObtenerEventosAsync(soloActivos: true);
                foreach (var e in entidades)
                {
                    lista.Add(new EventoItemViewModel
                    {
                        IdEvento = e.IdEvento,
                        NombreEvento = e.NombreEvento,
                        FechaEvento = e.FechaEvento ?? DateTime.Now,
                        Descripcion = e.Descripcion ?? string.Empty,
                        EsActivo = e.EsActivo,
                        CantReservasVinculadas = e.CantReservasVinculadas
                    });
                }
            }
            catch
            {
                // Fallback a datos demostrativos
            }
        }


        // Si la base no devolvió datos, cargar catálogo demostrativo interactivo como en la imagen
        if (!lista.Any())
        {
            OrigenDatosTexto = "🟠 Mock";
            OrigenDatosColor = "#E67E22";
            lista = new List<EventoItemViewModel>
            {
                new()
                {
                    IdEvento = 1,
                    NombreEvento = "Día del Amigo",
                    FechaEvento = new DateTime(2026, 10, 5),
                    Descripcion = "Menú festivo con copa de bienvenida para grupos de amigos.",
                    EsActivo = true,
                    CantReservasVinculadas = 12
                },
                new()
                {
                    IdEvento = 2,
                    NombreEvento = "Cena Show Jazz",
                    FechaEvento = new DateTime(2026, 9, 23),
                    Descripcion = "Presentación en vivo del quinteto de Jazz 'Blue Note'.",
                    EsActivo = true,
                    CantReservasVinculadas = 8
                },
                new()
                {
                    IdEvento = 3,
                    NombreEvento = "Año Nuevo RestoApp",
                    FechaEvento = new DateTime(2026, 12, 31),
                    Descripcion = "Gran cena de fin de año con brindis y DJ.",
                    EsActivo = true,
                    CantReservasVinculadas = 25
                },
                new()
                {
                    IdEvento = 4,
                    NombreEvento = "San Valentín",
                    FechaEvento = new DateTime(2026, 2, 14),
                    Descripcion = "Cena romántica de 3 pasos con maridaje de vinos.",
                    EsActivo = false,
                    CantReservasVinculadas = 0
                }
            };
        }
        else
        {
            OrigenDatosTexto = "🟢 Base de datos";
            OrigenDatosColor = "#27AE60";
        }

        Eventos = new ObservableCollection<EventoItemViewModel>(lista);
    }

    [RelayCommand]
    private async Task RegistrarEventoAsync()
    {
        if (string.IsNullOrWhiteSpace(NuevoNombre)) return;

        int nuevoId = Eventos.Any() ? Eventos.Max(e => e.IdEvento) + 1 : 1;
        var nuevoItem = new EventoItemViewModel
        {
            IdEvento = nuevoId,
            NombreEvento = NuevoNombre.Trim(),
            FechaEvento = NuevaFecha?.DateTime ?? DateTime.Now,
            Descripcion = string.IsNullOrWhiteSpace(NuevaDescripcion) ? "Evento especial y promociones para clientes." : NuevaDescripcion.Trim(),
            EsActivo = true,
            CantReservasVinculadas = 0
        };

        if (_eventoService != null)
        {
            try
            {
                var entity = new Evento
                {
                    NombreEvento = nuevoItem.NombreEvento,
                    FechaEvento = nuevoItem.FechaEvento,
                    Descripcion = nuevoItem.Descripcion,
                    EsActivo = true
                };
                await _eventoService.RegistrarEventoAsync(entity);
            }
            catch
            {
            }
        }

        Eventos.Insert(0, nuevoItem);

        // Limpiar formulario
        NuevoNombre = string.Empty;
        NuevaDescripcion = string.Empty;
        NuevaFecha = DateTimeOffset.Now.AddDays(14);
    }

    [RelayCommand]
    private async Task EliminarEventoAsync(EventoItemViewModel? item)
    {
        if (item == null) return;

        if (_eventoService != null)
        {
            try
            {
                await _eventoService.EliminarEventoAsync(item.IdEvento);
            }
            catch
            {
            }
        }

        Eventos.Remove(item);
    }
}
