using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RestoApp.Desktop.ViewModels;

public partial class EventoItemViewModel : ObservableObject
{
    public int IdEvento { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; } = DateTime.Now;
    public string Descripcion { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _esActivo = true;

    [ObservableProperty]
    private int _cantReservasVinculadas;

    public string FechaTexto => $"Fecha: {FechaEvento:dd/MM/yyyy}";
    public string ReservasVinculadasTexto => $"{CantReservasVinculadas} reservas";

    public string EstadoBadgeText => EsActivo ? "Activo" : "Desactivado";

    public IBrush EstadoBadgeBackground => EsActivo 
        ? SolidColorBrush.Parse("#2ECC71") 
        : SolidColorBrush.Parse("#E74C3C");

    public IBrush EstadoBadgeForeground => SolidColorBrush.Parse("#FFFFFF");

    public string BotonEstadoTexto => EsActivo ? "Activo" : "Desactivado";

    public IBrush BotonEstadoBackground => EsActivo 
        ? SolidColorBrush.Parse("#27AE60") 
        : SolidColorBrush.Parse("#E74C3C");

    partial void OnEsActivoChanged(bool value)
    {
        OnPropertyChanged(nameof(EstadoBadgeText));
        OnPropertyChanged(nameof(EstadoBadgeBackground));
        OnPropertyChanged(nameof(EstadoBadgeForeground));
        OnPropertyChanged(nameof(BotonEstadoTexto));
        OnPropertyChanged(nameof(BotonEstadoBackground));
    }

    [RelayCommand]
    private void ToggleEstado()
    {
        EsActivo = !EsActivo;
    }
}
