using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
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

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GESTION CLIENTES ERROR] {ex.Message}");
        }
    }

    private void AplicarFiltro()
    {
        string text = TxtBusqueda.Text?.Trim().ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(text))
        {
            GridClientes.ItemsSource = new ObservableCollection<ClienteItemViewModel>(_todosLosClientes);
        }
        else
        {
            var filtrados = _todosLosClientes.Where(c =>
                c.DniCliente.ToString().Contains(text) ||
                c.NombreCompleto.ToLower().Contains(text) ||
                c.Email.ToLower().Contains(text)
            ).ToList();

            GridClientes.ItemsSource = new ObservableCollection<ClienteItemViewModel>(filtrados);
        }
    }

    private void TxtBusqueda_TextChanged(object? sender, TextChangedEventArgs e)
    {
        AplicarFiltro();
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
            }
        }
    }

    private void BtnCerrar_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
