using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public interface IEventoRepository : IRepository<Evento>
{
    Task<IEnumerable<Evento>> GetEventosAsync();
    Task<IEnumerable<Evento>> GetEventosSpAsync(bool soloActivos = true);
    Task<int> CrearEventoSpAsync(string nombreEvento, DateTime? fechaEvento, string? descripcion);
    Task BajaLogicaEventoSpAsync(int idEvento);
}

