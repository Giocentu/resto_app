using System;

namespace RestoApp.Entities;

public class BackupHeaderInfo
{
    public string DatabaseName { get; set; } = string.Empty;
    public int BackupType { get; set; }
    public DateTime? BackupFinishDate { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string BackupName { get; set; } = string.Empty;

    public bool EsCopiaCompleta => BackupType == 1;
}

