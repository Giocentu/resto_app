using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

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

    public async Task<IEnumerable<Empleado>> GetEmpleadosSpAsync(bool soloActivos = true)
    {
        return await _dbSet
            .FromSqlRaw("EXEC sp_Empleado_ObtenerTodos @SoloActivos = {0}", soloActivos)
            .Include(e => e.PersonaInfo)
            .Include(e => e.Rol)
            .Include(e => e.Turno)
            .ToListAsync();
    }

    public async Task CrearEmpleadoSpAsync(long dni, string nombre, string apellido, string email, long telefono, string password, int idRol, int idTurno)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Empleado_Crear @Dni = {0}, @Nombre = {1}, @Apellido = {2}, @Email = {3}, @Telefono = {4}, @Password = {5}, @IdRol = {6}, @IdTurno = {7}",
            dni, nombre, apellido, email, telefono, password ?? "123456", idRol, idTurno);
    }

    public async Task BajaLogicaEmpleadoSpAsync(long dniEmpleado, int idRol)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Empleado_BajaLogica @DniEmpleado = {0}, @IdRol = {1}",
            dniEmpleado, idRol);
    }
}

