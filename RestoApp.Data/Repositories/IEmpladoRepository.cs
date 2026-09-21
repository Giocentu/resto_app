using RestoApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Data.Repositories;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<IEnumerable<Empleado>> GetEmpleadosConDetallesAsync();
    Task<IEnumerable<Empleado>> GetEmpleadosSpAsync(bool soloActivos = true);
    Task CrearEmpleadoSpAsync(long dni, string nombre, string apellido, string email, long telefono, string password, int idRol, int idTurno);
    Task BajaLogicaEmpleadoSpAsync(long dniEmpleado, int idRol);
}