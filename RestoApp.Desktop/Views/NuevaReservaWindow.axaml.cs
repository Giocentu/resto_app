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
    private readonly ClienteService? _clienteService;
    private List<Mesa> _listaMesas = new();
    private List<Evento> _listaEventos = new();
    private long _dniClienteVerificado = 0;

    public ReservaItemViewModel? ReservaResult { get; private set; }

    public NuevaReservaWindow()
    {
        InitializeComponent();
        _mesaService = App.Services?.GetService(typeof(MesaService)) as MesaService
            ?? new MesaService(new MesaRepository(new RestoAppDbContext()));
        _eventoService = App.Services?.GetService(typeof(EventoService)) as EventoService
            ?? new EventoService(new EventoRepository(new RestoAppDbContext()));
        _clienteService = App.Services?.GetService(typeof(ClienteService)) as ClienteService
            ?? new ClienteService(new ClienteRepository(new RestoAppDbContext()));

        DateTime dtInicial = DateTime.Now.AddHours(2);
        DpFecha.SelectedDate = dtInicial;
        TpHora.SelectedTime = dtInicial.TimeOfDay;

        _ = CargarDatosDbAsync();
    }

    public NuevaReservaWindow(ReservaItemViewModel reserva) : this()
    {
        _idReservaExistente = reserva.IdReserva;
        TxtTituloModal.Text = "Editar Reserva";
        TxtDni.Text = reserva.DniCliente > 0 ? reserva.DniCliente.ToString() : "";
        _dniClienteVerificado = reserva.DniCliente;
        TxtCliente.Text = reserva.ClienteNombre;
        TxtPersonas.Text = reserva.CantidadPersonas.ToString();

        if (TryParseFechaHora(reserva.FechaHora, out DateTime dtParsed))
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

    private async void TxtDni_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string text = TxtDni.Text?.Trim() ?? "";
        if (text.Length >= 7 && long.TryParse(text, out long dni))
        {
            await VerificarDniAsync(dni);
        }
        else
        {
            _dniClienteVerificado = 0;
            TxtCliente.Text = "";
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
            TxtCliente.Text = $"{persona.Nombre} {persona.Apellido}".Trim();
            _dniClienteVerificado = persona.Dni;
            TxtError.IsVisible = false;
        }
        else
        {
            TxtCliente.Text = "";
            _dniClienteVerificado = 0;
            MostrarError("⚠️ Este DNI no existe en el sistema. Por favor registre al cliente primero desde la gestión de Clientes.");
        }
    }

    private async void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (!long.TryParse(TxtDni.Text, out long dni) || dni <= 0)
        {
            MostrarError("⚠️ Por favor ingrese el número de DNI del cliente.");
            return;
        }

        if (_dniClienteVerificado != dni)
        {
            await VerificarDniAsync(dni);
            if (_dniClienteVerificado <= 0)
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(TxtCliente.Text))
        {
            MostrarError("⚠️ El cliente correspondiente a este DNI no fue encontrado.");
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
        int idMesaSeleccionada = 0;
        if (CboMesa.SelectedIndex >= 0 && CboMesa.SelectedIndex < _listaMesas.Count)
        {
            nroMesaSeleccionada = _listaMesas[CboMesa.SelectedIndex].NroMesa.ToString();
            idMesaSeleccionada = _listaMesas[CboMesa.SelectedIndex].IdMesa;
        }

        int? idEventoSeleccionado = null;
        if (CboEvento.SelectedIndex > 0 && (CboEvento.SelectedIndex - 1) < _listaEventos.Count)
        {
            idEventoSeleccionado = _listaEventos[CboEvento.SelectedIndex - 1].IdEvento;
        }

        ReservaResult = new ReservaItemViewModel
        {
            IdReserva = _idReservaExistente,
            ClienteNombre = TxtCliente.Text.Trim(),
            DniCliente = _dniClienteVerificado,
            FechaHora = fechaHoraCombinada.ToString("dd/MM/yyyy HH:mm"),
            IdMesa = idMesaSeleccionada,
            NroMesa = nroMesaSeleccionada,
            IdEvento = idEventoSeleccionado,
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

    private static bool TryParseFechaHora(string? texto, out DateTime dt)
    {
        dt = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(texto)) return false;

        string[] formats = new[]
        {
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HH:mm:ss",
            "d/M/yyyy HH:mm",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "g",
            "G"
        };

        if (DateTime.TryParseExact(texto, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            return true;

        if (DateTime.TryParse(texto, System.Globalization.CultureInfo.GetCultureInfo("es-AR"), System.Globalization.DateTimeStyles.None, out dt))
            return true;

        return DateTime.TryParse(texto, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt);
    }
}


