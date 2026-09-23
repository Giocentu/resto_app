using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.ViewModels;
using RestoApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.Views;

public partial class NuevoEmpleadoWindow : Window
{
    private readonly ClienteService? _clienteService;
    private readonly TurnoService? _turnoService;
    private List<TurnoEmpleado> _listaTurnos = new();
    private long _dniVerificado = 0;
    private bool _esEdicion = false;

    public EmpleadoItemViewModel? EmpleadoResult { get; private set; }

    public NuevoEmpleadoWindow()
    {
        InitializeComponent();
        _clienteService = App.Services?.GetService(typeof(ClienteService)) as ClienteService
            ?? new ClienteService(new ClienteRepository(new RestoAppDbContext()));
        _turnoService = App.Services?.GetService(typeof(TurnoService)) as TurnoService
            ?? new TurnoService(new TurnoRepository(new RestoAppDbContext()));

        _ = CargarTurnosDbAsync();
    }

    private async Task CargarTurnosDbAsync()
    {
        if (_turnoService == null) return;
        try
        {
            var turnos = await _turnoService.ObtenerTurnosAsync(soloActivos: true);
            _listaTurnos = turnos.ToList();
            var items = _listaTurnos.Select(t => $"Turno #{t.IdTurno} ({t.InicioTurno:hh\\:mm} - {t.FinTurno:hh\\:mm})").ToList();
            if (!items.Any())
            {
                items.Add("Turno 1 (19:55 - 01:55)");
            }
            CboTurno.ItemsSource = items;
            CboTurno.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NUEVO EMPLEADO TURNOS DB ERROR] {ex.Message}");
        }
    }

    public NuevoEmpleadoWindow(EmpleadoItemViewModel empleado) : this()
    {
        _esEdicion = true;
        TxtTituloModal.Text = "Editar Empleado";
        TxtDni.Text = empleado.DniEmpleado.ToString();
        _dniVerificado = empleado.DniEmpleado;
        TxtDni.IsEnabled = false; // El DNI es la clave identificadora
        TxtNombre.Text = empleado.NombreCompleto;
        SeleccionarRolEnCombo(empleado.RolCargo);
        TxtTelefono.Text = empleado.Telefono;
        SeleccionarEstadoEnCombo(empleado.Estado);
    }

    private async void TxtDni_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_esEdicion) return;

        string text = TxtDni.Text?.Trim() ?? "";
        if (text.Length >= 7 && long.TryParse(text, out long dni))
        {
            await VerificarDniAsync(dni);
        }
        else
        {
            _dniVerificado = 0;
            TxtNombre.Text = "";
            TxtTelefono.Text = "";
        }
    }

    private async void BtnBuscarDni_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;
        string text = TxtDni.Text?.Trim() ?? "";
        if (!long.TryParse(text, out long dni) || dni <= 0)
        {
            MostrarError("⚠️ Por favor ingrese un número de DNI válido.");
            return;
        }

        await VerificarDniAsync(dni);
    }

    private async Task VerificarDniAsync(long dni)
    {
        if (_clienteService == null) return;

        var persona = await _clienteService.BuscarPersonaPorDniAsync(dni);
        if (persona != null)
        {
            TxtNombre.Text = $"{persona.Nombre} {persona.Apellido}".Trim();
            TxtTelefono.Text = persona.Telefono > 0 ? persona.Telefono.ToString() : "";
            _dniVerificado = persona.Dni;
            TxtError.IsVisible = false;
        }
        else
        {
            _dniVerificado = 0;
            MostrarError("⚠️ Este DNI no existe en el sistema. Por favor registre a la persona primero desde la gestión de Clientes/Personas.");
        }
    }

    private void SeleccionarRolEnCombo(string rol)
    {
        for (int i = 0; i < CboRol.Items.Count; i++)
        {
            if (CboRol.Items[i] is ComboBoxItem item && item.Content?.ToString()?.Equals(rol, StringComparison.OrdinalIgnoreCase) == true)
            {
                CboRol.SelectedIndex = i;
                return;
            }
        }
    }

    private void SeleccionarEstadoEnCombo(string estado)
    {
        for (int i = 0; i < CboEstado.Items.Count; i++)
        {
            if (CboEstado.Items[i] is ComboBoxItem item && item.Content?.ToString()?.Equals(estado, StringComparison.OrdinalIgnoreCase) == true)
            {
                CboEstado.SelectedIndex = i;
                return;
            }
        }
    }

    private async void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtDni.Text) || !long.TryParse(TxtDni.Text.Trim(), out long dni) || dni <= 0)
        {
            MostrarError("⚠️ Por favor ingrese un número de DNI válido (números positivos sin puntos).");
            return;
        }

        if (!_esEdicion && _dniVerificado != dni)
        {
            await VerificarDniAsync(dni);
            if (_dniVerificado <= 0)
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MostrarError("⚠️ La persona correspondiente a este DNI no fue encontrada.");
            return;
        }

        if (CboRol.SelectedItem == null)
        {
            MostrarError("⚠️ Por favor seleccione un cargo/rol asignado para el empleado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtTelefono.Text))
        {
            MostrarError("⚠️ Por favor ingrese el teléfono de contacto del empleado.");
            return;
        }

        string rolSeleccionado = (CboRol.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Mozo";
        string estadoSeleccionado = (CboEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activo";

        EmpleadoResult = new EmpleadoItemViewModel
        {
            DniEmpleado = dni,
            NombreCompleto = TxtNombre.Text.Trim(),
            RolCargo = rolSeleccionado,
            Telefono = TxtTelefono.Text.Trim(),
            Estado = estadoSeleccionado
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


