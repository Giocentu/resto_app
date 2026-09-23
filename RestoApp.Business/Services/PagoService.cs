using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class PagoService
{
    private readonly IPagoRepository _pagoRepository;

    public PagoService(IPagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    public async Task<IEnumerable<Pago>> ObtenerPagosAsync()
    {
        return await _pagoRepository.GetPagosConDetallesAsync();
    }

    public async Task<IEnumerable<MetodoPago>> ObtenerMetodosPagoAsync()
    {
        return await _pagoRepository.GetMetodosPagoAsync();
    }

    public async Task<int> RegistrarPagoAsync(Pago pago, int? idMesa = null)
    {
        try
        {
            return await _pagoRepository.CrearPagoSpAsync((decimal)pago.Monto, pago.FechaPago, pago.IdMetodo, pago.IdReserva, idMesa);
        }
        catch
        {
            await _pagoRepository.AddAsync(pago);
            await _pagoRepository.SaveChangesAsync();
            return pago.IdPago;
        }
    }

    public async Task EliminarPagoAsync(int idPago)
    {
        var pago = await _pagoRepository.GetByIdAsync(idPago);
        if (pago != null)
        {
            _pagoRepository.Delete(pago);
            await _pagoRepository.SaveChangesAsync();
        }
    }
}

