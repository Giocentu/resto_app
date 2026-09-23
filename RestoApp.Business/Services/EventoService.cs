using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class EventoService
{
    private readonly IEventoRepository _eventoRepository;

    public EventoService(IEventoRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
    }

    public async Task<IEnumerable<Evento>> ObtenerEventosAsync(bool soloActivos = true)
    {
        try
        {
            return await _eventoRepository.GetEventosSpAsync(soloActivos);
        }
        catch
        {
            return await _eventoRepository.GetEventosAsync();
        }
    }

    public async Task<int> RegistrarEventoAsync(Evento evento)
    {
        try
        {
            return await _eventoRepository.CrearEventoSpAsync(evento.NombreEvento, evento.FechaEvento, evento.Descripcion);
        }
        catch
        {
            await _eventoRepository.AddAsync(evento);
            await _eventoRepository.SaveChangesAsync();
            return evento.IdEvento;
        }
    }

    public async Task CambiarEstadoEventoAsync(int idEvento, bool esActivo)
    {
        var evento = await _eventoRepository.GetByIdAsync(idEvento);
        if (evento == null)
            throw new InvalidOperationException($"No se encontró el evento {idEvento}.");

        evento.EsActivo = esActivo;
        _eventoRepository.Update(evento);
        await _eventoRepository.SaveChangesAsync();
    }

    public async Task EliminarEventoAsync(int idEvento)
    {
        try
        {
            await _eventoRepository.BajaLogicaEventoSpAsync(idEvento);
        }
        catch
        {
            var evento = await _eventoRepository.GetByIdAsync(idEvento);
            if (evento != null)
            {
                _eventoRepository.Delete(evento);
                await _eventoRepository.SaveChangesAsync();
            }
        }
    }
}
