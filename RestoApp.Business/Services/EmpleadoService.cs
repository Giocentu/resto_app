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

    public async Task<IEnumerable<Empleado>> ObtenerEmpleadosAsync()
    {
        // Ahora pedimos los empleados con todas sus tablas relacionadas
        return await _empleadoRepository.GetEmpleadosConDetallesAsync();    
    }
}