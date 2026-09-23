using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class ReservaRepository : Repository<Reserva>, IReservaRepository
{
    public ReservaRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<Reserva>> GetReservasConDetallesAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Cliente)
                .ThenInclude(c => c!.PersonaInfo)
            .Include(r => r.Mesas)
            .Include(r => r.Evento)
            .Include(r => r.Estado)
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetReservasSpAsync()
    {
        return await GetReservasConDetallesAsync();
    }

    public async Task<long> ObtenerOCrearClientePorDniYNombreAsync(long dni, string nombreCliente)
    {
        if (dni <= 0)
        {
            return await ObtenerOCrearClientePorNombreAsync(nombreCliente);
        }

        if (string.IsNullOrWhiteSpace(nombreCliente))
            nombreCliente = "Cliente General";

        string nombreTrim = nombreCliente.Trim();
        string[] partes = nombreTrim.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string nombre = partes.Length > 0 ? partes[0] : nombreTrim;
        string apellido = partes.Length > 1 ? partes[1] : "Cliente";

        var persona = await _context.Personas.FirstOrDefaultAsync(p => p.Dni == dni);
        if (persona == null)
        {
            string cleanNombre = nombre.ToLower().Replace(" ", "");
            string cleanApellido = apellido.ToLower().Replace(" ", "");
            string emailGenerado = $"{cleanNombre}.{cleanApellido}.{dni}@restoapp.com";
            long telefonoGenerado = 3794000000L + (dni % 899999L);

            persona = new Persona
            {
                Dni = dni,
                Nombre = nombre,
                Apellido = apellido,
                Email = emailGenerado,
                Telefono = telefonoGenerado,
                Password = "password1"
            };
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }

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

        return dni;
    }

    public async Task<long> ObtenerOCrearClientePorNombreAsync(string nombreCliente)
    {
        if (string.IsNullOrWhiteSpace(nombreCliente))
            nombreCliente = "Cliente General";

        string nombreTrim = nombreCliente.Trim();

        var clientes = await _context.Clientes
            .Include(c => c.PersonaInfo)
            .ToListAsync();

        var clienteExistente = clientes.FirstOrDefault(c =>
            c.PersonaInfo != null &&
            (
                $"{c.PersonaInfo.Nombre} {c.PersonaInfo.Apellido}".Trim().Equals(nombreTrim, StringComparison.OrdinalIgnoreCase) ||
                c.PersonaInfo.Nombre.Trim().Equals(nombreTrim, StringComparison.OrdinalIgnoreCase)
            )
        );

        if (clienteExistente != null)
        {
            return clienteExistente.DniCliente;
        }

        string[] partes = nombreTrim.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string nombre = partes.Length > 0 ? partes[0] : nombreTrim;
        string apellido = partes.Length > 1 ? partes[1] : "Cliente";

        var rand = new Random();
        long dniGenerado;
        do
        {
            dniGenerado = rand.Next(10000000, 99999999);
        } while (await _context.Personas.AnyAsync(p => p.Dni == dniGenerado));

        string cleanNombre = nombre.ToLower().Replace(" ", "");
        string cleanApellido = apellido.ToLower().Replace(" ", "");
        string emailGenerado = $"{cleanNombre}.{cleanApellido}.{dniGenerado}@restoapp.com";
        long telefonoGenerado = 3794000000L + (dniGenerado % 899999L);

        while (await _context.Personas.AnyAsync(p => p.Telefono == telefonoGenerado || p.Email == emailGenerado))
        {
            telefonoGenerado++;
            emailGenerado = $"{cleanNombre}.{cleanApellido}.{telefonoGenerado}@restoapp.com";
        }

        var nuevaPersona = new Persona
        {
            Dni = dniGenerado,
            Nombre = nombre,
            Apellido = apellido,
            Email = emailGenerado,
            Telefono = telefonoGenerado,
            Password = "password1"
        };

        var nuevoCliente = new Cliente
        {
            DniCliente = dniGenerado,
            PersonaInfo = nuevaPersona
        };

        _context.Personas.Add(nuevaPersona);
        _context.Clientes.Add(nuevoCliente);
        await _context.SaveChangesAsync();

        return dniGenerado;
    }

    public async Task<int> CrearReservaEfAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null)
    {
        var res = new Reserva
        {
            FechaReserva = fechaReserva,
            CantPersonas = cantPersonas,
            IdEstado = idEstado > 0 ? idEstado : 1,
            DniCliente = dniCliente,
            IdEvento = idEvento,
            DniEmpleado = dniEmpleado,
            IdRol = idRol
        };

        if (idMesa.HasValue && idMesa.Value > 0)
        {
            var mesaEntity = await _context.Mesas.FindAsync(idMesa.Value);
            if (mesaEntity != null)
            {
                res.Mesas.Add(mesaEntity);
            }
        }

        await AddAsync(res);
        await SaveChangesAsync();
        return res.IdReserva;
    }

    public async Task<int> CrearReservaSpAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null)
    {
        var pFecha = new SqlParameter("@FechaReserva", SqlDbType.DateTime) { Value = fechaReserva };
        var pCant = new SqlParameter("@CantPersonas", SqlDbType.Int) { Value = cantPersonas };
        var pEstado = new SqlParameter("@IdEstado", SqlDbType.Int) { Value = idEstado };
        var pCliente = new SqlParameter("@DniCliente", SqlDbType.BigInt) { Value = dniCliente };
        var pEvento = new SqlParameter("@IdEvento", SqlDbType.Int) { Value = (object?)idEvento ?? DBNull.Value };
        var pEmpleado = new SqlParameter("@DniEmpleado", SqlDbType.BigInt) { Value = (object?)dniEmpleado ?? DBNull.Value };
        var pRol = new SqlParameter("@IdRol", SqlDbType.Int) { Value = (object?)idRol ?? DBNull.Value };
        var pMesa = new SqlParameter("@IdMesa", SqlDbType.Int) { Value = (object?)idMesa ?? DBNull.Value };
        var pOutput = new SqlParameter("@NuevaReservaId", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Reserva_Crear @FechaReserva = {0}, @CantPersonas = {1}, @IdEstado = {2}, @DniCliente = {3}, @IdEvento = {4}, @DniEmpleado = {5}, @IdRol = {6}, @IdMesa = {7}, @NuevaReservaId = {8} OUTPUT",
            pFecha, pCant, pEstado, pCliente, pEvento, pEmpleado, pRol, pMesa, pOutput);

        return (int)(pOutput.Value != DBNull.Value ? pOutput.Value : 0);
    }

    public async Task CambiarEstadoReservaSpAsync(int idReserva, int nuevoEstadoId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Reserva_CambiarEstado @IdReserva = {0}, @NuevoEstadoId = {1}",
            idReserva, nuevoEstadoId);
    }
}