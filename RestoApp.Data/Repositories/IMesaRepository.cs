using RestoApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Data.Repositories;

public interface IMesaRepository : IRepository<Mesa>
{
    Task<IEnumerable<Mesa>> GetMesasConUbicacionAsync();
    Task<IEnumerable<Mesa>> GetMesasSpAsync(bool soloActivas = true);
    Task<int> CrearMesaSpAsync(int nroMesa, int capacidad, int idUbicacion, string estado = "LIBRE");
    Task EditarMesaSpAsync(int idMesa, int nroMesa, int capacidad, int idUbicacion, string estado);
    Task BajaLogicaMesaSpAsync(int idMesa);
    Task RestaurarMesaSpAsync(int idMesa);
}