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

    public bool PuedeVerClientes => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion;

    public bool PuedeVerMesas => SesionGlobal.RolActual == RolUsuario.Dueno 
                              ||  SesionGlobal.RolActual == RolUsuario.Gerente
                              ||  SesionGlobal.RolActual == RolUsuario.Cajero
                              ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                              ||  SesionGlobal.RolActual == RolUsuario.Mozo;

    public bool PuedeVerEventos => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion;

    public bool PuedeVerReservas => SesionGlobal.RolActual == RolUsuario.Dueno 
                                 ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                 ||  SesionGlobal.RolActual == RolUsuario.Cajero
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
        OnPropertyChanged(nameof(PuedeVerClientes));
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

    // Comandos para cambiar de sección al hacer clic en los botones del menú
    [RelayCommand]
    private void IrAInicio()
    {
        MesaService? mesaService = null;
        try
        {
            var mesaRepo = new MesaRepository(new RestoAppDbContext());
            mesaService = new MesaService(mesaRepo);
        }
        catch { }
        CurrentView = new InicioViewModel(mesaService, navigateAMesasAction: IrAMesas);
    }

    [RelayCommand]
    private void IrAClientes()
    {
        // CRUD Clientes
    }

    [RelayCommand]
    private void IrAEmpleados()
    {
        EmpleadoService? service = null;
        try
        {
            var empleadoRepo = new EmpleadoRepository(new RestoAppDbContext());
            service = new EmpleadoService(empleadoRepo);
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
            var reservaRepo = new ReservaRepository(new RestoAppDbContext());
            service = new ReservaService(reservaRepo);
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
            var mesaRepo = new MesaRepository(new RestoAppDbContext());
            service = new MesaService(mesaRepo);
        }
        catch { }
        CurrentView = new MesasViewModel(service);
    }

    [RelayCommand]
    private void IrACaja()
    {
        PagoService? pagoService = null;
        try
        {
            var pagoRepo = new PagoRepository(new RestoAppDbContext());
            pagoService = new PagoService(pagoRepo);
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
            var eventoRepo = new EventoRepository(new RestoAppDbContext());
            eventoService = new EventoService(eventoRepo);
        }
        catch { }
        CurrentView = new EventosViewModel(eventoService);
    }
}