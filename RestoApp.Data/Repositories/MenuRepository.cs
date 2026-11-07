using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class MenuRepository : Repository<MenuItem>, IMenuRepository
{
    public MenuRepository(RestoAppDbContext context) : base(context) { }

    public async Task<IEnumerable<MenuItem>> ObtenerItemsSpAsync(bool soloActivos = true, int? idCategoria = null)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Categoria)
            .Where(m => (!soloActivos || m.Activo) && (!idCategoria.HasValue || m.IdCategoria == idCategoria.Value))
            .OrderBy(m => m.Categoria != null ? m.Categoria.Nombre : string.Empty)
            .ThenBy(m => m.Descripcion)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoriaMenu>> ObtenerCategoriasSpAsync()
    {
        return await _context.CategoriasMenu
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<int> CrearItemSpAsync(string descripcion, double precio, int idCategoria, bool activo = true)
    {
        var nuevoIdParam = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Menu_Crear @Descripcion = {0}, @Precio = {1}, @IdCategoria = {2}, @Activo = {3}, @NuevoId = {4} OUTPUT",
            descripcion, precio, idCategoria, activo, nuevoIdParam);
        return (int)(nuevoIdParam.Value ?? 0);
    }

    public async Task EditarItemSpAsync(int idItem, string descripcion, double precio, int idCategoria, bool activo)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Menu_Editar @IdItem = {0}, @Descripcion = {1}, @Precio = {2}, @IdCategoria = {3}, @Activo = {4}",
            idItem, descripcion, precio, idCategoria, activo);
    }

    public async Task CambiarEstadoSpAsync(int idItem, bool activar)
    {
        if (activar)
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_Menu_Restaurar @IdItem = {0}", idItem);
        }
        else
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_Menu_BajaLogica @IdItem = {0}", idItem);
        }
    }
}
