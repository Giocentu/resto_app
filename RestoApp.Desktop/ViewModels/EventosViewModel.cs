using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Desktop.Services;
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
            catch (Exception ex)
            {
                Console.WriteLine($"[EVENTOS DB ERROR] Error al cargar eventos: {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Eventos = new ObservableCollection<EventoItemViewModel>(lista);
    }

    [RelayCommand]
    private async Task RegistrarEventoAsync()
    {
        if (string.IsNullOrWhiteSpace(NuevoNombre)) return;

        string nombre = NuevoNombre.Trim();
        DateTime fecha = NuevaFecha?.DateTime ?? DateTime.Now;
        string desc = string.IsNullOrWhiteSpace(NuevaDescripcion) ? "Evento especial y promociones para clientes." : NuevaDescripcion.Trim();

        if (_eventoService != null)
        {
            try
            {
                var entity = new Evento
                {
                    NombreEvento = nombre,
                    FechaEvento = fecha,
                    Descripcion = desc,
                    EsActivo = true
                };
                await _eventoService.RegistrarEventoAsync(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EVENTOS REGISTRAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarEventosAsync();

        // Limpiar formulario
        NuevoNombre = string.Empty;
        NuevaDescripcion = string.Empty;
        NuevaFecha = DateTimeOffset.Now.AddDays(14);
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync(EventoItemViewModel? item)
    {
        if (item == null || _eventoService == null) return;

        bool nuevoEstado = !item.EsActivo;
        try
        {
            await _eventoService.CambiarEstadoEventoAsync(item.IdEvento, nuevoEstado);
            item.EsActivo = nuevoEstado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EVENTOS CAMBIAR ESTADO DB ERROR] {ex.Message}");
            _ = AlertaService.MostrarAlertaConexionAsync();
        }
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
            catch (Exception ex)
            {
                Console.WriteLine($"[EVENTOS ELIMINAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarEventosAsync();
    }
}
