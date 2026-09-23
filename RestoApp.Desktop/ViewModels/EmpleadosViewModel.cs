using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Desktop.Services;
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
    private ObservableCollection<EmpleadoItemViewModel> _empleadosFiltrados = new();

    [ObservableProperty]
    private ObservableCollection<EmpleadoItemViewModel> _empleadosBajas = new();

    [ObservableProperty]
    private bool _esVistaPrincipal = true;

    [ObservableProperty]
    private bool _esVistaBajas = false;

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private string _filtroRol = "Todos";

    [ObservableProperty]
    private int _totalEmpleadosCount;

    [ObservableProperty]
    private int _activosCount;

    [ObservableProperty]
    private int _mozosCount;

    [ObservableProperty]
    private int _inactivosCount;

    partial void OnTextoBusquedaChanged(string value) => AplicarFiltro();
    partial void OnFiltroRolChanged(string value) => AplicarFiltro();

    [RelayCommand]
    private void VerBajas()
    {
        EsVistaPrincipal = false;
        EsVistaBajas = true;
        _ = CargarEmpleadosBajasAsync();
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
            empleado.Estado = "Activo";
            EmpleadosBajas.Remove(empleado);
            Empleados.Add(empleado);
            AplicarFiltro();
        }
    }

    private async Task CargarEmpleadosBajasAsync()
    {
        if (_empleadoService != null)
        {
            try
            {
                var inactivos = await _empleadoService.ObtenerEmpleadosAsync(soloActivos: false);
                var listaBajas = new List<EmpleadoItemViewModel>();
                foreach (var emp in inactivos.Where(e => !e.ActivoEnRol))
                {
                    string nombre = emp.PersonaInfo?.Nombre ?? "Empleado";
                    string apellido = emp.PersonaInfo?.Apellido ?? "";
                    string cargo = emp.Rol?.Descripcion ?? $"Rol ID: {emp.IdRol}";
                    string telefono = emp.PersonaInfo?.Telefono.ToString() ?? "No registrado";

                    listaBajas.Add(new EmpleadoItemViewModel
                    {
                        DniEmpleado = emp.DniEmpleado,
                        NombreCompleto = $"{nombre} {apellido}".Trim(),
                        RolCargo = cargo,
                        Telefono = telefono,
                        Estado = "Inactivo",
                        PuedeEditar = PuedeEditar
                    });
                }
                EmpleadosBajas = new ObservableCollection<EmpleadoItemViewModel>(listaBajas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMPLEADOS BAJAS DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        InactivosCount = EmpleadosBajas.Count;
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
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMPLEADOS DB ERROR] {ex.Message}");
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }
            Empleados.Remove(empleado);
            empleado.Estado = "Inactivo";
            EmpleadosBajas.Add(empleado);
            AplicarFiltro();
        }
    }

    public async Task GuardarEmpleadoAsync(EmpleadoItemViewModel emp)
    {
        if (emp == null) return;

        emp.PuedeEditar = PuedeEditar;

        int idRol = emp.RolCargo?.ToLower() switch
        {
            "dueño" or "dueno" => 1,
            "gerente" => 2,
            "cm" => 3,
            "cajero" => 4,
            "mozo" => 5,
            "recepcion" => 6,
            _ => 5
        };

        if (_empleadoService != null)
        {
            try
            {
                var partesNombre = emp.NombreCompleto.Trim().Split(' ', 2);
                string nombre = partesNombre.Length > 0 ? partesNombre[0] : emp.NombreCompleto;
                string apellido = partesNombre.Length > 1 ? partesNombre[1] : "";
                string email = $"{nombre.ToLower()}@restoapp.com";
                long.TryParse(emp.Telefono.Replace("-", "").Replace(" ", ""), out long tel);

                var listaActual = await _empleadoService.ObtenerEmpleadosAsync(soloActivos: false);
                var existente = listaActual.FirstOrDefault(e => e.DniEmpleado == emp.DniEmpleado);

                if (existente != null)
                {
                    await _empleadoService.EditarEmpleadoAsync(emp.DniEmpleado, nombre, apellido, email, tel, idRol);
                }
                else
                {
                    await _empleadoService.CrearEmpleadoAsync(emp.DniEmpleado, nombre, apellido, email, tel, "123456", idRol, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMPLEADOS GUARDAR DB ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }

        await CargarEmpleadosAsync();
    }

    public void AgregarOActualizarEmpleado(EmpleadoItemViewModel emp)
    {
        _ = GuardarEmpleadoAsync(emp);
    }

    [RelayCommand]
    private void FiltrarPorRolCommand(string rol)
    {
        FiltroRol = rol ?? "Todos";
    }

    private void AplicarFiltro()
    {
        IEnumerable<EmpleadoItemViewModel> resultado = Empleados;

        if (!string.IsNullOrWhiteSpace(FiltroRol) && FiltroRol != "Todos")
        {
            resultado = resultado.Where(e => e.RolCargo.Equals(FiltroRol, StringComparison.OrdinalIgnoreCase) || e.RolCargo.ToLower().Contains(FiltroRol.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            var q = TextoBusqueda.ToLower().Trim();
            resultado = resultado.Where(e => 
                e.NombreCompleto.ToLower().Contains(q) ||
                e.DniEmpleado.ToString().Contains(q) ||
                e.RolCargo.ToLower().Contains(q) ||
                e.Telefono.ToLower().Contains(q)
            );
        }

        EmpleadosFiltrados = new ObservableCollection<EmpleadoItemViewModel>(resultado);
        ActualizarEstadisticas();
    }

    private void ActualizarEstadisticas()
    {
        TotalEmpleadosCount = Empleados.Count;
        ActivosCount = Empleados.Count(e => e.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase));
        MozosCount = Empleados.Count(e => e.RolCargo.ToLower().Contains("mozo"));
        InactivosCount = EmpleadosBajas.Count;
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
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Empleados = new ObservableCollection<EmpleadoItemViewModel>(listaMapeada);
        AplicarFiltro();
    }
}