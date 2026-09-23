using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;

namespace RestoApp.Desktop.ViewModels;

public partial class VisualMesaItemViewModel : ObservableObject
{
    public int IdMesa { get; set; }
    public int NroMesa { get; set; }
    public int Capacidad { get; set; }
    public int IdUbicacion { get; set; }
    public string UbicacionDescripcion { get; set; } = string.Empty;

    [ObservableProperty]
    private string _estado = "LIBRE"; // LIBRE, OCUPADA, RESERVADA, EN LIMPIEZA

    [ObservableProperty]
    private string _clienteNombre = string.Empty;

    [ObservableProperty]
    private string _mozoNombre = string.Empty;

    [ObservableProperty]
    private decimal _consumoActual = 0.00m;

    [ObservableProperty]
    private bool _isSelected = false;

    public string ConsumoTexto => $"$ {ConsumoActual:N2}";

    public string BannerText => Estado.ToUpper();

    // Paleta de colores claros legibles
    public IBrush EstadoHeaderBackground => Estado.ToUpper() switch
    {
        "LIBRE" => SolidColorBrush.Parse("#2ECC71"),
        "OCUPADA" => SolidColorBrush.Parse("#F39C12"),
        "RESERVADA" => SolidColorBrush.Parse("#3498DB"),
        "EN LIMPIEZA" => SolidColorBrush.Parse("#E74C3C"),
        _ => SolidColorBrush.Parse("#95A5A6")
    };

    public IBrush EstadoCardBorder => Estado.ToUpper() switch
    {
        "LIBRE" => SolidColorBrush.Parse("#27AE60"),
        "OCUPADA" => SolidColorBrush.Parse("#D68910"),
        "RESERVADA" => SolidColorBrush.Parse("#2980B9"),
        "EN LIMPIEZA" => SolidColorBrush.Parse("#C0392B"),
        _ => SolidColorBrush.Parse("#BDC3C7")
    };

    public IBrush EstadoLightBackground => Estado.ToUpper() switch
    {
        "LIBRE" => SolidColorBrush.Parse("#F0FDF4"),
        "OCUPADA" => SolidColorBrush.Parse("#FFFBEB"),
        "RESERVADA" => SolidColorBrush.Parse("#EFF6FF"),
        "EN LIMPIEZA" => SolidColorBrush.Parse("#FEF2F2"),
        _ => SolidColorBrush.Parse("#F8FAFC")
    };

    partial void OnEstadoChanged(string value)
    {
        OnPropertyChanged(nameof(BannerText));
        OnPropertyChanged(nameof(EstadoHeaderBackground));
        OnPropertyChanged(nameof(EstadoCardBorder));
        OnPropertyChanged(nameof(EstadoLightBackground));
    }
}
