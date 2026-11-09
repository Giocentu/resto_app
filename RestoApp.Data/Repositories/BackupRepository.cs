using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using RestoApp.Entities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RestoApp.Data.Repositories;

public class BackupRepository
{
    private readonly RestoAppDbContext _context;

    public BackupRepository(RestoAppDbContext context)
    {
        _context = context;
    }

    public async Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        var rawDbName = _context.Database.GetDbConnection().Database;
        var databaseName = string.IsNullOrWhiteSpace(rawDbName) ? "resto_DB" : rawDbName.Replace("]", "]]", StringComparison.Ordinal);
        var sqlCommand = $"BACKUP DATABASE [{databaseName}] TO DISK = {{0}} WITH FORMAT, NAME = 'RestoApp Full Backup'";

        var previousTimeout = _context.Database.GetCommandTimeout();
        try
        {
            _context.Database.SetCommandTimeout(300);
            await _context.Database.ExecuteSqlRawAsync(sqlCommand, new object[] { backupFilePath }, cancellationToken);
        }
        finally
        {
            _context.Database.SetCommandTimeout(previousTimeout);
        }
    }

    public async Task<BackupHeaderInfo> LeerEncabezadoBackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("El archivo de copia de seguridad (.bak) no existe.", backupFilePath);
        }

        var conn = _context.Database.GetDbConnection();
        var connectionString = conn.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = @"Server=HERNAN\SQLEXPRESS;Database=resto_DB;Integrated Security=True;TrustServerCertificate=True;";
        }

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        using var masterConn = new SqlConnection(builder.ConnectionString);
        await masterConn.OpenAsync(cancellationToken);

        using var cmd = masterConn.CreateCommand();
        cmd.CommandText = "RESTORE HEADERONLY FROM DISK = @bakPath";
        cmd.Parameters.Add(new SqlParameter("@bakPath", backupFilePath));
        cmd.CommandTimeout = 60;

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("No se pudo leer el encabezado del archivo de backup. El archivo puede estar corrupto o vacío.");
        }

        var header = new BackupHeaderInfo();

        var dbNameOrdinal = reader.GetOrdinal("DatabaseName");
        if (!reader.IsDBNull(dbNameOrdinal))
        {
            header.DatabaseName = reader.GetString(dbNameOrdinal);
        }

        var backupTypeOrdinal = reader.GetOrdinal("BackupType");
        if (!reader.IsDBNull(backupTypeOrdinal))
        {
            header.BackupType = Convert.ToInt32(reader.GetValue(backupTypeOrdinal));
        }

        var serverNameOrdinal = reader.GetOrdinal("ServerName");
        if (!reader.IsDBNull(serverNameOrdinal))
        {
            header.ServerName = reader.GetString(serverNameOrdinal);
        }

        var backupNameOrdinal = reader.GetOrdinal("BackupName");
        if (!reader.IsDBNull(backupNameOrdinal))
        {
            header.BackupName = reader.GetString(backupNameOrdinal);
        }

        var finishDateOrdinal = reader.GetOrdinal("BackupFinishDate");
        if (!reader.IsDBNull(finishDateOrdinal))
        {
            header.BackupFinishDate = reader.GetDateTime(finishDateOrdinal);
        }

        return header;
    }

    public async Task RestoreDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        var conn = _context.Database.GetDbConnection();
        var connectionString = conn.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = @"Server=HERNAN\SQLEXPRESS;Database=resto_DB;Integrated Security=True;TrustServerCertificate=True;";
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var targetDbName = string.IsNullOrWhiteSpace(builder.InitialCatalog)
            ? (string.IsNullOrWhiteSpace(conn.Database) ? "resto_DB" : conn.Database)
            : builder.InitialCatalog;
        var safeDbName = targetDbName.Replace("]", "]]", StringComparison.Ordinal);

        // Cambiar a la base 'master' para poder operar sobre resto_DB sin bloqueo de uso
        builder.InitialCatalog = "master";

        using var masterConn = new SqlConnection(builder.ConnectionString);
        await masterConn.OpenAsync(cancellationToken);

        // 0. Ejecutar RESTORE HEADERONLY para validar el archivo antes de modificar conexiones o la base
        BackupHeaderInfo header;
        try
        {
            using var cmdHeader = masterConn.CreateCommand();
            cmdHeader.CommandText = "RESTORE HEADERONLY FROM DISK = @bakPath";
            cmdHeader.Parameters.Add(new SqlParameter("@bakPath", backupFilePath));
            cmdHeader.CommandTimeout = 60;

            using var reader = await cmdHeader.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("El archivo de copia de seguridad no contiene información de encabezado válida.");
            }

            header = new BackupHeaderInfo
            {
                DatabaseName = reader.IsDBNull(reader.GetOrdinal("DatabaseName")) ? string.Empty : reader.GetString(reader.GetOrdinal("DatabaseName")),
                BackupType = reader.IsDBNull(reader.GetOrdinal("BackupType")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("BackupType"))),
                ServerName = reader.IsDBNull(reader.GetOrdinal("ServerName")) ? string.Empty : reader.GetString(reader.GetOrdinal("ServerName")),
                BackupName = reader.IsDBNull(reader.GetOrdinal("BackupName")) ? string.Empty : reader.GetString(reader.GetOrdinal("BackupName")),
                BackupFinishDate = reader.IsDBNull(reader.GetOrdinal("BackupFinishDate")) ? null : reader.GetDateTime(reader.GetOrdinal("BackupFinishDate"))
            };
        }
        catch (SqlException sqlEx)
        {
            throw new InvalidOperationException($"El archivo seleccionado no es un archivo de backup válido de SQL Server o está dañado: {sqlEx.Message}", sqlEx);
        }

        // Validar que sea una copia completa de base de datos (BackupType == 1)
        if (!header.EsCopiaCompleta)
        {
            throw new InvalidOperationException(
                $"El archivo seleccionado no es una copia de seguridad completa (Tipo detectado: {header.BackupType}). " +
                "Solo se permite restaurar copias completas de base de datos para garantizar la integridad.");
        }

        // Validar que pertenezca a la base de datos resto_DB
        if (!string.IsNullOrWhiteSpace(header.DatabaseName) &&
            !string.Equals(header.DatabaseName, targetDbName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"El archivo de backup pertenece a la base de datos '{header.DatabaseName}', " +
                $"no a '{targetDbName}'. Se canceló la restauración para prevenir sobreescritura accidental con una base equivocada.");
        }

        // 1. Poner resto_DB en SINGLE_USER con rollback inmediato para desconectar clientes activos
        try
        {
            using var cmdSingle = masterConn.CreateCommand();
            cmdSingle.CommandText = $"ALTER DATABASE [{safeDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            cmdSingle.CommandTimeout = 60;
            await cmdSingle.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException)
        {
            // Si la base no existía aún o ya estaba en single_user, continuamos
        }

        try
        {
            // 2. Ejecutar la restauración con REPLACE
            using var cmdRestore = masterConn.CreateCommand();
            cmdRestore.CommandText = $"RESTORE DATABASE [{safeDbName}] FROM DISK = @bakPath WITH REPLACE";
            cmdRestore.Parameters.Add(new SqlParameter("@bakPath", backupFilePath));
            cmdRestore.CommandTimeout = 300; // 5 minutos
            await cmdRestore.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            // 3. Volver a MULTI_USER pase lo que pase
            try
            {
                using var cmdMulti = masterConn.CreateCommand();
                cmdMulti.CommandText = $"ALTER DATABASE [{safeDbName}] SET MULTI_USER";
                cmdMulti.CommandTimeout = 60;
                await cmdMulti.ExecuteNonQueryAsync(CancellationToken.None);
            }
            catch
            {
                // Ignorar error secundario para preservar la excepción principal
            }
        }

        // Limpiar todas las conexiones en caché del pool de ADO.NET
        SqlConnection.ClearAllPools();
    }
}
