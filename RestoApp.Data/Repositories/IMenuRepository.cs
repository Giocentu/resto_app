using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public interface IMenuRepository : IRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> ObtenerItemsSpAsync(bool soloActivos = true, int? idCategoria = null);
    Task<IEnumerable<CategoriaMenu>> ObtenerCategoriasSpAsync();
    Task<int> CrearItemSpAsync(string descripcion, double precio, int idCategoria, bool activo = true);
    Task EditarItemSpAsync(int idItem, string descripcion, double precio, int idCategoria, bool activo);
    Task CambiarEstadoSpAsync(int idItem, bool activar);
}

