using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;

namespace RestoApp.Desktop.Views;

public partial class NuevoEmpleadoWindow : Window
{
    public EmpleadoItemViewModel? EmpleadoResult { get; private set; }

    public NuevoEmpleadoWindow()
    {
        InitializeComponent();
    }

    public NuevoEmpleadoWindow(EmpleadoItemViewModel empleado)
    {
        InitializeComponent();
        TxtTituloModal.Text = "Editar Empleado";
        TxtDni.Text = empleado.DniEmpleado.ToString();
        TxtDni.IsEnabled = false; // El DNI es la clave identificadora
        TxtNombre.Text = empleado.NombreCompleto;
        SeleccionarRolEnCombo(empleado.RolCargo);
        TxtTelefono.Text = empleado.Telefono;
        SeleccionarEstadoEnCombo(empleado.Estado);
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

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtDni.Text) || !long.TryParse(TxtDni.Text.Trim(), out long dni) || dni <= 0)
        {
            MostrarError("⚠️ Por favor ingrese un número de DNI válido (números positivos sin puntos).");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MostrarError("⚠️ Por favor ingrese el nombre y apellido completo del empleado.");
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


