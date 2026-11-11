using CommunityToolkit.Mvvm.ComponentModel;

namespace RestoApp.Desktop.ViewModels;

public partial class CarritoItemViewModel : ObservableObject
{
    public int IdItem { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public double PrecioUnitario { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    [NotifyPropertyChangedFor(nameof(SubtotalTexto))]
    [NotifyPropertyChangedFor(nameof(CantidadTexto))]
    private int _cantidad = 1;

    public double Subtotal => Cantidad * PrecioUnitario;
    public string PrecioUnitarioTexto => $"${PrecioUnitario:N2}";
    public string SubtotalTexto => $"${Subtotal:N2}";
    public string CantidadTexto => $"x{Cantidad}";
}

