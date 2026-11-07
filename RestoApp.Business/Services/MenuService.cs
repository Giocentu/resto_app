using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class MenuService
{
    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<IEnumerable<MenuItem>> ObtenerMenuAsync(bool soloActivos = true, int? idCategoria = null)
    {
        return await _menuRepository.ObtenerItemsSpAsync(soloActivos, idCategoria);
    }

    public async Task<IEnumerable<CategoriaMenu>> ObtenerCategoriasAsync()
    {
        return await _menuRepository.ObtenerCategoriasSpAsync();
    }

    public async Task<int> GuardarItemAsync(int idItem, string descripcion, double precio, int idCategoria, bool activo)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción del ítem no puede estar vacía.");
        if (precio <= 0)
            throw new ArgumentException("El precio debe ser un valor positivo mayor a 0.");

        if (idItem == 0)
        {
            return await _menuRepository.CrearItemSpAsync(descripcion, precio, idCategoria, activo);
        }
        else
        {
            await _menuRepository.EditarItemSpAsync(idItem, descripcion, precio, idCategoria, activo);
            return idItem;
        }
    }

    public async Task CambiarEstadoAsync(int idItem, bool activar)
    {
        await _menuRepository.CambiarEstadoSpAsync(idItem, activar);
    }
}

