using RestoApp.Data.Repositories;

namespace RestoApp.Business.Services;

public class BackupService
{
    private readonly BackupRepository _backupRepository;

    public BackupService(BackupRepository backupRepository)
    {
        _backupRepository = backupRepository;
    }

    public Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        var fullPath = Path.GetFullPath(backupFilePath);
        var destinationDirectory = Path.GetDirectoryName(fullPath);
        if (destinationDirectory is null || !Directory.Exists(destinationDirectory))
        {
            throw new DirectoryNotFoundException("El directorio de destino no existe.");
        }

        return _backupRepository.BackupDatabaseAsync(fullPath, cancellationToken);
    }
}
