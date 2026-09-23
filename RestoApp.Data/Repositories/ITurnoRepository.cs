using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public interface ITurnoRepository : IRepository<TurnoEmpleado>
{
    Task<IEnumerable<TurnoEmpleado>> GetTurnosAsync(bool soloActivos = false);
    Task<TurnoEmpleado> CrearTurnoAsync(TimeSpan inicio, TimeSpan fin);
    Task CambiarEstadoTurnoAsync(int idTurno, bool nuevoEstado);
}
