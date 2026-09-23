using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public interface IUbicacionRepository : IRepository<UbicacionMesa>
{
    Task<IEnumerable<UbicacionMesa>> GetUbicacionesAsync(bool soloActivas = true);
}
