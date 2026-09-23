using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;

namespace RestoApp.Desktop.Views;

public partial class NuevaMesaWindow : Window
{
    private readonly int _idMesaExistente = 0;
    public MesaItemViewModel? MesaResult { get; private set; }

    public NuevaMesaWindow()
    {
        InitializeComponent();
    }

    public NuevaMesaWindow(MesaItemViewModel mesa)
    {
        InitializeComponent();
        _idMesaExistente = mesa.IdMesa;
        TxtNroMesa.Text = mesa.NroMesa.ToString();
        TxtCapacidad.Text = mesa.Capacidad.ToString();
        TxtUbicacion.Text = mesa.UbicacionDescripcion;
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtNroMesa.Text, out int nroMesa) || nroMesa <= 0)
            return;

        if (!int.TryParse(TxtCapacidad.Text, out int capacidad) || capacidad <= 0)
            capacidad = 4;

        string ubicacion = string.IsNullOrWhiteSpace(TxtUbicacion.Text) ? "Terraza" : TxtUbicacion.Text.Trim();

        MesaResult = new MesaItemViewModel
        {
            IdMesa = _idMesaExistente,
            NroMesa = nroMesa,
            Capacidad = capacidad,
            UbicacionDescripcion = ubicacion
        };

        Close(true);
    }
}