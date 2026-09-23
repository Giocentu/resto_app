using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using RestoApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestoApp.Desktop.Views;

public partial class NuevaMesaWindow : Window
{
    private readonly int _idMesaExistente = 0;
    public MesaItemViewModel? MesaResult { get; private set; }
    public int SelectedUbicacionId { get; private set; }

    public NuevaMesaWindow() : this((IEnumerable<UbicacionMesa>?)null)
    {
    }

    public NuevaMesaWindow(IEnumerable<UbicacionMesa>? ubicaciones)
    {
        InitializeComponent();
        CargarUbicacionesCombo(ubicaciones, null);
    }

    public NuevaMesaWindow(MesaItemViewModel mesa, IEnumerable<UbicacionMesa>? ubicaciones = null)
    {
        InitializeComponent();
        _idMesaExistente = mesa.IdMesa;
        TxtTituloModal.Text = "Editar Mesa";
        TxtNroMesa.Text = mesa.NroMesa.ToString();
        TxtCapacidad.Text = mesa.Capacidad.ToString();
        CargarUbicacionesCombo(ubicaciones, mesa.UbicacionDescripcion);
    }

    private void CargarUbicacionesCombo(IEnumerable<UbicacionMesa>? ubicaciones, string? ubicacionSeleccionada)
    {
        var lista = ubicaciones?.ToList() ?? new List<UbicacionMesa>
        {
            new UbicacionMesa { IdUbicacion = 1, Ubicacion = "Terraza" },
            new UbicacionMesa { IdUbicacion = 2, Ubicacion = "2doPiso" },
            new UbicacionMesa { IdUbicacion = 3, Ubicacion = "PlantaBaja" },
            new UbicacionMesa { IdUbicacion = 4, Ubicacion = "Patio" }
        };

        CboUbicacion.ItemsSource = lista;

        if (!string.IsNullOrWhiteSpace(ubicacionSeleccionada))
        {
            var sel = lista.FirstOrDefault(u => u.Ubicacion.Equals(ubicacionSeleccionada, StringComparison.OrdinalIgnoreCase));
            if (sel != null)
            {
                CboUbicacion.SelectedItem = sel;
            }
            else if (lista.Any())
            {
                CboUbicacion.SelectedIndex = 0;
            }
        }
        else if (lista.Any())
        {
            CboUbicacion.SelectedIndex = 0;
        }
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtNroMesa.Text) || !int.TryParse(TxtNroMesa.Text, out int nroMesa) || nroMesa <= 0)
        {
            MostrarError("⚠️ Por favor ingrese un número de mesa válido (entero mayor a 0).");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtCapacidad.Text) || !int.TryParse(TxtCapacidad.Text, out int capacidad) || capacidad <= 0)
        {
            MostrarError("⚠️ Por favor ingrese una capacidad de personas válida (mayor a 0).");
            return;
        }

        if (CboUbicacion.SelectedItem is not UbicacionMesa ubicacionSeleccionada)
        {
            MostrarError("⚠️ Por favor seleccione una ubicación para la mesa.");
            return;
        }

        SelectedUbicacionId = ubicacionSeleccionada.IdUbicacion;

        MesaResult = new MesaItemViewModel
        {
            IdMesa = _idMesaExistente,
            NroMesa = nroMesa,
            Capacidad = capacidad,
            UbicacionDescripcion = ubicacionSeleccionada.Ubicacion
        };

        Close(true);
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        TxtError.IsVisible = true;
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}