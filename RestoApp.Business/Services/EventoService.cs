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

    public async Task<IEnumerable<Evento>> ObtenerEventosAsync()
    {
        return await _eventoRepository.GetEventosAsync();
    }

    public async Task RegistrarEventoAsync(Evento evento)
    {
        await _eventoRepository.AddAsync(evento);
        await _eventoRepository.SaveChangesAsync();
    }

    public async Task EliminarEventoAsync(int idEvento)
    {
        var evento = await _eventoRepository.GetByIdAsync(idEvento);
        if (evento != null)
        {
            _eventoRepository.Delete(evento);
            await _eventoRepository.SaveChangesAsync();
        }
    }
}
