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
        return await _dbSet
            .Include(p => p.MetodoPago)
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Cliente)
                    .ThenInclude(c => c!.PersonaInfo)
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Mesas)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();
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

}

