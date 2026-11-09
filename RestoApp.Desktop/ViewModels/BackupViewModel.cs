using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace RestoApp.Desktop.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    public MainViewModel MainVm { get; }

    public BackupViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;
        MainVm.PropertyChanged += OnMainVmPropertyChanged;
    }

    private void OnMainVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.BackupStatus) ||
            e.PropertyName == nameof(MainViewModel.IsBackupInProgress) ||
            e.PropertyName == nameof(MainViewModel.IsRestoreInProgress))
        {
            OnPropertyChanged(e.PropertyName);
            OnPropertyChanged(nameof(TieneEstado));
            OnPropertyChanged(nameof(EstaProcesando));
        }
    }

    public IRelayCommand CrearBackupCommand => MainVm.CrearBackupCommand;
    public IRelayCommand RestaurarBackupCommand => MainVm.RestaurarBackupCommand;
    public string BackupStatus => MainVm.BackupStatus;
    public bool IsBackupInProgress => MainVm.IsBackupInProgress;
    public bool IsRestoreInProgress => MainVm.IsRestoreInProgress;
    public bool EstaProcesando => IsBackupInProgress || IsRestoreInProgress;
    public bool TieneEstado => !string.IsNullOrWhiteSpace(BackupStatus);
}

