using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;

namespace RestoApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    // Propiedades de visibilidad basadas en el rol global
    public bool EsAdmin => SesionGlobal.TipoUsuarioActual == 1;

    // Si el mozo no debe ver clientes/personal, solo devolvemos true si es Admin
    public bool PuedeVerClientes => SesionGlobal.TipoUsuarioActual == 1;

    // Los mozos y admins pueden ver mesas
    public bool PuedeVerMesas => SesionGlobal.TipoUsuarioActual == 1 || SesionGlobal.TipoUsuarioActual == 2;

    // 0 = Admin, 1 = Mozo. Empezamos en 0 (Admin)
    [ObservableProperty]
    private int _indiceRolSeleccionado = 0;

    public MainViewModel()
    {
        // Al iniciar la aplicación cargamos la vista Inicio por defecto
        IrAInicio();
    }

    // Este método se ejecuta automáticamente cuando IndiceRolSeleccionado cambia
    partial void OnIndiceRolSeleccionadoChanged(int value)
    {
        // Actualizamos la sesión global: 0 -> 1 (Admin), 1 -> 2 (Mozo)
        SesionGlobal.TipoUsuarioActual = value == 0 ? 1 : 2;

        // Notificamos a la barra lateral que re-evalúe los permisos
        OnPropertyChanged(nameof(EsAdmin));
        OnPropertyChanged(nameof(PuedeVerClientes));
        OnPropertyChanged(nameof(PuedeVerMesas));

        // Si estamos en Inicio o Mesas, recargamos la vista activa
        if (CurrentView is InicioViewModel)
        {
            IrAInicio();
        }
        else if (CurrentView is MesasViewModel)
        {
            IrAMesas();
        }
    }

    private InicioViewModel? _inicioViewModel;
    private CajaViewModel? _cajaViewModel;
    private MesasViewModel? _mesasViewModel;

    // Comandos de navegación para la barra lateral
    [RelayCommand]
    private void IrAInicio()
    {
        if (_inicioViewModel == null)
        {
            MesaService? service = null;
            try
            {
                var mesaRepo = new MesaRepository(new RestoAppDbContext());
                service = new MesaService(mesaRepo);
            }
            catch
            {
            }
            _inicioViewModel = new InicioViewModel(service, navigateAMesasAction: IrAMesas);
        }

        CurrentView = _inicioViewModel;
    }

    [RelayCommand]
    private void IrAReservas()
    {
        // Se asignará ReservasViewModel cuando se implemente la vista
    }

    [RelayCommand]
    private void IrAPersonal()
    {
        // Se asignará PersonalViewModel cuando se implemente la vista
    }

    [RelayCommand]
    private void IrACaja()
    {
        if (_cajaViewModel == null)
        {
            PagoService? service = null;
            try
            {
                var pagoRepo = new PagoRepository(new RestoAppDbContext());
                service = new PagoService(pagoRepo);
            }
            catch
            {
            }
            _cajaViewModel = new CajaViewModel(service);
        }

        CurrentView = _cajaViewModel;
    }

    private EventosViewModel? _eventosViewModel;

    [RelayCommand]
    private void IrAEventos()
    {
        if (_eventosViewModel == null)
        {
            EventoService? service = null;
            try
            {
                var eventoRepo = new EventoRepository(new RestoAppDbContext());
                service = new EventoService(eventoRepo);
            }
            catch
            {
            }
            _eventosViewModel = new EventosViewModel(service);
        }

        CurrentView = _eventosViewModel;
    }

    [RelayCommand]
    private void IrAMesas()
    {
        if (_mesasViewModel == null)
        {
            MesaService? service = null;
            try
            {
                var mesaRepo = new MesaRepository(new RestoAppDbContext());
                service = new MesaService(mesaRepo);
            }
            catch
            {
            }
            _mesasViewModel = new MesasViewModel(service ?? new MesaService(new MesaRepository(new RestoAppDbContext())));
        }

        CurrentView = _mesasViewModel;
    }
}