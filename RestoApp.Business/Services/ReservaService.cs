using System;
using RestoApp.Data.Repositories;
using RestoApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Business.Services;

public class ReservaService
{
    private readonly IReservaRepository _reservaRepository;

    public ReservaService(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    public async Task<IEnumerable<Reserva>> ObtenerReservasAsync()
    {
        try
        {
            return await _reservaRepository.GetReservasSpAsync();
        }
        catch
        {
            return await _reservaRepository.GetReservasConDetallesAsync();
        }
    }

    public async Task<int> CrearReservaAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null)
    {
        return await _reservaRepository.CrearReservaSpAsync(fechaReserva, cantPersonas, idEstado, dniCliente, idEvento, dniEmpleado, idRol, idMesa);
    }

    public async Task CambiarEstadoReservaAsync(int idReserva, int nuevoEstadoId)
    {
        await _reservaRepository.CambiarEstadoReservaSpAsync(idReserva, nuevoEstadoId);
    }
}