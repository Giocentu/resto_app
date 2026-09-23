using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class UbicacionService
{
    private readonly IUbicacionRepository _ubicacionRepository;
    private static readonly HashSet<int> _ubicacionesArchivadasIds = new();

    public UbicacionService(IUbicacionRepository ubicacionRepository)
    {
        _ubicacionRepository = ubicacionRepository;
    }

    public async Task<IEnumerable<UbicacionMesa>> ObtenerUbicacionesAsync(bool soloActivas = true)
    {
        var lista = await _ubicacionRepository.GetUbicacionesAsync(soloActivas: false);
        foreach (var item in lista)
        {
            item.EsActivo = !_ubicacionesArchivadasIds.Contains(item.IdUbicacion);
        }

        if (soloActivas)
        {
            return lista.Where(u => u.EsActivo);
        }
        else
        {
            return lista.Where(u => !u.EsActivo);
        }
    }

    public async Task<int> CrearUbicacionAsync(string nombreUbicacion)
    {
        var entidad = new UbicacionMesa
        {
            Ubicacion = nombreUbicacion.Trim(),
            EsActivo = true
        };
        await _ubicacionRepository.AddAsync(entidad);
        await _ubicacionRepository.SaveChangesAsync();
        return entidad.IdUbicacion;
    }

    public async Task EditarUbicacionAsync(int idUbicacion, string nombreUbicacion)
    {
        var u = await _ubicacionRepository.GetByIdAsync(idUbicacion);
        if (u != null)
        {
            u.Ubicacion = nombreUbicacion.Trim();
            _ubicacionRepository.Update(u);
            await _ubicacionRepository.SaveChangesAsync();
        }
    }

    public async Task BajaLogicaUbicacionAsync(int idUbicacion)
    {
        _ubicacionesArchivadasIds.Add(idUbicacion);
        await Task.CompletedTask;
    }

    public async Task RestaurarUbicacionAsync(int idUbicacion)
    {
        _ubicacionesArchivadasIds.Remove(idUbicacion);
        await Task.CompletedTask;
    }
}
