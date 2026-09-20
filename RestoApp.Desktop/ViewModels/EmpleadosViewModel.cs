using CommunityToolkit.Mvvm.ComponentModel;
using RestoApp.Business.Services;
using RestoApp.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestoApp.Desktop.ViewModels;

public partial class EmpleadosViewModel : ObservableObject
{
    private readonly EmpleadoService _empleadoService;

    // Regla de negocio: Solo el Admin puede ver botones de crear/editar/eliminar personal
    public bool PuedeAdministrarPersonal => SesionGlobal.RolActual == RolUsuario.Admin;

    [ObservableProperty]
    private ObservableCollection<EmpleadoItemViewModel> _empleados = new();

    public EmpleadosViewModel(EmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
        _ = CargarEmpleadosAsync();
    }

    public async Task CargarEmpleadosAsync()
    {
        try
        {
            var listaEntidades = await _empleadoService.ObtenerEmpleadosAsync();
            var listaMapeada = new ObservableCollection<EmpleadoItemViewModel>();

            foreach (var emp in listaEntidades)
            {
                // Extraemos los datos navegando hacia las entidades relacionadas
                string nombre = emp.PersonaInfo?.Nombre ?? "Sin Nombre";
                string apellido = emp.PersonaInfo?.Apellido ?? "";

                // Asumiendo que el Rol es un Enum en la entidad Empleado. 
                // Si es una tabla, sería algo como emp.Rol?.NombreRol ?? "Sin Rol"
                string cargo = emp.RolEmpleado.ToString(); 

                listaMapeada.Add(new EmpleadoItemViewModel
                {
                    IdEmpleado = emp.DniEmpleado,
                    NombreCompleto = $"{nombre} {apellido}".Trim(),
                    RolCargo = cargo,
                    Telefono = emp.PersonaInfo?.Telefono ?? "No registrado", // Si el teléfono está en PersonaInfo
                    Estado = emp.ActivoEnRol ? "Activo" : "Inactivo",
                    PuedeEditar = PuedeAdministrarPersonal
                });
            }

            Empleados = listaMapeada;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- ERROR AL CARGAR EMPLEADOS ---");
            Console.WriteLine($"Mensaje: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Detalle: {ex.InnerException.Message}");
            Console.WriteLine($"---------------------------------\n");
        }
    }
}