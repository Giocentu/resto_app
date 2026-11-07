using System;
using CommunityToolkit.Mvvm.ComponentModel;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{
    public int IdItem { get; set; }
    public int IdCategoria { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrecioFormateado))]
    private double _precio;

    [ObservableProperty]
    private string _descripcion = string.Empty;

    [ObservableProperty]
    private string _categoriaNombre = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstadoTexto))]
    [NotifyPropertyChangedFor(nameof(ColorBadgeEstado))]
    [NotifyPropertyChangedFor(nameof(TextoBotonEstado))]
    [NotifyPropertyChangedFor(nameof(ColorFondoBotonEstado))]
    [NotifyPropertyChangedFor(nameof(ColorTextoBotonEstado))]
    private bool _activo;

    public string PrecioFormateado => $"${Precio:N2}";
    public string EstadoTexto => Activo ? "Activo" : "Desactivado";
    public string ColorBadgeEstado => Activo ? "#27AE60" : "#E74C3C";

    public string TextoBotonEstado => Activo ? "Activo" : "Desactivado";
    public string ColorFondoBotonEstado => Activo ? "#DCFCE7" : "#FEE2E2";
    public string ColorTextoBotonEstado => Activo ? "#166534" : "#991B1B";

    public string ColorFondoCategoria => ObtenerPaletaCategoria(IdCategoria).Fondo;
    public string ColorTextoCategoria => ObtenerPaletaCategoria(IdCategoria).Texto;

    private static (string Fondo, string Texto) ObtenerPaletaCategoria(int id)
    {
        return id switch
        {
            1 => ("#FEF3C7", "#B45309"), // Entradas (Ámbar suave)
            2 => ("#FEE2E2", "#B91C1C"), // Parrilla y Carnes (Rojo suave)
            3 => ("#FFEDD5", "#C2410C"), // Pastas Artesanales (Naranja suave)
            4 => ("#FEF9C3", "#A16207"), // Minutas y Hamburguesas (Amarillo suave)
            5 => ("#E0F2FE", "#0369A1"), // Bebidas sin Alcohol (Azul celeste)
            6 => ("#FDE68A", "#92400E"), // Cervezas (Dorado / malta)
            7 => ("#F3E8FF", "#6B21A8"), // Vinos de Selección (Púrpura suave)
            8 => ("#FCE7F3", "#BE185D"), // Postres Caseros (Rosa pastel)
            9 => ("#E2E8F0", "#334155"), // Cafetería (Gris pizarra)
            _ => PaletaDinamica(id)
        };
    }

    private static (string Fondo, string Texto) PaletaDinamica(int id)
    {
        var paletas = new (string Fondo, string Texto)[]
        {
            ("#ECFDF5", "#047857"),
            ("#EEF2FF", "#4338CA"),
            ("#F0FDF4", "#15803D"),
            ("#FFF1F2", "#BE123C"),
            ("#F0FDFA", "#0F766E")
        };
        return paletas[Math.Abs(id) % paletas.Length];
    }

    public MenuItemViewModel(MenuItem item)
    {
        IdItem = item.IdItem;
        IdCategoria = item.IdCategoria;
        Descripcion = item.Descripcion;
        Precio = item.Precio;
        CategoriaNombre = item.Categoria?.Nombre ?? "General";
        Activo = item.Activo;
    }
}

