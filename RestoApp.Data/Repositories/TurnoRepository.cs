using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class TurnoRepository : Repository<TurnoEmpleado>, ITurnoRepository
{
    private static bool _columnaVerificada = false;

    public TurnoRepository(RestoAppDbContext context) : base(context)
    {
        AsegurarColumnaEsActivo();
    }

    private void AsegurarColumnaEsActivo()
    {
        if (_columnaVerificada) return;
        try
        {
            _context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.turno_empleado') AND name = 'es_activo')
                BEGIN
                    ALTER TABLE dbo.turno_empleado ADD es_activo BIT NOT NULL DEFAULT 1;
                END");
            _columnaVerificada = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TURNO REPOSITORY MIGRATION INFO] {ex.Message}");
        }
    }

    public async Task<IEnumerable<TurnoEmpleado>> GetTurnosAsync(bool soloActivos = false)
    {
        IQueryable<TurnoEmpleado> query = _dbSet.AsNoTracking();
        if (soloActivos)
        {
            query = query.Where(t => t.EsActivo);
        }
        return await query
            .OrderBy(t => t.InicioTurno)
            .ToListAsync();
    }

    public async Task<TurnoEmpleado> CrearTurnoAsync(TimeSpan inicio, TimeSpan fin)
    {
        var nuevoTurno = new TurnoEmpleado
        {
            InicioTurno = inicio,
            FinTurno = fin,
            EsActivo = true
        };

        await AddAsync(nuevoTurno);
        await SaveChangesAsync();
        return nuevoTurno;
    }

    public async Task CambiarEstadoTurnoAsync(int idTurno, bool nuevoEstado)
    {
        var turno = await _context.TurnosEmpleado.FindAsync(idTurno);
        if (turno != null)
        {
            turno.EsActivo = nuevoEstado;
            _context.TurnosEmpleado.Update(turno);
            await _context.SaveChangesAsync();
        }
    }
}
