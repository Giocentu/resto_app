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
        return await _mesaRepository.CrearMesaSpAsync(nroMesa, capacidad, idUbicacion, estado);
    }

    public async Task EditarMesaAsync(int idMesa, int nroMesa, int capacidad, int idUbicacion, string estado)
    {
        await _mesaRepository.EditarMesaSpAsync(idMesa, nroMesa, capacidad, idUbicacion, estado);
    }

    public async Task BajaLogicaMesaAsync(int idMesa)
    {
        await _mesaRepository.BajaLogicaMesaSpAsync(idMesa);
    }

    public async Task RestaurarMesaAsync(int idMesa)
    {
        await _mesaRepository.RestaurarMesaSpAsync(idMesa);
    }
}