using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RestoApp.Desktop.ViewModels;

public partial class CuentaPendienteViewModel : ObservableObject
{
    public int IdMesa { get; set; }
    public int NroMesa { get; set; }
    public string Sector { get; set; } = "Salón Principal";
    public string ClienteNombre { get; set; } = "Cliente Particular";
    [ObservableProperty]
    private decimal _montoConsumo;

    public int CuentaId { get; set; }
    public int CantidadItems { get; set; }
    public DateTime FechaApertura { get; set; }

    [ObservableProperty]
    private string _atendidoPor = "Mozo";

    partial void OnMontoConsumoChanged(decimal value)
    {
        OnPropertyChanged(nameof(MontoConsumoTexto));
        OnPropertyChanged(nameof(DisplayCombo));
    }

    public string HoraApertura => FechaApertura != default ? FechaApertura.ToString("HH:mm") : "--:--";
    public string MontoConsumoTexto => $"$ {MontoConsumo:N2}";
    public string DisplayCombo => $"Mesa #{NroMesa} ({Sector}) - {ClienteNombre} - $ {MontoConsumo:N2}";
}
