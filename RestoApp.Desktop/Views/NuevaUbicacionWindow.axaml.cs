using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Entities;
using System;

namespace RestoApp.Desktop.Views;

public partial class NuevaUbicacionWindow : Window
{
    private readonly int _idUbicacionExistente = 0;
    public UbicacionMesa? UbicacionResult { get; private set; }

    public NuevaUbicacionWindow()
    {
        InitializeComponent();
    }

    public NuevaUbicacionWindow(UbicacionMesa ubicacion)
    {
        InitializeComponent();
        _idUbicacionExistente = ubicacion.IdUbicacion;
        TxtTituloModal.Text = "Editar Ubicación";
        TxtNombreUbicacion.Text = ubicacion.Ubicacion;
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtNombreUbicacion.Text))
        {
            MostrarError("⚠️ Por favor ingrese el nombre o sector de la ubicación.");
            return;
        }

        UbicacionResult = new UbicacionMesa
        {
            IdUbicacion = _idUbicacionExistente,
            Ubicacion = TxtNombreUbicacion.Text.Trim(),
            EsActivo = true
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
