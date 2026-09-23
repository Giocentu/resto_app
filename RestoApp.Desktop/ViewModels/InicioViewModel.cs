using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Desktop.Services;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class InicioViewModel : ObservableObject
{
    private readonly MesaService? _mesaService;
    private readonly UbicacionService? _ubicacionService;
    private readonly Action? _navigateAMesasAction;

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesas = new();

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesasFiltradas = new();

    [ObservableProperty]
    private ObservableCollection<string> _ubicaciones = new() { "Todas" };

    [ObservableProperty]
    private ObservableCollection<string> _sectores = new() { "Todas" };

    [ObservableProperty]
    private VisualMesaItemViewModel? _selectedMesa;

    [ObservableProperty]
    private string _ubicacionSeleccionada = "Todas";

    [ObservableProperty]
    private string _sectorSeleccionado = "Todas";

    [ObservableProperty]
    private int _libresCount;

    [ObservableProperty]
    private int _reservadasCount;

    [ObservableProperty]
    private int _ocupadasCount;

    [ObservableProperty]
    private int _limpiezaCount;

    public InicioViewModel(MesaService? mesaService = null, Action? navigateAMesasAction = null, UbicacionService? ubicacionService = null)
    {
        _mesaService = mesaService;
        _navigateAMesasAction = navigateAMesasAction;
        _ubicacionService = ubicacionService ?? App.Services?.GetService(typeof(UbicacionService)) as UbicacionService;
        _ = CargarMesasAsync();
    }

    public async Task CargarMesasAsync()
    {
        var lista = new List<VisualMesaItemViewModel>();

        if (_mesaService != null)
        {
            try
            {
                var entidades = await _mesaService.ObtenerMesasAsync(soloActivas: true);
                foreach (var m in entidades)
                {
                    lista.Add(new VisualMesaItemViewModel
                    {
                        IdMesa = m.IdMesa,
                        NroMesa = m.NroMesa,
                        Capacidad = m.Capacidad,
                        IdUbicacion = m.IdUbicacion,
                        UbicacionDescripcion = m.Ubicacion?.Ubicacion ?? "Salón Principal",
                        Estado = string.IsNullOrWhiteSpace(m.Estado) ? "LIBRE" : m.Estado
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INICIO DB ERROR] Error al cargar mesas: {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }
        }
        else
        {
            _ = AlertaService.MostrarAlertaConexionAsync();
        }

        Mesas = new ObservableCollection<VisualMesaItemViewModel>(lista);

        var listaUbicaciones = new List<string> { "Todas" };
        if (_ubicacionService != null)
        {
            try
            {
                var uList = await _ubicacionService.ObtenerUbicacionesAsync(soloActivas: true);
                foreach (var u in uList)
                {
                    if (!string.IsNullOrWhiteSpace(u.Ubicacion) && !listaUbicaciones.Contains(u.Ubicacion))
                    {
                        listaUbicaciones.Add(u.Ubicacion);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INICIO UBICACIONES DB ERROR] {ex.Message}");
            }
        }
        foreach (var m in Mesas)
        {
            if (!string.IsNullOrWhiteSpace(m.UbicacionDescripcion) && !listaUbicaciones.Contains(m.UbicacionDescripcion))
            {
                listaUbicaciones.Add(m.UbicacionDescripcion);
            }
        }

        Ubicaciones = new ObservableCollection<string>(listaUbicaciones.Distinct());
        Sectores = Ubicaciones;

        ActualizarConteos();
        AplicarFiltroUbicacion();

        if (MesasFiltradas.Any())
        {
            SeleccionarMesa(MesasFiltradas.First());
        }
        else
        {
            SelectedMesa = null;
        }
    }

    private void ActualizarConteos()
    {
        LibresCount = Mesas.Count(m => string.Equals(m.Estado, "LIBRE", StringComparison.OrdinalIgnoreCase));
        ReservadasCount = Mesas.Count(m => string.Equals(m.Estado, "RESERVADA", StringComparison.OrdinalIgnoreCase));
        OcupadasCount = Mesas.Count(m => string.Equals(m.Estado, "OCUPADA", StringComparison.OrdinalIgnoreCase));
        LimpiezaCount = Mesas.Count(m => string.Equals(m.Estado, "EN LIMPIEZA", StringComparison.OrdinalIgnoreCase));
    }

    [RelayCommand]
    private void FiltrarPorUbicacion(string ubicacion)
    {
        UbicacionSeleccionada = ubicacion;
        SectorSeleccionado = ubicacion;
        AplicarFiltroUbicacion();
    }

    [RelayCommand]
    private void FiltrarPorSector(string sector) => FiltrarPorUbicacion(sector);

    private void AplicarFiltroUbicacion()
    {
        if (string.IsNullOrEmpty(UbicacionSeleccionada) || UbicacionSeleccionada == "Todas" || UbicacionSeleccionada == "Todos")
        {
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(Mesas);
        }
        else
        {
            var filtradas = Mesas.Where(m => string.Equals(m.UbicacionDescripcion, UbicacionSeleccionada, StringComparison.OrdinalIgnoreCase));
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(filtradas);
        }
    }

    [RelayCommand]
    private void SeleccionarMesa(VisualMesaItemViewModel? mesa)
    {
        if (mesa == null) return;
        foreach (var m in Mesas)
        {
            m.IsSelected = false;
        }
        mesa.IsSelected = true;
        SelectedMesa = mesa;
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync(string nuevoEstado)
    {
        if (SelectedMesa != null)
        {
            var estadoAnterior = SelectedMesa.Estado;
            SelectedMesa.Estado = nuevoEstado;

            if (_mesaService != null && SelectedMesa.IdMesa > 0)
            {
                try
                {
                    int idUbicacion = SelectedMesa.IdUbicacion > 0 ? SelectedMesa.IdUbicacion : 1;
                    await _mesaService.EditarMesaAsync(
                        SelectedMesa.IdMesa,
                        SelectedMesa.NroMesa,
                        SelectedMesa.Capacidad,
                        idUbicacion,
                        nuevoEstado
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CAMBIAR ESTADO DB ERROR] {ex.Message}");
                    SelectedMesa.Estado = estadoAnterior;
                    _ = AlertaService.MostrarAlertaConexionAsync();
                }
            }

            ActualizarConteos();
            OnPropertyChanged(nameof(SelectedMesa));
        }
    }

    [RelayCommand]
    private void IrAMesas()
    {
        _navigateAMesasAction?.Invoke();
    }
}
