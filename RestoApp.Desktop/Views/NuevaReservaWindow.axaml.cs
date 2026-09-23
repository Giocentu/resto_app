using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using System;

namespace RestoApp.Desktop.Views;

public partial class NuevaReservaWindow : Window
{
    private readonly int _idReservaExistente = 0;

    public ReservaItemViewModel? ReservaResult { get; private set; }

    public NuevaReservaWindow()
    {
        InitializeComponent();
        DateTime dtInicial = DateTime.Now.AddHours(2);
        DpFecha.SelectedDate = dtInicial;
        TpHora.SelectedTime = dtInicial.TimeOfDay;
    }

    public NuevaReservaWindow(ReservaItemViewModel reserva)
    {
        InitializeComponent();
        _idReservaExistente = reserva.IdReserva;
        TxtTituloModal.Text = "Editar Reserva";
        TxtCliente.Text = reserva.ClienteNombre;
        TxtMesa.Text = reserva.NroMesa;
        TxtPersonas.Text = reserva.CantidadPersonas.ToString();

        if (DateTime.TryParse(reserva.FechaHora, out DateTime dtParsed))
        {
            DpFecha.SelectedDate = dtParsed;
            TpHora.SelectedTime = dtParsed.TimeOfDay;
        }
        else
        {
            DpFecha.SelectedDate = DateTime.Now;
            TpHora.SelectedTime = DateTime.Now.TimeOfDay;
        }
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtCliente.Text))
        {
            MostrarError("Por favor ingrese el nombre del cliente o empresa.");
            return;
        }

        if (!DpFecha.SelectedDate.HasValue)
        {
            MostrarError("Por favor seleccione una fecha válida para la reserva.");
            return;
        }

        if (!TpHora.SelectedTime.HasValue)
        {
            MostrarError("Por favor seleccione la hora de la reserva.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtMesa.Text))
        {
            MostrarError("Por favor ingrese la mesa asignada.");
            return;
        }

        if (!int.TryParse(TxtPersonas.Text, out int cantPersonas) || cantPersonas <= 0)
        {
            MostrarError("Ingrese una cantidad válida de personas (mayor a 0).");
            return;
        }

        DateTime fecha = DpFecha.SelectedDate.Value.DateTime;
        TimeSpan hora = TpHora.SelectedTime.Value;
        DateTime fechaHoraCombinada = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora.Hours, hora.Minutes, 0);

        ReservaResult = new ReservaItemViewModel
        {
            IdReserva = _idReservaExistente,
            ClienteNombre = TxtCliente.Text.Trim(),
            FechaHora = fechaHoraCombinada.ToString("dd/MM/yyyy HH:mm"),
            NroMesa = TxtMesa.Text.Trim(),
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


