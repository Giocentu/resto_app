using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;


namespace RestoApp.Data.Repositories;

public class PagoRepository : Repository<Pago>, IPagoRepository
{
    public PagoRepository(RestoAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pago>> GetPagosConDetallesAsync()
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.MetodoPago)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Cliente)
                        .ThenInclude(c => c!.PersonaInfo)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Mesas)
                .OrderByDescending(p => p.IdPago)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PAGO REPOSITORY] Conexión primaria falló: {ex.Message}. Reintentando con contexto limpio...");
            using var cleanContext = new RestoAppDbContext();
            return await cleanContext.Pagos
                .AsNoTracking()
                .Include(p => p.MetodoPago)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Cliente)
                        .ThenInclude(c => c!.PersonaInfo)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Mesas)
                .OrderByDescending(p => p.IdPago)
                .ToListAsync();
        }
    }
    public async Task AgregarItemACuentaSpAsync(
        int cuentaId,
        int idItem,
        int cantidad)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Cuenta_AgregarItem " +
            "@CuentaId = {0}, @IdItem = {1}, @Cantidad = {2}",
            cuentaId,
            idItem,
            cantidad);
    }
    public async Task<int> CrearCuentaSpAsync(int idMesa, long? dniEmpleado = null, int? idRol = null)
    {
        var nuevaCuentaIdParam = new SqlParameter("@NuevaCuentaId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        var dniParam = dniEmpleado.HasValue ? (object)dniEmpleado.Value : DBNull.Value;
        var rolParam = idRol.HasValue ? (object)idRol.Value : DBNull.Value;

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Cuenta_Crear @IdMesa = {0}, @DniEmpleado = {1}, @IdRol = {2}, @NuevaCuentaId = {3} OUTPUT",
            idMesa, dniParam, rolParam, nuevaCuentaIdParam);

        return Convert.ToInt32(nuevaCuentaIdParam.Value ?? 0);
    }

    public async Task<int> CrearCuentaConItemSpAsync(
    int idMesa,
    int idItem,
    int cantidad)
{
    var nuevaCuentaIdParam = new SqlParameter(
        "@NuevaCuentaId",
        SqlDbType.Int)
    {
        Direction = ParameterDirection.Output
    };

    await _context.Database.ExecuteSqlRawAsync(
        "EXEC sp_Cuenta_CrearConItem " +
        "@IdMesa = {0}, @IdItem = {1}, @Cantidad = {2}, " +
        "@NuevaCuentaId = {3} OUTPUT",
        idMesa,
        idItem,
        cantidad,
        nuevaCuentaIdParam);

    if (nuevaCuentaIdParam.Value is null ||
        nuevaCuentaIdParam.Value == DBNull.Value)
    {
        throw new InvalidOperationException(
            "La base de datos no devolvió el ID de la cuenta.");
    }

    return Convert.ToInt32(nuevaCuentaIdParam.Value);
}

    public async Task<IEnumerable<MetodoPago>> GetMetodosPagoAsync()
    {
        return await _context.MetodosPago.ToListAsync();
    }

    public async Task<int> CrearPagoSpAsync(decimal monto, DateTime fechaPago, int idMetodo, int? idReserva = null, int? idMesa = null)
    {
        var nuevoIdParam = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var idReservaParam = idReserva.HasValue ? (object)idReserva.Value : DBNull.Value;
        var idMesaParam = idMesa.HasValue ? (object)idMesa.Value : DBNull.Value;

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Pago_Crear @Monto = {0}, @FechaPago = {1}, @IdMetodo = {2}, @IdReserva = {3}, @IdMesa = {4}, @NuevoId = {5} OUTPUT",
            (double)monto, fechaPago, idMetodo, idReservaParam, idMesaParam, nuevoIdParam);

        return (int)(nuevoIdParam.Value ?? 0);
    }
    public async Task<List<CuentaPendienteDto>> ObtenerCuentasPendientesAsync()
{
    return await _context.Database
        .SqlQueryRaw<CuentaPendienteDto>("EXEC sp_Cuenta_ObtenerTodasPendientes")
        .ToListAsync();
}

