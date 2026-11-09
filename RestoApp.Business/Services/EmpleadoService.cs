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
        var empleados = await _empleadoRepository.GetEmpleadosSpAsync(soloActivos: false);
        var emp = System.Linq.Enumerable.FirstOrDefault(empleados, e => e.DniEmpleado == dni);
        if (emp != null)
        {
            if (emp.PersonaInfo != null)
            {
                emp.PersonaInfo.Nombre = nombre;
                emp.PersonaInfo.Apellido = apellido;
                emp.PersonaInfo.Email = email;
                emp.PersonaInfo.Telefono = telefono;
            }
            if (emp.IdRol != idRol)
            {
                await _empleadoRepository.BajaLogicaEmpleadoSpAsync(dni, emp.IdRol);
                emp.IdRol = idRol;
            }
            _empleadoRepository.Update(emp);
            await _empleadoRepository.SaveChangesAsync();
        }
    }

    public async Task BajaLogicaEmpleadoAsync(long dniEmpleado, int idRol)
    {
        await _empleadoRepository.BajaLogicaEmpleadoSpAsync(dniEmpleado, idRol);
    }

    public async Task RestaurarEmpleadoAsync(long dniEmpleado, int idRol)
    {
        await _empleadoRepository.RestaurarEmpleadoSpAsync(dniEmpleado, idRol);
    }

    public async Task<List<UsuarioAutorizanteItem>> ObtenerUsuariosAutorizantesAsync()
    {
        var lista = new List<UsuarioAutorizanteItem>();
        try
        {
            var empleados = await ObtenerEmpleadosAsync(soloActivos: true);
            foreach (var emp in empleados)
            {
                var rolDesc = emp.Rol?.Descripcion ?? (emp.IdRol == 1 ? "Dueño" : emp.IdRol == 2 ? "Gerente" : string.Empty);
                bool esAutorizado = emp.IdRol == 1 || emp.IdRol == 2 ||
                                    rolDesc.Equals("Dueño", StringComparison.OrdinalIgnoreCase) ||
                                    rolDesc.Equals("Dueno", StringComparison.OrdinalIgnoreCase) ||
                                    rolDesc.Equals("Gerente", StringComparison.OrdinalIgnoreCase) ||
                                    (emp.Rol?.PermisoAdmin == true);

                if (esAutorizado)
                {
                    var nombre = $"{emp.PersonaInfo?.Nombre} {emp.PersonaInfo?.Apellido}".Trim();
                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        nombre = $"Empleado #{emp.DniEmpleado}";
                    }

                    lista.Add(new UsuarioAutorizanteItem
                    {
                        Id = emp.DniEmpleado.ToString(),
                        NombreCompleto = nombre,
                        Rol = string.IsNullOrWhiteSpace(rolDesc) ? "Gerente" : rolDesc,
                        PasswordEsperada = emp.PersonaInfo?.Password ?? string.Empty
                    });
                }
            }
        }
        catch
        {
            // Fallback si la BD no está disponible
        }

        // Siempre garantizamos la presencia de usuarios Dueño y Gerente para pruebas o entornos base
        if (!System.Linq.Enumerable.Any(lista, u => u.Id == "96079673" || u.Id == "demo_dueno"))
        {
            lista.Add(new UsuarioAutorizanteItem
            {
                Id = "96079673",
                NombreCompleto = "Giovanni Centurion",
                Rol = "Dueño",
                PasswordEsperada = "123123"
            });
        }

        if (!System.Linq.Enumerable.Any(lista, u => u.Id == "42739764" || u.Id == "demo_gerente"))
        {
            lista.Add(new UsuarioAutorizanteItem
            {
                Id = "42739764",
                NombreCompleto = "Hernan Cespedes",
                Rol = "Gerente",
                PasswordEsperada = "123123"
            });
        }

        return lista;
    }
}