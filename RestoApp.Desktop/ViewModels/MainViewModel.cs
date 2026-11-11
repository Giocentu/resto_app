using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.Views;
using RestoApp.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _backupStatus = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearBackupCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestaurarBackupCommand))]
    private bool _isBackupInProgress;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearBackupCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestaurarBackupCommand))]
    private bool _isRestoreInProgress;

    private readonly Func<Task<string?>>? _seleccionarRutaBackupAsync;
    private readonly Func<Task<string?>>? _seleccionarRutaRestoreAsync;
    private readonly Func<Task<bool>>? _solicitarAutorizacionRestoreAsync;

    // Propiedades de visibilidad basadas en el rol global
    public bool EsDueno => SesionGlobal.RolActual == RolUsuario.Dueno;

    public bool PuedeVerMesas => SesionGlobal.RolActual == RolUsuario.Dueno
                              || SesionGlobal.RolActual == RolUsuario.Gerente
                              || SesionGlobal.RolActual == RolUsuario.Recepcion
                              || SesionGlobal.RolActual == RolUsuario.Mozo;

    public bool PuedeVerEventos => SesionGlobal.RolActual == RolUsuario.Dueno
                                || SesionGlobal.RolActual == RolUsuario.Gerente
                                || SesionGlobal.RolActual == RolUsuario.Recepcion;

    public bool PuedeVerReservas => SesionGlobal.RolActual == RolUsuario.Dueno
                                 || SesionGlobal.RolActual == RolUsuario.Gerente
                                 || SesionGlobal.RolActual == RolUsuario.Recepcion;

    public bool PuedeVerPersonal => SesionGlobal.RolActual == RolUsuario.Dueno
                                 || SesionGlobal.RolActual == RolUsuario.Gerente;

    public bool PuedeVerCaja => SesionGlobal.RolActual == RolUsuario.Dueno
                             || SesionGlobal.RolActual == RolUsuario.Gerente
                             || SesionGlobal.RolActual == RolUsuario.Cajero;

    public bool PuedeVerPrincipal => SesionGlobal.RolActual == RolUsuario.Dueno
                                  || SesionGlobal.RolActual == RolUsuario.Gerente
                                  || SesionGlobal.RolActual == RolUsuario.Cajero
                                  || SesionGlobal.RolActual == RolUsuario.Recepcion
                                  || SesionGlobal.RolActual == RolUsuario.Mozo;

    public bool PuedeVerMenu => SesionGlobal.RolActual == RolUsuario.Dueno
                             || SesionGlobal.RolActual == RolUsuario.Gerente
                             || SesionGlobal.RolActual == RolUsuario.Mozo;

    public bool PuedeVerReportes => SesionGlobal.RolActual == RolUsuario.Dueno
                                || SesionGlobal.RolActual == RolUsuario.Gerente;

    public string NombreUsuarioConectado => string.IsNullOrWhiteSpace(SesionGlobal.NombreCompletoActual)
        ? (string.IsNullOrWhiteSpace(SesionGlobal.UsuarioActual) ? "Usuario Conectado" : SesionGlobal.UsuarioActual)
        : SesionGlobal.NombreCompletoActual;

    public string RolActualTexto => SesionGlobal.RolActual switch
    {
        RolUsuario.Dueno => "Dueño",
        RolUsuario.Gerente => "Gerente",
        RolUsuario.Cajero => "Cajero",
        RolUsuario.Recepcion => "Recepción",
        RolUsuario.Mozo => "Mozo",
        _ => "Personal"
    };

    // 0 = Dueño, 1 = Gerente, 2 = Cajero, 3 = Recepción, 4 = Mozo
    [ObservableProperty]
    private int _indiceRolSeleccionado = 0;

    public MainViewModel(
        Func<Task<string?>>? seleccionarRutaBackupAsync = null,
        Func<Task<string?>>? seleccionarRutaRestoreAsync = null,
        Func<Task<bool>>? solicitarAutorizacionRestoreAsync = null)
    {
        _seleccionarRutaBackupAsync = seleccionarRutaBackupAsync;
        _seleccionarRutaRestoreAsync = seleccionarRutaRestoreAsync;
        _solicitarAutorizacionRestoreAsync = solicitarAutorizacionRestoreAsync;

        // Sincronizar el ComboBox con el rol actual establecido en el Login o SesionGlobal
        _indiceRolSeleccionado = SesionGlobal.RolActual switch
        {
            RolUsuario.Dueno => 0,
            RolUsuario.Gerente => 1,
            RolUsuario.Cajero => 2,
            RolUsuario.Recepcion => 3,
            RolUsuario.Mozo => 4,
            _ => 0
        };

        IrAInicio();
    }

    // Este método se ejecuta automáticamente cuando IndiceRolSeleccionado cambia
    partial void OnIndiceRolSeleccionadoChanged(int value)
    {
        // Actualizamos la sesión global
        SesionGlobal.RolActual = value switch
        {
            0 => RolUsuario.Dueno,
            1 => RolUsuario.Gerente,
            2 => RolUsuario.Cajero,
            3 => RolUsuario.Recepcion,
            _ => RolUsuario.Mozo
        };

        // Notificamos a la barra lateral que re-evalúe qué botones mostrar
        OnPropertyChanged(nameof(EsDueno));
        OnPropertyChanged(nameof(PuedeVerMesas));
        OnPropertyChanged(nameof(PuedeVerEventos));
        OnPropertyChanged(nameof(PuedeVerReservas));
        OnPropertyChanged(nameof(PuedeVerPersonal));
        OnPropertyChanged(nameof(PuedeVerCaja));
        OnPropertyChanged(nameof(PuedeVerPrincipal));
        OnPropertyChanged(nameof(PuedeVerMenu));
        OnPropertyChanged(nameof(PuedeVerReportes));
        OnPropertyChanged(nameof(NombreUsuarioConectado));
        CrearBackupCommand.NotifyCanExecuteChanged();
        RestaurarBackupCommand.NotifyCanExecuteChanged();

        // Recargamos la vista central actual si aplica
        if (CurrentView is InicioViewModel)
        {
            IrAInicio();
        }
        else if (CurrentView is MenuViewModel)
        {
            IrAMenu();
        }
        else if (CurrentView is ReportesViewModel)
        {
            IrAReportes();
        }
        else if (CurrentView is MesasViewModel)
        {
            IrAMesas();
        }
        else if (CurrentView is ReservasViewModel)
        {
            IrAReservas();
        }
        else if (CurrentView is EmpleadosViewModel)
        {
            IrAEmpleados();
        }
        else if (CurrentView is CajaViewModel)
        {
            IrACaja();
        }
        else if (CurrentView is EventosViewModel)
        {
            IrAEventos();
        }
    }

    private InicioViewModel? _inicioViewModelCache;

    // Comandos para cambiar de sección al hacer clic en los botones del menú
    private MesaService CreateMesaService()
    {
        return App.Services?.GetService(typeof(MesaService)) as MesaService
            ?? new MesaService(new MesaRepository(new RestoAppDbContext()));
    }

    private ReservaService CreateReservaService()
    {
        return App.Services?.GetService(typeof(ReservaService)) as ReservaService
            ?? new ReservaService(new ReservaRepository(new RestoAppDbContext()));
    }

    private EmpleadoService CreateEmpleadoService()
    {
        return App.Services?.GetService(typeof(EmpleadoService)) as EmpleadoService
            ?? new EmpleadoService(new EmpleadoRepository(new RestoAppDbContext()));
    }

    private EventoService CreateEventoService()
    {
        return App.Services?.GetService(typeof(EventoService)) as EventoService
            ?? new EventoService(new EventoRepository(new RestoAppDbContext()));
    }

    private PagoService CreatePagoService()
    {
        return App.Services?.GetService(typeof(PagoService)) as PagoService
            ?? new PagoService(new PagoRepository(new RestoAppDbContext()));
    }

    private BackupService CreateBackupService()
    {
        return App.Services?.GetService(typeof(BackupService)) as BackupService
            ?? new BackupService(new BackupRepository(new RestoAppDbContext()));
    }

    private MenuService CreateMenuService()
    {
        return App.Services?.GetService(typeof(MenuService)) as MenuService
            ?? new MenuService(new MenuRepository(new RestoAppDbContext()));
    }

    private ReporteService CreateReporteService()
    {
        return App.Services?.GetService(typeof(ReporteService)) as ReporteService
            ?? new ReporteService(new ReporteRepository(new RestoAppDbContext()));
    }

    private bool CanCrearBackup() => EsDueno && !IsBackupInProgress && !IsRestoreInProgress;

    [RelayCommand(CanExecute = nameof(CanCrearBackup))]
    private async Task CrearBackupAsync()
    {
        if (_seleccionarRutaBackupAsync is null)
        {
            BackupStatus = "No se pudo abrir el selector de destino.";
            return;
        }

        IsBackupInProgress = true;
        try
        {
            string? backupFilePath;
            try
            {
                BackupStatus = "Selecciona el destino de la copia...";
                backupFilePath = await _seleccionarRutaBackupAsync();
            }
            catch (Exception ex)
            {
                BackupStatus = $"No se pudo seleccionar el destino: {ex.Message}";
                return;
            }

            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                BackupStatus = string.Empty;
                return;
            }

            BackupStatus = "Creando copia de seguridad...";
            await CreateBackupService().BackupDatabaseAsync(backupFilePath);
            BackupStatus = $"Copia creada: {Path.GetFileName(backupFilePath)}";
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("error 5", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Access is denied", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Acceso denegado", StringComparison.OrdinalIgnoreCase))
            {
                BackupStatus = "Error de permisos: SQL Server no tiene permiso para escribir en esa carpeta. Intenta elegir C:\\Backups o una carpeta con permisos para SQL Server.";
            }
            else
            {
                BackupStatus = $"No se pudo crear la copia: {ex.Message}";
            }
        }
        finally
        {
            IsBackupInProgress = false;
        }
    }

    private bool CanRestaurarBackup() => EsDueno && !IsBackupInProgress && !IsRestoreInProgress;

    [RelayCommand(CanExecute = nameof(CanRestaurarBackup))]
    private async Task RestaurarBackupAsync()
    {
        if (_seleccionarRutaRestoreAsync is null || _solicitarAutorizacionRestoreAsync is null)
        {
            BackupStatus = "No se pudo abrir el selector o la ventana de autorización.";
            return;
        }

        try
        {
            BackupStatus = "Seleccione el archivo .bak a restaurar...";
            var backupFilePath = await _seleccionarRutaRestoreAsync();

            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                BackupStatus = string.Empty;
                return;
            }

            if (!File.Exists(backupFilePath))
            {
                BackupStatus = "El archivo seleccionado no existe.";
                return;
            }

            BackupStatus = "Verificando archivo de backup (RESTORE HEADERONLY)...";
            var header = await CreateBackupService().LeerEncabezadoBackupAsync(backupFilePath);

            if (!header.EsCopiaCompleta)
            {
                BackupStatus = $"El archivo no es una copia de seguridad completa (Tipo: {header.BackupType}). Restauración bloqueada.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(header.DatabaseName) &&
                !string.Equals(header.DatabaseName, "resto_DB", StringComparison.OrdinalIgnoreCase))
            {
                BackupStatus = $"El backup pertenece a '{header.DatabaseName}', no a 'resto_DB'. Restauración bloqueada por seguridad.";
                return;
            }

            var fechaStr = header.BackupFinishDate.HasValue ? header.BackupFinishDate.Value.ToString("yyyy-MM-dd HH:mm") : "fecha desconocida";
            BackupStatus = $"Backup válido de '{header.DatabaseName}' ({fechaStr}). Esperando autorización de 2 usuarios...";

            var autorizado = await _solicitarAutorizacionRestoreAsync();
            if (!autorizado)
            {
                BackupStatus = "Restauración cancelada: autorización no completada.";
                return;
            }

            IsRestoreInProgress = true;
            BackupStatus = $"Restaurando base de datos desde {Path.GetFileName(backupFilePath)}...";

            await CreateBackupService().RestoreDatabaseAsync(backupFilePath);
            BackupStatus = $"¡Base de datos restaurada exitosamente desde {Path.GetFileName(backupFilePath)}!";

            // Recargamos la vista central
            if (CurrentView is InicioViewModel)
            {
                _inicioViewModelCache = null;
                IrAInicio();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("error 5", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Access is denied", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Acceso denegado", StringComparison.OrdinalIgnoreCase))
            {
                BackupStatus = "Error de permisos: SQL Server no tiene permiso para leer el archivo en esa carpeta. Intenta mover el .bak a C:\\Backups o a la carpeta de SQL Server.";
            }
            else
            {
                BackupStatus = $"No se pudo restaurar: {ex.Message}";
            }
        }
        finally
        {
            IsRestoreInProgress = false;
        }
    }

    [RelayCommand]
    private void IrAInicio()
    {
        if (_inicioViewModelCache == null)
        {
            MesaService? mesaService = null;
            UbicacionService? ubicacionService = null;
            PagoService? pagoService = null;
            try
            {
                mesaService = CreateMesaService();
                ubicacionService = App.Services?.GetService(typeof(UbicacionService)) as UbicacionService
                    ?? new UbicacionService(new UbicacionRepository(new RestoAppDbContext()));
                pagoService = CreatePagoService();
            }
            catch { }
            _inicioViewModelCache = new InicioViewModel(
                mesaService, 
                navigateAMesasAction: IrAMesas, 
                ubicacionService: ubicacionService,
                pagoService: pagoService);
        }
        else
        {
            _ = _inicioViewModelCache.CargarMesasAsync();
        }

        CurrentView = _inicioViewModelCache;
    }

    [RelayCommand]
    private void IrAEmpleados()
    {
        EmpleadoService? service = null;
        try
        {
            service = CreateEmpleadoService();
        }
        catch { }
        CurrentView = new EmpleadosViewModel(service);
    }

    [RelayCommand]
    private void IrAReservas()
    {
        ReservaService? service = null;
        try
        {
            service = CreateReservaService();
        }
        catch { }
        CurrentView = new ReservasViewModel(service);
    }

    [RelayCommand]
    private void IrAMesas()
    {
        MesaService? service = null;
        try
        {
            service = CreateMesaService();
        }
        catch { }
        CurrentView = new MesasViewModel(service, () => IrAInicio());
    }

    [RelayCommand]
    private void IrACaja()
    {
        CurrentView = new CajaViewModel(
            CreatePagoService(),
            CreateMesaService(),
            CreateMenuService());
    }

    [RelayCommand]
    private void IrAEventos()
    {
        EventoService? eventoService = null;
        try
        {
            eventoService = CreateEventoService();
        }
        catch { }
        CurrentView = new EventosViewModel(eventoService);
    }

    [RelayCommand]
    private void IrAMenu()
    {
        MenuService? menuService = null;
        try
        {
            menuService = CreateMenuService();
        }
        catch { }
        CurrentView = new MenuViewModel(menuService);
    }

    [RelayCommand]
    private void IrAReportes()
    {
        ReporteService? reporteService = null;
        try
        {
            reporteService = CreateReporteService();
        }
        catch { }
        CurrentView = new ReportesViewModel(reporteService);
    }

    [RelayCommand]
    private void IrABackup()
    {
        CurrentView = new BackupViewModel(this);
    }

}