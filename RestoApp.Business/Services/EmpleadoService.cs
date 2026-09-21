using RestoApp.Data.Repositories;
using RestoApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Business.Services;

public class EmpleadoService
{
    private readonly IEmpleadoRepository _empleadoRepository;

    public EmpleadoService(IEmpleadoRepository empleadoRepository)
    {
        _empleadoRepository = empleadoRepository;
    }

    public async Task<IEnumerable<Empleado>> ObtenerEmpleadosAsync(bool soloActivos = true)
    {
        try
        {
            return await _empleadoRepository.GetEmpleadosSpAsync(soloActivos);
        }
        catch
        {
            return await _empleadoRepository.GetEmpleadosConDetallesAsync();
        }
    }

    public async Task CrearEmpleadoAsync(long dni, string nombre, string apellido, string email, long telefono, string password, int idRol, int idTurno)
    {
        await _empleadoRepository.CrearEmpleadoSpAsync(dni, nombre, apellido, email, telefono, password, idRol, idTurno);
    }

    public async Task BajaLogicaEmpleadoAsync(long dniEmpleado, int idRol)
    {
        await _empleadoRepository.BajaLogicaEmpleadoSpAsync(dniEmpleado, idRol);
    }
}