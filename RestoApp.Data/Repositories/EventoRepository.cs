using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public class EventoRepository : Repository<Evento>, IEventoRepository
{
    public EventoRepository(RestoAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Evento>> GetEventosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<Evento>> GetEventosSpAsync(bool soloActivos = true)
    {
        return await _dbSet
            .Where(e => !soloActivos || e.EsActivo)
            .OrderByDescending(e => e.FechaEvento)
            .ToListAsync();
    }

    public async Task<int> CrearEventoSpAsync(string nombreEvento, DateTime? fechaEvento, string? descripcion)
    {
        var nuevoIdParam = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var fechaParam = fechaEvento.HasValue ? (object)fechaEvento.Value : DBNull.Value;
        var descParam = !string.IsNullOrWhiteSpace(descripcion) ? (object)descripcion : DBNull.Value;

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Evento_Crear @NombreEvento = {0}, @FechaEvento = {1}, @Descripcion = {2}, @NuevoId = {3} OUTPUT",
            nombreEvento, fechaParam, descParam, nuevoIdParam);

        return (int)(nuevoIdParam.Value ?? 0);
    }

    public async Task BajaLogicaEventoSpAsync(int idEvento)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_Evento_BajaLogica @IdEvento = {0}", idEvento);
    }
}

