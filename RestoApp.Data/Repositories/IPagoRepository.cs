using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public interface IPagoRepository : IRepository<Pago>
{
    Task<IEnumerable<Pago>> GetPagosConDetallesAsync();
    Task<IEnumerable<MetodoPago>> GetMetodosPagoAsync();
    Task<List<CuentaPendienteDto>> ObtenerCuentasPendientesAsync();
    Task<int> CerrarCuentaAsync(int cuentaId, int idMetodo);
    Task<int> CrearPagoSpAsync(decimal monto, DateTime fechaPago, int idMetodo, int? idReserva = null, int? idMesa = null);

    Task<int> CrearCuentaConItemSpAsync(int idMesa, int idItem, int cantidad);
    Task AgregarItemACuentaSpAsync(int cuentaId, int idItem, int cantidad);
}

