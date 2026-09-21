using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class EmpleadosViewModel : ObservableObject
{
    private readonly EmpleadoService? _empleadoService;

    // Regla de negocio: Solo el Admin/Dueño o Gerente pueden ver botones de crear/editar/eliminar personal
    public bool PuedeAgregar => SesionGlobal.RolActual == RolUsuario.Dueno;
    public bool PuedeEditar => SesionGlobal.RolActual == RolUsuario.Dueno
                        || SesionGlobal.RolActual == RolUsuario.Gerente;

    [ObservableProperty]
    private ObservableCollection<EmpleadoItemViewModel> _empleados = new();

    [ObservableProperty]
    private ObservableCollection<EmpleadoItemViewModel> _empleadosBajas = new();

    [ObservableProperty]
    private bool _esVistaPrincipal = true;

    [ObservableProperty]
    private bool _esVistaBajas = false;

    [ObservableProperty]
    private string _origenDatosTexto = "🟢 Base de datos";

    [ObservableProperty]
    private string _origenDatosColor = "#27AE60";

    [RelayCommand]
    private void VerBajas()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = true;
        CargarEmpleadosBajas();
    }

    [RelayCommand]
    private void VolverPrincipal()
    {
        EsVistaPrincipal = true;
        EsVistaBajas = false;
    }

    [RelayCommand]
    private void RestaurarEmpleado(EmpleadoItemViewModel empleado)
    {
        if (empleado != null)
        {
            EmpleadosBajas.Remove(empleado);
            Empleados.Add(empleado);
        }
    }

    private void CargarEmpleadosBajas()
    {
        if (!EmpleadosBajas.Any())
        {
            EmpleadosBajas = new ObservableCollection<EmpleadoItemViewModel>
            {
                new EmpleadoItemViewModel { DniEmpleado = 11223344, NombreCompleto = "Juan Perez (Retirado)", RolCargo = "Mozo", Telefono = "11-2233-4455", Estado = "Inactivo", PuedeEditar = PuedeEditar }
            };
        }
    }

    [RelayCommand]
    private async Task DarBajaEmpleadoAsync(EmpleadoItemViewModel empleado)
    {
        if (empleado != null)
        {
            if (_empleadoService != null)
            {
                try
                {
                    await _empleadoService.BajaLogicaEmpleadoAsync(empleado.DniEmpleado, 1);
                }
                catch { }
            }
            Empleados.Remove(empleado);
            empleado.Estado = "Inactivo";
            EmpleadosBajas.Add(empleado);
        }
    }

    public EmpleadosViewModel(EmpleadoService? empleadoService = null)
    {
        _empleadoService = empleadoService;
        _ = CargarEmpleadosAsync();
    }

    public async Task CargarEmpleadosAsync()
    {
        var listaMapeada = new List<EmpleadoItemViewModel>();

        if (_empleadoService != null)
        {
            try
            {
                var listaEntidades = await _empleadoService.ObtenerEmpleadosAsync(soloActivos: true);
                
                foreach (var emp in listaEntidades)
                {
                    string nombre = emp.PersonaInfo?.Nombre ?? "Empleado";
                    string apellido = emp.PersonaInfo?.Apellido ?? "";
                    string cargo = emp.Rol?.Descripcion ?? $"Rol ID: {emp.IdRol}"; 
                    string telefono = emp.PersonaInfo?.Telefono.ToString() ?? "No registrado";

                    listaMapeada.Add(new EmpleadoItemViewModel
                    {
                        DniEmpleado = emp.DniEmpleado,
                        NombreCompleto = $"{nombre} {apellido}".Trim(),
                        RolCargo = cargo,
                        Telefono = telefono,
                        Estado = emp.ActivoEnRol ? "Activo" : "Inactivo",
                        PuedeEditar = PuedeEditar
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMPLEADOS DB ERROR] Error al cargar empleados de la BD: {ex.Message}");
            }
        }

        // Si no hay datos en BD o falló la conexión, mostramos datos de demostración
        if (!listaMapeada.Any())
        {
            OrigenDatosTexto = "🟠 Mock";
            OrigenDatosColor = "#E67E22";
            listaMapeada = new List<EmpleadoItemViewModel>
            {
                new EmpleadoItemViewModel { DniEmpleado = 38450192, NombreCompleto = "Hernán Céspedes", RolCargo = "Dueño", Telefono = "11-4455-6677", Estado = "Activo", PuedeEditar = PuedeEditar },
                new EmpleadoItemViewModel { DniEmpleado = 40192834, NombreCompleto = "Giovanni Centurión", RolCargo = "Gerente", Telefono = "11-5566-7788", Estado = "Activo", PuedeEditar = PuedeEditar },
                new EmpleadoItemViewModel { DniEmpleado = 41982345, NombreCompleto = "Carlos Valenzuela", RolCargo = "Mozo", Telefono = "11-6677-8899", Estado = "Activo", PuedeEditar = PuedeEditar },
                new EmpleadoItemViewModel { DniEmpleado = 42834912, NombreCompleto = "Mariana López", RolCargo = "Cajero", Telefono = "11-7788-9900", Estado = "Activo", PuedeEditar = PuedeEditar }
            };
        }
        else
        {
            OrigenDatosTexto = "🟢 Base de datos";
            OrigenDatosColor = "#27AE60";
        }

        Empleados = new ObservableCollection<EmpleadoItemViewModel>(listaMapeada);
    }

}