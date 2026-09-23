using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.Views;
using RestoApp.Entities;
using System;

namespace RestoApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    // Propiedades de visibilidad basadas en el rol global
    public bool EsDueno => SesionGlobal.RolActual == RolUsuario.Dueno;


    public bool PuedeVerMesas => SesionGlobal.RolActual == RolUsuario.Dueno 
                              ||  SesionGlobal.RolActual == RolUsuario.Gerente
                              ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                              ||  SesionGlobal.RolActual == RolUsuario.Mozo;

    public bool PuedeVerEventos => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion;

    public bool PuedeVerReservas => SesionGlobal.RolActual == RolUsuario.Dueno 
                                 ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                 ||  SesionGlobal.RolActual == RolUsuario.Recepcion;

    public bool PuedeVerPersonal => SesionGlobal.RolActual == RolUsuario.Dueno 
                                 ||  SesionGlobal.RolActual == RolUsuario.Gerente;

    public bool PuedeVerCaja => SesionGlobal.RolActual == RolUsuario.Dueno
                             || SesionGlobal.RolActual == RolUsuario.Gerente
                             || SesionGlobal.RolActual == RolUsuario.Cajero;

    public bool PuedeVerPrincipal => SesionGlobal.RolActual == RolUsuario.Dueno 
                                  ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                  ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                  ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                  ||  SesionGlobal.RolActual == RolUsuario.Mozo;

    // 0 = Dueño, 1 = Gerente, 2 = Cajero, 3 = Recepción, 4 = Mozo
    [ObservableProperty]
    private int _indiceRolSeleccionado = 0; 

    public MainViewModel()
    {
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

        // Recargamos la vista central actual si aplica
        if (CurrentView is InicioViewModel)
        {
            IrAInicio();
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

    [RelayCommand]
    private void IrAInicio()
    {
        if (_inicioViewModelCache == null)
        {
            MesaService? mesaService = null;
            try
            {
                mesaService = CreateMesaService();
            }
            catch { }
            _inicioViewModelCache = new InicioViewModel(mesaService, navigateAMesasAction: IrAMesas);
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
        CurrentView = new MesasViewModel(service, () => IrAInicio() );
    }

    [RelayCommand]
    private void IrACaja()
    {
        PagoService? pagoService = null;
        try
        {
            pagoService = CreatePagoService();
        }
        catch { }
        CurrentView = new CajaViewModel(pagoService);
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


}