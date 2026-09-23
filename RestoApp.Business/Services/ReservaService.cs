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
        return await _reservaRepository.GetReservasConDetallesAsync();
    }

    public async Task<int> CrearReservaAsync(DateTime fechaReserva, int cantPersonas, int idEstado, long dniCliente, int? idEvento = null, long? dniEmpleado = null, int? idRol = null, int? idMesa = null)
    {
        try
        {
            return await _reservaRepository.CrearReservaSpAsync(fechaReserva, cantPersonas, idEstado, dniCliente, idEvento, dniEmpleado, idRol, idMesa);
        }
        catch
        {
            var res = new Reserva
            {
                FechaReserva = fechaReserva,
                CantPersonas = cantPersonas,
                IdEstado = idEstado > 0 ? idEstado : 1,
                DniCliente = dniCliente > 0 ? dniCliente : 46452703, // Fallback a un cliente registrado
                IdEvento = idEvento,
                DniEmpleado = dniEmpleado,
                IdRol = idRol
            };
            await _reservaRepository.AddAsync(res);
            await _reservaRepository.SaveChangesAsync();
            return res.IdReserva;
        }
    }

    public async Task CambiarEstadoReservaAsync(int idReserva, int nuevoEstadoId)
    {
        try
        {
            await _reservaRepository.CambiarEstadoReservaSpAsync(idReserva, nuevoEstadoId);
        }
        catch
        {
            var res = await _reservaRepository.GetByIdAsync(idReserva);
            if (res != null)
            {
                res.IdEstado = nuevoEstadoId;
                _reservaRepository.Update(res);
                await _reservaRepository.SaveChangesAsync();
            }
        }
    }

    public async Task EditarReservaAsync(int idReserva, DateTime fechaReserva, int cantPersonas, int idEstado)
    {
        var res = await _reservaRepository.GetByIdAsync(idReserva);
        if (res != null)
        {
            res.FechaReserva = fechaReserva;
            res.CantPersonas = cantPersonas;
            res.IdEstado = idEstado;
            _reservaRepository.Update(res);
            await _reservaRepository.SaveChangesAsync();
        }
    }
}