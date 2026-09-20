using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class PagoRepository : Repository<Pago>, IPagoRepository
{
    public PagoRepository(RestoAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pago>> GetPagosConDetallesAsync()
    {
        return await _dbSet
            .Include(p => p.MetodoPago)
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Cliente)
                    .ThenInclude(c => c!.PersonaInfo)
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Mesas)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();
    }

    public async Task<IEnumerable<MetodoPago>> GetMetodosPagoAsync()
    {
        return await _context.MetodosPago.ToListAsync();
    }
}
