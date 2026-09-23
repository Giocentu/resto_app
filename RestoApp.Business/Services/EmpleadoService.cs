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
        try
        {
            await _empleadoRepository.CrearEmpleadoSpAsync(dni, nombre, apellido, email, telefono, password, idRol, idTurno);
        }
        catch
        {
            var emp = new Empleado
            {
                DniEmpleado = dni,
                IdRol = idRol,
                IdTurno = idTurno > 0 ? idTurno : 1,
                ActivoEnRol = true
            };
            await _empleadoRepository.AddAsync(emp);
            await _empleadoRepository.SaveChangesAsync();
        }
    }

    public async Task EditarEmpleadoAsync(long dni, string nombre, string apellido, string email, long telefono, int idRol)
    {
        var emp = await _empleadoRepository.GetByIdAsync(dni);
        if (emp != null)
        {
            emp.IdRol = idRol;
            if (emp.PersonaInfo != null)
            {
                emp.PersonaInfo.Nombre = nombre;
                emp.PersonaInfo.Apellido = apellido;
                emp.PersonaInfo.Email = email;
                emp.PersonaInfo.Telefono = telefono;
            }
            _empleadoRepository.Update(emp);
            await _empleadoRepository.SaveChangesAsync();
        }
    }

    public async Task BajaLogicaEmpleadoAsync(long dniEmpleado, int idRol)
    {
        try
        {
            await _empleadoRepository.BajaLogicaEmpleadoSpAsync(dniEmpleado, idRol);
        }
        catch
        {
            var emp = await _empleadoRepository.GetByIdAsync(dniEmpleado);
            if (emp != null)
            {
                emp.ActivoEnRol = false;
                _empleadoRepository.Update(emp);
                await _empleadoRepository.SaveChangesAsync();
            }
        }
    }

    public async Task RestaurarEmpleadoAsync(long dniEmpleado, int idRol = 1)
    {
        var emp = await _empleadoRepository.GetByIdAsync(dniEmpleado);
        if (emp != null)
        {
            emp.ActivoEnRol = true;
            _empleadoRepository.Update(emp);
            await _empleadoRepository.SaveChangesAsync();
        }
    }
}