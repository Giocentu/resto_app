using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class ClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<IEnumerable<Cliente>> ObtenerClientesAsync(bool soloActivos = true)
    {
        return await _clienteRepository.GetClientesConPersonaAsync(soloActivos);
    }

    public async Task<Persona?> BuscarPersonaPorDniAsync(long dni)
    {
        return await _clienteRepository.BuscarPersonaPorDniAsync(dni);
    }

    public async Task<Cliente?> BuscarClientePorDniAsync(long dni)
    {
        return await _clienteRepository.BuscarClientePorDniAsync(dni);
    }

    public async Task GuardarClienteAsync(long dni, string nombre, string apellido, string email, long telefono)
    {
        await _clienteRepository.CrearOActualizarClienteAsync(dni, nombre, apellido, email, telefono);
    }

    public async Task BajaLogicaClienteAsync(long dni)
    {
        await _clienteRepository.BajaLogicaClienteAsync(dni);
    }

    public async Task RestaurarClienteAsync(long dni)
    {
        await _clienteRepository.RestaurarClienteAsync(dni);
    }
}
