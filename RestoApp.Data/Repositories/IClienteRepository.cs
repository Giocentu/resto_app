using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<IEnumerable<Cliente>> GetClientesConPersonaAsync(bool soloActivos = true);
    Task<Persona?> BuscarPersonaPorDniAsync(long dni);
    Task<Cliente?> BuscarClientePorDniAsync(long dni);
    Task CrearOActualizarClienteAsync(long dni, string nombre, string apellido, string email, long telefono);
    Task BajaLogicaClienteAsync(long dni);
    Task RestaurarClienteAsync(long dni);
}
