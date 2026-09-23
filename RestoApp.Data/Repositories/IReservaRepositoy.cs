using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public interface IReservaRepository : IRepository<Reserva>
{
    Task<IEnumerable<Reserva>> GetReservasConDetallesAsync();
    Task<IEnumerable<Reserva>> GetReservasSpAsync();
    Task<long> ObtenerOCrearClientePorNombreAsync(string nombreCliente);
    Task<int> CrearReservaEfAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null);
    Task<int> CrearReservaSpAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null);
    Task CambiarEstadoReservaSpAsync(int idReserva, int nuevoEstadoId);
}