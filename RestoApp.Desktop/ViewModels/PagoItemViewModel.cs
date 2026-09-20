using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RestoApp.Desktop.ViewModels;

public partial class PagoItemViewModel : ObservableObject
{
    public int IdPago { get; set; }
    public DateTime FechaPago { get; set; }
    public string Hora => FechaPago.ToString("HH:mm:ss");
    public string Descripcion { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string MedioPago { get; set; } = "Efectivo";
    public decimal Monto { get; set; }

    public string MontoTexto => $"$ {Monto:N2}";

    public IBrush MedioPagoBadgeBackground => MedioPago.ToUpper() switch
    {
        var m when m.Contains("EFECTIVO") => SolidColorBrush.Parse("#2ECC71"),
        var m when m.Contains("TARJETA") || m.Contains("DÉBITO") || m.Contains("CRÉDITO") => SolidColorBrush.Parse("#3498DB"),
        var m when m.Contains("TRANSF") || m.Contains("QR") || m.Contains("MERCADO") => SolidColorBrush.Parse("#9B59B6"),
        _ => SolidColorBrush.Parse("#95A5A6")
    };

    public IBrush MedioPagoBadgeForeground => SolidColorBrush.Parse("#FFFFFF");
}
