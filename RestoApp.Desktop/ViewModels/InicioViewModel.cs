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
    private readonly Action? _navigateAMesasAction;

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesas = new();

    [ObservableProperty]
    private ObservableCollection<VisualMesaItemViewModel> _mesasFiltradas = new();

    [ObservableProperty]
    private ObservableCollection<string> _sectores = new() { "Todos" };

    [ObservableProperty]
    private VisualMesaItemViewModel? _selectedMesa;

    [ObservableProperty]
    private string _sectorSeleccionado = "Todos";

    [ObservableProperty]
    private int _libresCount;

    [ObservableProperty]
    private int _reservadasCount;

    [ObservableProperty]
    private int _ocupadasCount;

    [ObservableProperty]
    private int _limpiezaCount;

    public InicioViewModel(MesaService? mesaService = null, Action? navigateAMesasAction = null)
    {
        _mesaService = mesaService;
        _navigateAMesasAction = navigateAMesasAction;
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

        var listaSectores = new List<string> { "Todos" };
        listaSectores.AddRange(Mesas.Select(m => m.UbicacionDescripcion).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct());
        Sectores = new ObservableCollection<string>(listaSectores.Distinct());

        ActualizarConteos();
        AplicarFiltroSector();

        if (MesasFiltradas.Any())
        {
            SeleccionarMesa(MesasFiltradas.First());
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
    private void FiltrarPorSector(string sector)
    {
        SectorSeleccionado = sector;
        AplicarFiltroSector();
    }

    private void AplicarFiltroSector()
    {
        if (SectorSeleccionado == "Todos")
        {
            MesasFiltradas = new ObservableCollection<VisualMesaItemViewModel>(Mesas);
        }
        else
        {
            var filtradas = Mesas.Where(m => string.Equals(m.UbicacionDescripcion, SectorSeleccionado, StringComparison.OrdinalIgnoreCase));
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
    private void CambiarEstado(string nuevoEstado)
    {
        if (SelectedMesa != null)
        {
            SelectedMesa.Estado = nuevoEstado;
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
