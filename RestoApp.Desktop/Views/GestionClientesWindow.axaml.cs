using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.Services;
using RestoApp.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.Views;

public partial class GestionClientesWindow : Window
{
    private readonly ClienteService? _clienteService;
    private List<ClienteItemViewModel> _todosLosClientes = new();
    private bool _esVistaBajas;

    public GestionClientesWindow()
    {
        InitializeComponent();
        _clienteService = App.Services?.GetService(typeof(ClienteService)) as ClienteService
            ?? new ClienteService(new ClienteRepository(new RestoAppDbContext()));

        _ = CargarClientesAsync();
    }

    private async Task CargarClientesAsync()
    {
        if (_clienteService == null) return;

        try
        {
            if (!_esVistaBajas)
            {
                TxtTitulo.Text = "Gestión de Clientes";
                TxtSubtitulo.Text = "Búsqueda, modificación y alta de clientes activos del restaurante";
                TxtToggleBajas.Text = "Inactivos";
                IconToggleBajas.Text = "📁";
                BtnToggleBajas.Background = SolidColorBrush.Parse("#34495E");
                BtnNuevoCliente.IsVisible = true;

                var entidades = await _clienteService.ObtenerClientesAsync(soloActivos: true);
                _todosLosClientes = entidades.Select(c => new ClienteItemViewModel
                {
                    DniCliente = c.DniCliente,
                    Nombre = c.PersonaInfo?.Nombre ?? "",
                    Apellido = c.PersonaInfo?.Apellido ?? "",
                    Email = c.PersonaInfo?.Email ?? "",
                    Telefono = c.PersonaInfo?.Telefono ?? 0,
                    EsActivo = true
                }).ToList();
            }
            else
            {
                TxtTitulo.Text = "Historial de Clientes Dados de Baja";
                TxtSubtitulo.Text = "Listado de clientes inactivos con posibilidad de reactivación";
                TxtToggleBajas.Text = "Atrás";
                IconToggleBajas.Text = "⬅️";
                BtnToggleBajas.Background = SolidColorBrush.Parse("#2980B9");
                BtnNuevoCliente.IsVisible = false;

                var inactivos = await _clienteService.ObtenerClientesInactivosAsync();
                _todosLosClientes = inactivos.Select(p => new ClienteItemViewModel
                {
                    DniCliente = p.Dni,
                    Nombre = p.Nombre ?? "",
                    Apellido = p.Apellido ?? "",
                    Email = p.Email ?? "",
                    Telefono = p.Telefono,
                    EsActivo = false
                }).ToList();
            }

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GESTION CLIENTES ERROR] {ex.Message}");
            _ = AlertaService.MostrarAlertaConexionAsync($"Error al cargar clientes: {ex.Message}");
        }
    }

    private void AplicarFiltro()
    {
        string text = TxtBusqueda.Text?.Trim().ToLower() ?? "";
        List<ClienteItemViewModel> filtrados;
        if (string.IsNullOrWhiteSpace(text))
        {
            filtrados = _todosLosClientes;
        }
        else
        {
            filtrados = _todosLosClientes.Where(c =>
                c.DniCliente.ToString().Contains(text) ||
                c.NombreCompleto.ToLower().Contains(text) ||
                c.Email.ToLower().Contains(text)
            ).ToList();
        }

        GridClientes.ItemsSource = new ObservableCollection<ClienteItemViewModel>(filtrados);
        TxtContador.Text = $"{filtrados.Count} clientes";
    }

    private void TxtBusqueda_TextChanged(object? sender, TextChangedEventArgs e)
    {
        AplicarFiltro();
    }

    private async void BtnToggleBajas_Click(object? sender, RoutedEventArgs e)
    {
        _esVistaBajas = !_esVistaBajas;
        TxtBusqueda.Text = string.Empty;
        await CargarClientesAsync();
    }

    private async void BtnNuevoCliente_Click(object? sender, RoutedEventArgs e)
    {
        var win = new NuevoClienteWindow();
        bool? res = await win.ShowDialog<bool?>(this);
        if (res == true)
        {
            await CargarClientesAsync();
        }
    }

    private async void BtnEditar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ClienteItemViewModel cliente)
        {
            var win = new NuevoClienteWindow(cliente);
            bool? res = await win.ShowDialog<bool?>(this);
            if (res == true)
            {
                await CargarClientesAsync();
            }
        }
    }

    private async void BtnBaja_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ClienteItemViewModel cliente && _clienteService != null)
        {
            try
            {
                await _clienteService.BajaLogicaClienteAsync(cliente.DniCliente);
                await CargarClientesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BAJA CLIENTE ERROR] {ex.Message}");
                string msg = ex.InnerException?.Message ?? ex.Message;
                if (msg.Contains("fk_reserva_cliente", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase))
                {
                    _ = AlertaService.MostrarAlertaConexionAsync("No se puede dar de baja al cliente porque tiene reservas asociadas en el sistema.");
                }
                else
                {
                    _ = AlertaService.MostrarAlertaConexionAsync($"No se pudo dar de baja al cliente: {msg}");
                }
            }
        }
    }

    private async void BtnReactivar_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ClienteItemViewModel cliente && _clienteService != null)
        {
            try
            {
                await _clienteService.RestaurarClienteAsync(cliente.DniCliente);
                await CargarClientesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REACTIVAR CLIENTE ERROR] {ex.Message}");
                _ = AlertaService.MostrarAlertaConexionAsync($"Error al reactivar cliente: {ex.Message}");
            }
        }
    }

    private void BtnCerrar_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
