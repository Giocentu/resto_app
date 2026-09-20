using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public interface IEventoRepository : IRepository<Evento>
{
    Task<IEnumerable<Evento>> GetEventosAsync();
}
