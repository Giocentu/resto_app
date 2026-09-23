using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<Cliente>> GetClientesConPersonaAsync(bool soloActivos = true)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.PersonaInfo)
            .OrderBy(c => c.PersonaInfo != null ? c.PersonaInfo.Apellido : "")
            .ThenBy(c => c.PersonaInfo != null ? c.PersonaInfo.Nombre : "")
            .ToListAsync();
    }

    public async Task<Persona?> BuscarPersonaPorDniAsync(long dni)
    {
        return await _context.Personas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Dni == dni);
    }

    public async Task<Cliente?> BuscarClientePorDniAsync(long dni)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.PersonaInfo)
            .FirstOrDefaultAsync(c => c.DniCliente == dni);
    }

    public async Task CrearOActualizarClienteAsync(long dni, string nombre, string apellido, string email, long telefono)
    {
        var persona = await _context.Personas.FirstOrDefaultAsync(p => p.Dni == dni);
        if (persona != null)
        {
            persona.Nombre = nombre;
            persona.Apellido = apellido;
            persona.Email = email;
            persona.Telefono = telefono;
            _context.Personas.Update(persona);
        }
        else
        {
            persona = new Persona
            {
                Dni = dni,
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Telefono = telefono,
                Password = "password1"
            };
            _context.Personas.Add(persona);
        }

        await _context.SaveChangesAsync();

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.DniCliente == dni);
        if (cliente == null)
        {
            cliente = new Cliente
            {
                DniCliente = dni
            };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }
    }

    public async Task BajaLogicaClienteAsync(long dni)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.DniCliente == dni);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RestaurarClienteAsync(long dni)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.DniCliente == dni);
        if (cliente == null)
        {
            var persona = await _context.Personas.FirstOrDefaultAsync(p => p.Dni == dni);
            if (persona != null)
            {
                cliente = new Cliente
                {
                    DniCliente = dni,
                    PersonaInfo = persona
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }
        }
    }
}
