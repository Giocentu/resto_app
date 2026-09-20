using RestoApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Data.Repositories;

public interface IReservaRepository
{
    Task<IEnumerable<Reserva>> GetReservasConDetallesAsync();
}