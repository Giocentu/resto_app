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
    public bool PuedeAgregar => SesionGlobal.RolActual == RolUsuario.Dueno;
    public bool PuedeEditar => SesionGlobal.RolActual == RolUsuario.Dueno
                        || SesionGlobal.RolActual == RolUsuario.Gerente
                        || SesionGlobal.RolActual == RolUsuario.Recepcion
                        || SesionGlobal.RolActual == RolUsuario.Cajero;

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
            // Extraemos los datos navegando hacia las entidades (asumiendo que Persona tiene Nombre/Apellido/Telefono)
            string nombre = emp.PersonaInfo?.Nombre ?? "Sin Nombre";
            string apellido = emp.PersonaInfo?.Apellido ?? "";
            
            
            string cargo = emp.Rol?.Descripcion ?? $"Rol ID: {emp.IdRol}"; 
            string telefono = emp.PersonaInfo?.Telefono ?? "No registrado";

            listaMapeada.Add(new EmpleadoItemViewModel
            {
                DniEmpleado = emp.DniEmpleado, // Usamos tu propiedad DniEmpleado
                NombreCompleto = $"{nombre} {apellido}".Trim(),
                RolCargo = cargo,
                Telefono = telefono,
                Estado = emp.ActivoEnRol ? "Activo" : "Inactivo", // Usamos tu propiedad ActivoEnRol
                PuedeEditar = PuedeEditar
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