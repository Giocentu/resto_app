using System;
using System.Collections.Generic;
using System.Data;
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
            .Include(r => r.Cliente)         // Trae el Cliente
                .ThenInclude(c => c.PersonaInfo) // Trae los datos de la Persona (Nombre, DNI)
            .Include(r => r.Mesas)           // Trae la lista de Mesas asignadas
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetReservasSpAsync()
    {
        return await _dbSet
            .FromSqlRaw("EXEC sp_Reserva_ObtenerTodas")
            .Include(r => r.Cliente)
                .ThenInclude(c => c!.PersonaInfo)
            .Include(r => r.Mesas)
            .Include(r => r.Evento)
            .Include(r => r.Estado)
            .ToListAsync();
    }


    public async Task<int> CrearReservaSpAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null)
    {
        var nuevaReservaIdParam = new SqlParameter("@NuevaReservaId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        
        var idEventoParam = idEvento.HasValue ? (object)idEvento.Value : DBNull.Value;
        var dniEmpleadoParam = dniEmpleado.HasValue ? (object)dniEmpleado.Value : DBNull.Value;
        var idRolParam = idRol.HasValue ? (object)idRol.Value : DBNull.Value;
        var idMesaParam = idMesa.HasValue ? (object)idMesa.Value : DBNull.Value;

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Reserva_Crear @FechaReserva = {0}, @CantPersonas = {1}, @IdEstado = {2}, @DniCliente = {3}, @IdEvento = {4}, @DniEmpleado = {5}, @IdRol = {6}, @IdMesa = {7}, @NuevaReservaId = @NuevaReservaId OUTPUT",
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