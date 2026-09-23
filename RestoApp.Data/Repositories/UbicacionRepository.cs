using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class UbicacionRepository : Repository<UbicacionMesa>, IUbicacionRepository
{
    public UbicacionRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<UbicacionMesa>> GetUbicacionesAsync(bool soloActivas = true)
    {
        return await _dbSet
            .OrderBy(u => u.Ubicacion)
            .ToListAsync();
    }
}
