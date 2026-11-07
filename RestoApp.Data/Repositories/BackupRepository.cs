using Microsoft.EntityFrameworkCore;

namespace RestoApp.Data.Repositories;

public class BackupRepository
{
    private readonly RestoAppDbContext _context;

    public BackupRepository(RestoAppDbContext context)
    {
        _context = context;
    }

    public Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        var databaseName = _context.Database.GetDbConnection().Database
            .Replace("]", "]]", StringComparison.Ordinal);
        var sqlCommand = $"BACKUP DATABASE [{databaseName}] TO DISK = {{0}} WITH FORMAT, NAME = 'RestoApp Full Backup'";

        return _context.Database.ExecuteSqlRawAsync(sqlCommand, new object[] { backupFilePath }, cancellationToken);
    }
}
