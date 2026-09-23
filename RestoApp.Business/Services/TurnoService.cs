using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class TurnoService
{
    private readonly ITurnoRepository _turnoRepository;

    public TurnoService(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    public async Task<IEnumerable<TurnoEmpleado>> ObtenerTurnosAsync(bool soloActivos = false)
    {
        return await _turnoRepository.GetTurnosAsync(soloActivos);
    }

    public async Task<TurnoEmpleado> CrearTurnoAsync(TimeSpan inicio, TimeSpan fin)
    {
        return await _turnoRepository.CrearTurnoAsync(inicio, fin);
    }

    public async Task CambiarEstadoTurnoAsync(int idTurno, bool nuevoEstado)
    {
        await _turnoRepository.CambiarEstadoTurnoAsync(idTurno, nuevoEstado);
    }
}
