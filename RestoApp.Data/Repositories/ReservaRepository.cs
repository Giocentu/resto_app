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
                c.PersonaInfo.Nombre.Equals(nombreTrim, StringComparison.OrdinalIgnoreCase)
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

        var nuevaPersona = new Persona
        {
            Dni = dniGenerado,
            Nombre = nombre,
            Apellido = apellido,
            Email = $"{nombre.ToLower().Replace(" ", "")}@cliente.com",
            Telefono = 3794000000 + rand.Next(100000, 999999),
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
        var nuevaReservaIdParam = new SqlParameter("@NuevaReservaId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        
        var idEventoParam = idEvento.HasValue ? (object)idEvento.Value : DBNull.Value;
        var dniEmpleadoParam = dniEmpleado.HasValue ? (object)dniEmpleado.Value : DBNull.Value;
        var idRolParam = idRol.HasValue ? (object)idRol.Value : DBNull.Value;
        var idMesaParam = idMesa.HasValue ? (object)idMesa.Value : DBNull.Value;

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Reserva_Crear @FechaReserva = {0}, @CantPersonas = {1}, @IdEstado = {2}, @DniCliente = {3}, @IdEvento = {4}, @DniEmpleado = {5}, @IdRol = {6}, @IdMesa = {7}, @NuevaReservaId = {8} OUTPUT",
            fechaReserva, cantPersonas, idEstado, dniCliente, idEventoParam, dniEmpleadoParam, idRolParam, idMesaParam, nuevaReservaIdParam);

        return (int)(nuevaReservaIdParam.Value ?? 0);
    }

    public async Task CambiarEstadoReservaSpAsync(int idReserva, int nuevoEstadoId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Reserva_CambiarEstado @IdReserva = {0}, @NuevoEstadoId = {1}",
            idReserva, nuevoEstadoId);
    }
}