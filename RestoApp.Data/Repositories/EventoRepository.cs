using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class EventoRepository : Repository<Evento>, IEventoRepository
{
    public EventoRepository(RestoAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Evento>> GetEventosAsync()
    {
        return await _dbSet.ToListAsync();
    }
}
