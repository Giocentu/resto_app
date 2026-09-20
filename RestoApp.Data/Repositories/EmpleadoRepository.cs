using RestoApp.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Data.Repositories;

public class EmpleadoRepository : Repository<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<Empleado>> GetEmpleadosActivosConDetallesAsync()
    {
        return await _dbSet
            .Include(e => e.PersonaInfo)
            .Include(e => e.Rol)
            .Include(e => e.Turno)
            .Where(e => e.ActivoEnRol)
            .Include(e => e.PersonaInfo) // Trae los datos personales (Nombre, Apellido, etc.)
            // .Include(e => e.Rol)      // Descomenta o ajusta esto si el Rol está en otra tabla
            .ToListAsync();
    }    
}
