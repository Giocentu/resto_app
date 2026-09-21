using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public class MesaRepository : Repository<Mesa>, IMesaRepository
{
    public MesaRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<Mesa>> GetMesasConUbicacionAsync()
    {
        return await _dbSet
            .Include(m => m.Ubicacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mesa>> GetMesasSpAsync(bool soloActivas = true)
    {
        return await _dbSet
            .FromSqlRaw("EXEC sp_Mesa_ObtenerTodas @SoloActivas = {0}", soloActivas)
            .Include(m => m.Ubicacion)
            .ToListAsync();
    }

    public async Task<int> CrearMesaSpAsync(int nroMesa, int capacidad, int idUbicacion, string estado = "LIBRE")
    {
        var nuevoIdParam = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Mesa_Crear @NroMesa = {0}, @Capacidad = {1}, @IdUbicacion = {2}, @Estado = {3}, @NuevoId = @NuevoId OUTPUT",
            nroMesa, capacidad, idUbicacion, estado, nuevoIdParam);
        return (int)(nuevoIdParam.Value ?? 0);
    }

    public async Task EditarMesaSpAsync(int idMesa, int nroMesa, int capacidad, int idUbicacion, string estado)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Mesa_Editar @IdMesa = {0}, @NroMesa = {1}, @Capacidad = {2}, @IdUbicacion = {3}, @Estado = {4}",
            idMesa, nroMesa, capacidad, idUbicacion, estado);
    }

    public async Task BajaLogicaMesaSpAsync(int idMesa)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_Mesa_BajaLogica @IdMesa = {0}", idMesa);
    }

    public async Task RestaurarMesaSpAsync(int idMesa)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_Mesa_Restaurar @IdMesa = {0}", idMesa);
    }
}