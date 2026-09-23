using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestoApp.Desktop.Views;

public partial class GestionTurnosWindow : Window
{
    private readonly TurnoService _turnoService;
    private List<TurnoItemViewModel> _listaTurnos = new();

    public GestionTurnosWindow()
    {
        InitializeComponent();
        _turnoService = App.Services?.GetService(typeof(TurnoService)) as TurnoService
            ?? new TurnoService(new TurnoRepository(new RestoAppDbContext()));

        TpHoraInicio.SelectedTime = new TimeSpan(16, 0, 0);
        TpHoraFin.SelectedTime = new TimeSpan(0, 0, 0);

        _ = CargarTurnosAsync();
    }

    private async Task CargarTurnosAsync()
    {
        try
        {
            var turnosDb = await _turnoService.ObtenerTurnosAsync();
            _listaTurnos = turnosDb.Select(t => new TurnoItemViewModel
            {
                IdTurno = t.IdTurno,
                InicioTexto = t.InicioTurno.ToString(@"hh\:mm"),
                FinTexto = t.FinTurno.ToString(@"hh\:mm"),
                EsActivo = t.EsActivo
            }).ToList();

            DgTurnos.ItemsSource = _listaTurnos;
        }
        catch (Exception ex)
        {
            MostrarError($"⚠️ Error al cargar los turnos de la base de datos: {ex.Message}");
        }
    }

    private async void BtnCrearTurno_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (!TpHoraInicio.SelectedTime.HasValue)
        {
            MostrarError("⚠️ Seleccione la hora de inicio del turno.");
            return;
        }

        if (!TpHoraFin.SelectedTime.HasValue)
        {
            MostrarError("⚠️ Seleccione la hora de fin del turno.");
            return;
        }

        TimeSpan inicio = TpHoraInicio.SelectedTime.Value;
        TimeSpan fin = TpHoraFin.SelectedTime.Value;

        try
        {
            await _turnoService.CrearTurnoAsync(inicio, fin);
            await CargarTurnosAsync();
        }
        catch (Exception ex)
        {
            MostrarError($"⚠️ No se pudo guardar el nuevo turno: {ex.Message}");
        }
    }

    private async void BtnToggleEstadoTurno_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (sender is Button btn && btn.DataContext is TurnoItemViewModel turno)
        {
            try
            {
                bool nuevoEstado = !turno.EsActivo;
                await _turnoService.CambiarEstadoTurnoAsync(turno.IdTurno, nuevoEstado);
                await CargarTurnosAsync();
            }
            catch (Exception ex)
            {
                MostrarError($"⚠️ No se pudo cambiar el estado del turno #{turno.IdTurno}: {ex.Message}");
            }
        }
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        TxtError.IsVisible = true;
    }

    private void BtnCerrar_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
