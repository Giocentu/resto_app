using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly RestoAppDbContext _context;

    public ReporteRepository(RestoAppDbContext context)
    {
        _context = context;
    }

    public async Task<ReporteKpiDto> ObtenerKpisAsync(DateTime inicio, DateTime fin)
    {
        var kpis = new ReporteKpiDto();
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                ISNULL((SELECT SUM(monto) FROM pagos WHERE fecha_pago BETWEEN CAST(@FechaInicio AS DATE) AND CAST(@FechaFin AS DATE)), 0) AS TotalVentas,
                ISNULL((SELECT COUNT(cuenta_id) FROM cuenta_head WHERE estado = 'CERRADA' AND fecha_apertura BETWEEN @FechaInicio AND @FechaFin), 0) AS CuentasCerradas,
                ISNULL((SELECT AVG(total) FROM cuenta_head WHERE estado = 'CERRADA' AND fecha_apertura BETWEEN @FechaInicio AND @FechaFin), 0) AS TicketPromedio,
                ISNULL((SELECT COUNT(id_reserva) FROM reserva WHERE fecha_reserva BETWEEN CAST(@FechaInicio AS DATE) AND CAST(@FechaFin AS DATE)), 0) AS TotalReservas,
                ISNULL((
                    SELECT SUM(cb.cantidad) 
                    FROM cuenta_body cb 
                    INNER JOIN cuenta_head ch ON cb.cuenta_id = ch.cuenta_id 
                    WHERE ch.estado = 'CERRADA' AND ch.fecha_apertura BETWEEN @FechaInicio AND @FechaFin
                ), 0) AS TotalItemsVendidos;";
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add(new SqlParameter("@FechaInicio", inicio));
        cmd.Parameters.Add(new SqlParameter("@FechaFin", fin));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            kpis.TotalVentas = Convert.ToDouble(reader["TotalVentas"] != DBNull.Value ? reader["TotalVentas"] : 0);
            kpis.CuentasCerradas = Convert.ToInt32(reader["CuentasCerradas"] != DBNull.Value ? reader["CuentasCerradas"] : 0);
            kpis.TicketPromedio = Convert.ToDouble(reader["TicketPromedio"] != DBNull.Value ? reader["TicketPromedio"] : 0);
            kpis.TotalReservas = Convert.ToInt32(reader["TotalReservas"] != DBNull.Value ? reader["TotalReservas"] : 0);
            kpis.TotalItemsVendidos = Convert.ToInt32(reader["TotalItemsVendidos"] != DBNull.Value ? reader["TotalItemsVendidos"] : 0);
        }
        return kpis;
    }

    public async Task<IEnumerable<ReporteTopPlatoDto>> ObtenerTopPlatosAsync(DateTime inicio, DateTime fin, int top = 10)
    {
        var lista = new List<ReporteTopPlatoDto>();
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT TOP ({top})
                m.descripcion AS Descripcion,
                cm.nombre AS Categoria,
                SUM(cb.cantidad) AS CantidadVendida,
                SUM(cb.cantidad * cb.precio_unitario) AS TotalRecaudado
            FROM cuenta_body cb
            INNER JOIN cuenta_head ch ON cb.cuenta_id = ch.cuenta_id
            INNER JOIN menu m ON cb.id_item = m.id_item
            INNER JOIN categoria_menu cm ON m.id_categoria = cm.id_categoria
            WHERE ch.estado = 'CERRADA' 
              AND ch.fecha_apertura BETWEEN @FechaInicio AND @FechaFin
            GROUP BY m.id_item, m.descripcion, cm.nombre
            ORDER BY CantidadVendida DESC;";
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add(new SqlParameter("@FechaInicio", inicio));
        cmd.Parameters.Add(new SqlParameter("@FechaFin", fin));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new ReporteTopPlatoDto
            {
                Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                Categoria = reader["Categoria"]?.ToString() ?? string.Empty,
                CantidadVendida = Convert.ToInt32(reader["CantidadVendida"]),
                TotalRecaudado = Convert.ToDouble(reader["TotalRecaudado"])
            });
        }
        return lista;
    }

    public async Task<IEnumerable<ReporteMetodoPagoDto>> ObtenerVentasMetodoPagoAsync(DateTime inicio, DateTime fin)
    {
        var lista = new List<ReporteMetodoPagoDto>();
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                mp.forma_pago AS MetodoPago,
                ISNULL(SUM(p.monto), 0) AS TotalCobrado,
                COUNT(p.id_pago) AS CantidadOperaciones
            FROM metodo_pago mp
            LEFT JOIN pagos p ON mp.id_metodo = p.id_metodo 
                             AND p.fecha_pago BETWEEN @FechaInicio AND @FechaFin
            GROUP BY mp.id_metodo, mp.forma_pago
            ORDER BY TotalCobrado DESC;";
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add(new SqlParameter("@FechaInicio", inicio.Date));
        cmd.Parameters.Add(new SqlParameter("@FechaFin", fin.Date));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new ReporteMetodoPagoDto
            {
                MetodoPago = reader["MetodoPago"]?.ToString() ?? string.Empty,
                TotalCobrado = Convert.ToDouble(reader["TotalCobrado"]),
                CantidadOperaciones = Convert.ToInt32(reader["CantidadOperaciones"])
            });
        }

        double totalGlobal = lista.Sum(x => x.TotalCobrado);
        if (totalGlobal > 0)
        {
            foreach (var item in lista)
            {
                item.PorcentajeDelTotal = (item.TotalCobrado / totalGlobal) * 100.0;
            }
        }

        return lista;
    }

    public async Task<IEnumerable<ReporteMozoDto>> ObtenerRendimientoMozosAsync(DateTime inicio, DateTime fin)
    {
        var lista = new List<ReporteMozoDto>();
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                p.nombre + ' ' + p.apellido AS NombreEmpleado,
                re.descripcion AS Puesto,
                COUNT(ch.cuenta_id) AS MesasAtendidas,
                ISNULL(SUM(ch.total), 0) AS FacturacionTotal,
                ISNULL(AVG(ch.total), 0) AS PromedioPorMesa
            FROM cuenta_head ch
            INNER JOIN empleado e ON ch.dni_empleado = e.dni_empleado AND ch.id_rol = e.id_rol
            INNER JOIN persona p ON e.dni_empleado = p.dni
            INNER JOIN rol_empleado re ON e.id_rol = re.id_rol
            WHERE ch.estado = 'CERRADA'
              AND ch.fecha_apertura BETWEEN @FechaInicio AND @FechaFin
            GROUP BY p.nombre, p.apellido, re.descripcion
            ORDER BY FacturacionTotal DESC;";
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add(new SqlParameter("@FechaInicio", inicio));
        cmd.Parameters.Add(new SqlParameter("@FechaFin", fin));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new ReporteMozoDto
            {
                NombreEmpleado = reader["NombreEmpleado"]?.ToString() ?? string.Empty,
                Puesto = reader["Puesto"]?.ToString() ?? string.Empty,
                MesasAtendidas = Convert.ToInt32(reader["MesasAtendidas"]),
                FacturacionTotal = Convert.ToDouble(reader["FacturacionTotal"]),
                PromedioPorMesa = Convert.ToDouble(reader["PromedioPorMesa"])
            });
        }
        return lista;
    }
}

