using RestoApp.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Data.Repositories;

public class EmpleadoRepository : Repository<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<Empleado>> GetEmpleadosConDetallesAsync()
    {
        return await _dbSet
            .Include(e => e.PersonaInfo) // Trae los datos de la Persona
            .Include(e => e.Rol)         // Trae los datos del RolEmpleado
            .ToListAsync();
    }    
}
