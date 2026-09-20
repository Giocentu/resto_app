using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.Views;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    // Propiedades de visibilidad basadas en el rol global
    public bool EsDueno => SesionGlobal.RolActual ==  RolUsuario.Dueno;
    public bool PuedeVerClientes => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente;
    
    public bool PuedeVerMesas => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente;
    
    public bool PuedeVerEventos => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente;
    
    public bool PuedeVerReservas => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente;

    public bool PuedeVerPersonal => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente;

    public bool PuedeVerCaja => SesionGlobal.RolActual == RolUsuario.Cajero;

    public bool PuedeVerPrincipal => SesionGlobal.RolActual == RolUsuario.Dueno 
                                ||  SesionGlobal.RolActual == RolUsuario.Cajero
                                ||  SesionGlobal.RolActual == RolUsuario.Recepcion
                                ||  SesionGlobal.RolActual == RolUsuario.Gerente
                                ||  SesionGlobal.RolActual == RolUsuario.Mozo;

// 0 = Admin, 1 = Mozo. Empezamos en 1 (Mozo)
    [ObservableProperty]
    private int _indiceRolSeleccionado = 0; 

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

        // Recargamos la vista central actual para que se apliquen u oculten las columnas
        if (CurrentView is MesasViewModel)
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
        // Puedes agregar más if() aquí a medida que crees las otras vistas (Clientes, Empleados)
    }


    // Comandos para cambiar de sección al hacer clic en los botones del menú
    [RelayCommand]
    private void IrAClientes()
    {
        // Aquí asignaremos el ViewModel correspondiente al CRUD de clientes más adelante
        // CurrentView = new ClientesViewModel();
    }

    [RelayCommand]
    private void IrAEmpleados()
    {
        var empleadoRepo = new EmpleadoRepository(new RestoAppDbContext());
        var empleadoService = new EmpleadoService(empleadoRepo);
        CurrentView = new EmpleadosViewModel(empleadoService);
    }

    [RelayCommand]
    private void IrAReservas()
    {
        var reservaRepo = new ReservaRepository(new RestoAppDbContext());
        var reservaService = new ReservaService(reservaRepo);
        CurrentView = new ReservasViewModel(reservaService);
    }


    [RelayCommand]
    private void IrAMesas()
    {
        var mesaRepo = new MesaRepository(new RestoAppDbContext());
        var mesaService = new MesaService(mesaRepo);
        CurrentView = new MesasViewModel(mesaService);
    }
    
}