public async Task<int> CerrarCuentaAsync(int cuentaId, int idMetodo)
{
    var nuevoPagoIdParam = new SqlParameter("@NuevoPagoId", SqlDbType.Int)
    {
        Direction = ParameterDirection.Output
    };

    await _context.Database.ExecuteSqlRawAsync(
        "EXEC sp_Cuenta_Cerrar @CuentaId = {0}, @IdMetodo = {1}, @NuevoPagoId = {2} OUTPUT",
        cuentaId,
        idMetodo,
        nuevoPagoIdParam);

    return Convert.ToInt32(nuevoPagoIdParam.Value);
}

    public async Task QuitarItemDeCuentaSpAsync(int cuentaId, int idItem, int cantidad = 1)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Cuenta_QuitarItem @CuentaId = {0}, @IdItem = {1}, @Cantidad = {2}",
            cuentaId,
            idItem,
            cantidad);
    }

    public async Task<List<CuentaItemDetalleDto>> ObtenerDetalleCuentaAsync(int cuentaId)
    {
        return await _context.Database
            .SqlQueryRaw<CuentaItemDetalleDto>(
                @"SELECT 
                    cb.id_cuenta_body AS IdCuentaBody,
                    cb.cuenta_id AS CuentaId,
                    cb.id_item AS IdItem,
                    m.descripcion AS Descripcion,
                    m.descripcion AS DescripcionItem,
                    cm.nombre AS Categoria,
                    cb.precio_unitario AS PrecioUnitario,
                    cb.cantidad AS Cantidad,
                    (cb.cantidad * cb.precio_unitario) AS Subtotal
                FROM cuenta_body cb
                INNER JOIN menu m ON cb.id_item = m.id_item
                INNER JOIN categoria_menu cm ON m.id_categoria = cm.id_categoria
                WHERE cb.cuenta_id = {0}
                ORDER BY cm.nombre ASC, m.descripcion ASC",
                cuentaId)
            .ToListAsync();
    }

    public async Task CambiarMozoCuentaAsync(int cuentaId, long dniEmpleado, int idRol)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE cuenta_head SET dni_empleado = {0}, id_rol = {1} WHERE cuenta_id = {2} AND estado = 'ABIERTA'",
            dniEmpleado,
            idRol,
            cuentaId);
    }

    public async Task CancelarCuentaAsync(int cuentaId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Cuenta_Cancelar @CuentaId = {0}",
            cuentaId);
    }

    public async Task<Dictionary<int, CuentaPagoInfoDto>> ObtenerInfoCuentasPagosAsync()
    {
        var dict = new Dictionary<int, CuentaPagoInfoDto>();
        try
        {
            using var isolatedContext = new RestoAppDbContext();
            var conn = isolatedContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        ch.cuenta_id AS CuentaId,
                        ch.id_mesa AS IdMesa,
                        m.nro_mesa AS NroMesa,
                        ch.fecha_cierre AS FechaCierre,
                        ISNULL(pMozo.nombre + ' ' + pMozo.apellido, 'Mozo de Turno') AS AtendidoPor
                    FROM cuenta_head ch
                    INNER JOIN mesa m ON ch.id_mesa = m.id_mesa
                    LEFT JOIN empleado e ON ch.dni_empleado = e.dni_empleado AND ch.id_rol = e.id_rol
                    LEFT JOIN persona pMozo ON e.dni_empleado = pMozo.dni";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int ctaId = reader.GetInt32(0);
                    int idMesa = reader.GetInt32(1);
                    int nroMesa = reader.GetInt32(2);
                    DateTime? fechaCierre = reader.IsDBNull(3) ? null : reader.GetDateTime(3);
                    string mozo = reader.IsDBNull(4) ? "Mozo de Turno" : reader.GetString(4);

                    dict[ctaId] = new CuentaPagoInfoDto
                    {
                        CuentaId = ctaId,
                        IdMesa = idMesa,
                        NroMesa = nroMesa,
                        FechaCierre = fechaCierre,
                        AtendidoPor = mozo
                    };
                }
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    await conn.CloseAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OBTENER INFO CUENTAS SQL ERROR] {ex.Message}");
        }

        return dict;
    }
}

