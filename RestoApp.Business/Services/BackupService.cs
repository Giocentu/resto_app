using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Business.Services;

public class BackupService
{
    private readonly BackupRepository _backupRepository;

    public BackupService(BackupRepository backupRepository)
    {
        _backupRepository = backupRepository;
    }

    public Task<BackupHeaderInfo> LeerEncabezadoBackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        var fullPath = Path.GetFullPath(backupFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("El archivo de copia de seguridad no existe.", fullPath);
        }

        return _backupRepository.LeerEncabezadoBackupAsync(fullPath, cancellationToken);
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

    public Task RestoreDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        var fullPath = Path.GetFullPath(backupFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("El archivo de copia de seguridad no existe.", fullPath);
        }

        return _backupRepository.RestoreDatabaseAsync(fullPath, cancellationToken);
    }
}
