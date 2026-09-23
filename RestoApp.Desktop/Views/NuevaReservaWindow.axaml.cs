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

public partial class NuevaReservaWindow : Window
{
    private readonly int _idReservaExistente = 0;
    private readonly MesaService? _mesaService;
    private readonly EventoService? _eventoService;
    private List<Mesa> _listaMesas = new();
    private List<Evento> _listaEventos = new();

    public ReservaItemViewModel? ReservaResult { get; private set; }

    public NuevaReservaWindow()
    {
        InitializeComponent();
        _mesaService = App.Services?.GetService(typeof(MesaService)) as MesaService
            ?? new MesaService(new MesaRepository(new RestoAppDbContext()));
        _eventoService = App.Services?.GetService(typeof(EventoService)) as EventoService
            ?? new EventoService(new EventoRepository(new RestoAppDbContext()));

        DateTime dtInicial = DateTime.Now.AddHours(2);
        DpFecha.SelectedDate = dtInicial;
        TpHora.SelectedTime = dtInicial.TimeOfDay;

        _ = CargarDatosDbAsync();
    }

    public NuevaReservaWindow(ReservaItemViewModel reserva) : this()
    {
        _idReservaExistente = reserva.IdReserva;
        TxtTituloModal.Text = "Editar Reserva";
        TxtCliente.Text = reserva.ClienteNombre;
        TxtPersonas.Text = reserva.CantidadPersonas.ToString();

        if (DateTime.TryParse(reserva.FechaHora, out DateTime dtParsed))
        {
            DpFecha.SelectedDate = dtParsed;
            TpHora.SelectedTime = dtParsed.TimeOfDay;
        }

        _ = CargarDatosDbAsync(reserva.NroMesa);
    }

    private async Task CargarDatosDbAsync(string? mesaSeleccionadaTexto = null)
    {
        try
        {
            if (_mesaService != null)
            {
                var mesas = await _mesaService.ObtenerMesasAsync(soloActivas: true);
                _listaMesas = mesas.ToList();

                var itemsMesa = _listaMesas.Select(m => 
                    $"Mesa #{m.NroMesa} ({m.Ubicacion?.Ubicacion ?? "Salón"} - Cap. {m.Capacidad})"
                ).ToList();

                CboMesa.ItemsSource = itemsMesa;
                if (itemsMesa.Any())
                {
                    int index = 0;
                    if (!string.IsNullOrWhiteSpace(mesaSeleccionadaTexto))
                    {
                        for (int i = 0; i < _listaMesas.Count; i++)
                        {
                            if (_listaMesas[i].NroMesa.ToString() == mesaSeleccionadaTexto || itemsMesa[i].Contains($"Mesa #{mesaSeleccionadaTexto}"))
                            {
                                index = i;
                                break;
                            }
                        }
                    }
                    CboMesa.SelectedIndex = index;
                }
            }

            if (_eventoService != null)
            {
                var eventos = await _eventoService.ObtenerEventosAsync(soloActivos: true);
                _listaEventos = eventos.ToList();

                var itemsEvento = new List<string> { "Sin evento especial" };
                itemsEvento.AddRange(_listaEventos.Select(e => e.NombreEvento));

                CboEvento.ItemsSource = itemsEvento;
                CboEvento.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NUEVA RESERVA DB ERROR] {ex.Message}");
        }
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtCliente.Text))
        {
            MostrarError("⚠️ Por favor ingrese el nombre del cliente o empresa.");
            return;
        }

        if (!DpFecha.SelectedDate.HasValue)
        {
            MostrarError("⚠️ Por favor seleccione una fecha válida para la reserva.");
            return;
        }

        if (!TpHora.SelectedTime.HasValue)
        {
            MostrarError("⚠️ Por favor seleccione la hora de la reserva.");
            return;
        }

        if (CboMesa.SelectedIndex < 0 || CboMesa.SelectedItem == null)
        {
            MostrarError("⚠️ Por favor seleccione una mesa asignada de la lista.");
            return;
        }

        if (!int.TryParse(TxtPersonas.Text, out int cantPersonas) || cantPersonas <= 0)
        {
            MostrarError("⚠️ Ingrese una cantidad válida de comensales (número positivo mayor a 0).");
            return;
        }

        DateTime fecha = DpFecha.SelectedDate.Value.DateTime;
        TimeSpan hora = TpHora.SelectedTime.Value;
        DateTime fechaHoraCombinada = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora.Hours, hora.Minutes, 0);

        if (fechaHoraCombinada < DateTime.Now.AddMinutes(-5))
        {
            MostrarError("⚠️ La fecha y hora de la reserva no pueden estar en el pasado.");
            return;
        }

        string nroMesaSeleccionada = "1";
        if (CboMesa.SelectedIndex >= 0 && CboMesa.SelectedIndex < _listaMesas.Count)
        {
            nroMesaSeleccionada = _listaMesas[CboMesa.SelectedIndex].NroMesa.ToString();
        }

        ReservaResult = new ReservaItemViewModel
        {
            IdReserva = _idReservaExistente,
            ClienteNombre = TxtCliente.Text.Trim(),
            FechaHora = fechaHoraCombinada.ToString("dd/MM/yyyy HH:mm"),
            NroMesa = nroMesaSeleccionada,
            CantidadPersonas = cantPersonas,
            EstadoTexto = "Confirmada"
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


