using RestoApp.Data.Repositories;
using RestoApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoApp.Business.Services;

public class MesaService
{
    private readonly IMesaRepository _mesaRepository;

    public MesaService(IMesaRepository mesaRepository)
    {
        _mesaRepository = mesaRepository;
    }

    public async Task<IEnumerable<Mesa>> ObtenerMesasAsync(bool soloActivas = true)
    {
        try
        {
            return await _mesaRepository.GetMesasSpAsync(soloActivas);
        }
        catch
        {
            return await _mesaRepository.GetMesasConUbicacionAsync();
        }
    }

    public async Task<int> CrearMesaAsync(int nroMesa, int capacidad, int idUbicacion, string estado = "LIBRE")
    {
        try
        {
            return await _mesaRepository.CrearMesaSpAsync(nroMesa, capacidad, idUbicacion, estado);
        }
        catch
        {
            var mesa = new Mesa
            {
                NroMesa = nroMesa,
                Capacidad = capacidad,
                IdUbicacion = idUbicacion,
                Estado = estado,
                EsActivo = true
            };
            await _mesaRepository.AddAsync(mesa);
            await _mesaRepository.SaveChangesAsync();
            return mesa.IdMesa;
        }
    }

    public async Task EditarMesaAsync(int idMesa, int nroMesa, int capacidad, int idUbicacion, string estado)
    {
        try
        {
            await _mesaRepository.EditarMesaSpAsync(idMesa, nroMesa, capacidad, idUbicacion, estado);
        }
        catch
        {
            var mesa = await _mesaRepository.GetByIdAsync(idMesa);
            if (mesa != null)
            {
                mesa.NroMesa = nroMesa;
                mesa.Capacidad = capacidad;
                mesa.IdUbicacion = idUbicacion;
                mesa.Estado = estado;
                _mesaRepository.Update(mesa);
                await _mesaRepository.SaveChangesAsync();
            }
        }
    }

    public async Task BajaLogicaMesaAsync(int idMesa)
    {
        try
        {
            await _mesaRepository.BajaLogicaMesaSpAsync(idMesa);
        }
        catch
        {
            var mesa = await _mesaRepository.GetByIdAsync(idMesa);
            if (mesa != null)
            {
                mesa.EsActivo = false;
                _mesaRepository.Update(mesa);
                await _mesaRepository.SaveChangesAsync();
            }
        }
    }

    public async Task RestaurarMesaAsync(int idMesa)
    {
        try
        {
            await _mesaRepository.RestaurarMesaSpAsync(idMesa);
        }
        catch
        {
            var mesa = await _mesaRepository.GetByIdAsync(idMesa);
            if (mesa != null)
            {
                mesa.EsActivo = true;
                _mesaRepository.Update(mesa);
                await _mesaRepository.SaveChangesAsync();
            }
        }
    }
}