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

    public async Task<Dictionary<int, CuentaPagoInfoDto>> ObtenerInfoCuentasPagosAsync()
    {
        return await _pagoRepository.ObtenerInfoCuentasPagosAsync();
    }

    public async Task<IEnumerable<MetodoPago>> ObtenerMetodosPagoAsync()
    {
        return await _pagoRepository.GetMetodosPagoAsync();
    }
    public Task<int> CrearCuentaAsync(int idMesa, long? dniEmpleado = null, int? idRol = null)
    {
        return _pagoRepository.CrearCuentaSpAsync(idMesa, dniEmpleado, idRol);
    }

    public Task<int> CrearCuentaConItemAsync(
        int idMesa,
        int idItem,
        int cantidad)
    {
        return _pagoRepository.CrearCuentaConItemSpAsync(
            idMesa,
            idItem,
            cantidad);
    }
    public Task AgregarItemACuentaAsync(
        int cuentaId,
        int idItem,
        int cantidad)
    {
        return _pagoRepository.AgregarItemACuentaSpAsync(
            cuentaId,
            idItem,
            cantidad);
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
    public Task<List<CuentaPendienteDto>> ObtenerCuentasPendientesAsync()
    {
        return _pagoRepository.ObtenerCuentasPendientesAsync();
    }

    public Task<int> CerrarCuentaAsync(int cuentaId, int idMetodo)
    {
        return _pagoRepository.CerrarCuentaAsync(cuentaId, idMetodo);
    }

    public Task CancelarCuentaAsync(int cuentaId)
    {
        return _pagoRepository.CancelarCuentaAsync(cuentaId);
    }

    public Task QuitarItemDeCuentaAsync(int cuentaId, int idItem, int cantidad = 1)
    {
        return _pagoRepository.QuitarItemDeCuentaSpAsync(cuentaId, idItem, cantidad);
    }

    public Task<List<CuentaItemDetalleDto>> ObtenerDetalleCuentaAsync(int cuentaId)
    {
        return _pagoRepository.ObtenerDetalleCuentaAsync(cuentaId);
    }

    public Task CambiarMozoCuentaAsync(int cuentaId, long dniEmpleado, int idRol)
    {
        return _pagoRepository.CambiarMozoCuentaAsync(cuentaId, dniEmpleado, idRol);
    }
}

