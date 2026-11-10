using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Desktop.ViewModels;
using RestoApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestoApp.Desktop.Views;

public partial class NuevoMenuItemWindow : Window
{
    private readonly int _idItemExistente = 0;

    // Resultado que el caller puede leer después del ShowDialog
    public string? ResultDescripcion { get; private set; }
    public double ResultPrecio { get; private set; }
    public int ResultIdCategoria { get; private set; }
    public bool ResultActivo { get; private set; }

    // Constructor vacío (Nuevo Item, sin categorías precargadas)
    public NuevoMenuItemWindow() : this((IEnumerable<CategoriaMenu>?)null)
    {
    }

    // Constructor para Nuevo Item con categorías
    public NuevoMenuItemWindow(IEnumerable<CategoriaMenu>? categorias)
    {
        InitializeComponent();
        CargarCategoriasCombo(categorias, null);
    }

    // Constructor para Editar Item existente
    public NuevoMenuItemWindow(MenuItemViewModel item, IEnumerable<CategoriaMenu>? categorias = null)
    {
        InitializeComponent();
        _idItemExistente = item.IdItem;
        TxtTituloModal.Text = "Editar Item del Menú";
        TxtDescripcion.Text = item.Descripcion;
        TxtPrecio.Text = item.Precio.ToString();
        ChkActivo.IsChecked = item.Activo;
        CargarCategoriasCombo(categorias, item.IdCategoria);
    }

    private void CargarCategoriasCombo(IEnumerable<CategoriaMenu>? categorias, int? idCategoriaSeleccionada)
    {
        // Filtrar la opción ficticia "Todas las Categorías" (IdCategoria == 0)
        var lista = categorias?.Where(c => c.IdCategoria > 0).ToList() ?? new List<CategoriaMenu>();

        CboCategoria.ItemsSource = lista;

        if (idCategoriaSeleccionada.HasValue && idCategoriaSeleccionada.Value > 0)
        {
            var sel = lista.FirstOrDefault(c => c.IdCategoria == idCategoriaSeleccionada.Value);
            if (sel != null)
            {
                CboCategoria.SelectedItem = sel;
            }
            else if (lista.Any())
            {
                CboCategoria.SelectedIndex = 0;
            }
        }
        else if (lista.Any())
        {
            CboCategoria.SelectedIndex = 0;
        }
    }

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        // Validar Descripción
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            MostrarError("⚠️ Por favor ingrese una descripción o nombre para el producto.");
            return;
        }

        // Validar Precio
        if (string.IsNullOrWhiteSpace(TxtPrecio.Text) || !double.TryParse(TxtPrecio.Text, out double precio) || precio <= 0)
        {
            MostrarError("⚠️ Por favor ingrese un precio válido (mayor a 0).");
            return;
        }

        // Validar Categoría
        if (CboCategoria.SelectedItem is not CategoriaMenu categoriaSeleccionada)
        {
            MostrarError("⚠️ Por favor seleccione una categoría para el producto.");
            return;
        }

        // Setear resultados para que el caller los consuma
        ResultDescripcion = TxtDescripcion.Text.Trim();
        ResultPrecio = precio;
        ResultIdCategoria = categoriaSeleccionada.IdCategoria;
        ResultActivo = ChkActivo.IsChecked == true;

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
