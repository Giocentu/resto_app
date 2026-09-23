using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestoApp.Desktop.Views;

public partial class NuevoClienteWindow : Window
{
    private readonly ClienteService? _clienteService;
    private readonly bool _esEdicion = false;
    public ClienteItemViewModel? ClienteResult { get; private set; }

    public NuevoClienteWindow()
    {
        InitializeComponent();
        _clienteService = App.Services?.GetService(typeof(ClienteService)) as ClienteService
            ?? new ClienteService(new ClienteRepository(new RestoAppDbContext()));
    }

    public NuevoClienteWindow(ClienteItemViewModel cliente) : this()
    {
        _esEdicion = true;
        TxtTituloModal.Text = "Editar Cliente";
        TxtDni.Text = cliente.DniCliente.ToString();
        TxtDni.IsReadOnly = true;
        TxtDni.Background = Avalonia.Media.Brushes.LightGray;
        TxtNombre.Text = cliente.Nombre;
        TxtApellido.Text = cliente.Apellido;
        TxtEmail.Text = cliente.Email;
        TxtTelefono.Text = cliente.Telefono.ToString();
    }

    private async void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (!long.TryParse(TxtDni.Text, out long dni) || dni <= 0)
        {
            MostrarError("⚠️ Ingrese un número de DNI válido (numérico mayor a 0).");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MostrarError("⚠️ Por favor ingrese el nombre del cliente.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtApellido.Text))
        {
            MostrarError("⚠️ Por favor ingrese el apellido del cliente.");
            return;
        }

        string email = TxtEmail.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            MostrarError("⚠️ Ingrese un correo electrónico válido (formato ej: usuario@correo.com).");
            return;
        }

        if (!long.TryParse(TxtTelefono.Text, out long telefono) || telefono <= 0)
        {
            MostrarError("⚠️ Ingrese un número de teléfono válido.");
            return;
        }

        if (!_esEdicion && _clienteService != null)
        {
            var personaExistente = await _clienteService.BuscarPersonaPorDniAsync(dni);
            var clienteExistente = await _clienteService.BuscarClientePorDniAsync(dni);
            if (clienteExistente != null)
            {
                MostrarError("⚠️ Ya existe un cliente registrado activamente con este número de DNI.");
                return;
            }
        }

        if (_clienteService != null)
        {
            try
            {
                await _clienteService.GuardarClienteAsync(dni, TxtNombre.Text.Trim(), TxtApellido.Text.Trim(), email, telefono);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENTE GUARDAR ERROR] {ex.Message}");
                MostrarError($"⚠️ Error al guardar cliente: {ex.Message}");
                return;
            }
        }

        ClienteResult = new ClienteItemViewModel
        {
            DniCliente = dni,
            Nombre = TxtNombre.Text.Trim(),
            Apellido = TxtApellido.Text.Trim(),
            Email = email,
            Telefono = telefono,
            EsActivo = true
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
