using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public interface IPagoRepository : IRepository<Pago>
{
    Task<IEnumerable<Pago>> GetPagosConDetallesAsync();
    Task<IEnumerable<MetodoPago>> GetMetodosPagoAsync();
    Task<int> CrearPagoSpAsync(decimal monto, DateTime fechaPago, int idMetodo, int? idReserva = null, int? idMesa = null);

}

