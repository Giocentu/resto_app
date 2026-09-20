using CommunityToolkit.Mvvm.ComponentModel;

namespace RestoApp.Desktop.ViewModels;

public partial class CuentaPendienteViewModel : ObservableObject
{
    public int IdMesa { get; set; }
    public int NroMesa { get; set; }
    public string Sector { get; set; } = "Salón Principal";
    public string ClienteNombre { get; set; } = "Cliente Particular";
    public decimal MontoConsumo { get; set; }

    public string DisplayCombo => $"Mesa #{NroMesa} ({Sector}) - {ClienteNombre} - $ {MontoConsumo:N2}";
}
