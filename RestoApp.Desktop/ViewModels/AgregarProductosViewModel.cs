using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class AgregarProductosViewModel : ObservableObject
{
    private readonly MenuService _menuService;
    private readonly PagoService _pagoService;

    public event Action<bool>? RequestClose;

    [ObservableProperty]
    private CuentaPendienteViewModel _cuenta;

    public string TituloMesa => $"Mesa #{Cuenta.NroMesa} ({Cuenta.Sector})";
    public string MozoMesaTexto => !string.IsNullOrWhiteSpace(Cuenta.AtendidoPor) ? $"Atendido por: {Cuenta.AtendidoPor}" : "Sin mozo asignado";

    // Catálogo y Filtros
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _itemsCatalogo = new();

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _itemsFiltrados = new();

    [ObservableProperty]
    private ObservableCollection<string> _categorias = new() { "Todas" };

    [ObservableProperty]
    private string _categoriaSeleccionada = "Todas";

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    // Formulario de Selección / Cantidad
    [ObservableProperty]
    private bool _mostrarFormularioCantidad;

    [ObservableProperty]
    private MenuItemViewModel? _itemEnEdicion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalEnEdicion))]
    [NotifyPropertyChangedFor(nameof(SubtotalEnEdicionTexto))]
    private decimal? _cantidadParaAgregar = 1m;

    [ObservableProperty]
    private string _mensajeValidacion = string.Empty;

    public double SubtotalEnEdicion => (ItemEnEdicion != null && CantidadParaAgregar.HasValue && CantidadParaAgregar.Value > 0)
        ? (double)CantidadParaAgregar.Value * ItemEnEdicion.Precio
        : 0;

    public string SubtotalEnEdicionTexto => $"${SubtotalEnEdicion:N2}";

    // Pedido a Agregar
    [ObservableProperty]
    private ObservableCollection<CarritoItemViewModel> _itemsCarrito = new();

    [ObservableProperty]
    private bool _tieneItems;

    [ObservableProperty]
    private bool _estaVacio = true;

    [ObservableProperty]
    private double _totalCarrito;

    [ObservableProperty]
    private string _totalCarritoTexto = "$0,00";

    [ObservableProperty]
    private int _totalArticulosCarrito;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    private bool _esProcesando;

    public AgregarProductosViewModel(
        CuentaPendienteViewModel cuenta,
        MenuService? menuService = null,
        PagoService? pagoService = null)
    {
        _cuenta = cuenta;

        _menuService = menuService
            ?? App.Services?.GetService(typeof(MenuService)) as MenuService
            ?? new MenuService(new MenuRepository(new RestoAppDbContext()));

        _pagoService = pagoService
            ?? App.Services?.GetService(typeof(PagoService)) as PagoService
            ?? new PagoService(new PagoRepository(new RestoAppDbContext()));

        NotificarCambiosCarrito();
        _ = CargarCatalogoAsync();
    }

    public async Task CargarCatalogoAsync()
    {
        try
        {
            var menuEntidades = await _menuService.ObtenerMenuAsync(soloActivos: true);
            var categoriasEntidades = await _menuService.ObtenerCategoriasAsync();

            var listaCategorias = new List<string> { "Todas" };
            listaCategorias.AddRange(categoriasEntidades.Select(c => c.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)));
            Categorias = new ObservableCollection<string>(listaCategorias.Distinct());

            var listaItems = menuEntidades
                .Where(m => m.Activo)
                .Select(m => new MenuItemViewModel(m))
                .OrderBy(m => m.CategoriaNombre)
                .ThenBy(m => m.Descripcion)
                .ToList();

            ItemsCatalogo = new ObservableCollection<MenuItemViewModel>(listaItems);
            AplicarFiltros();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CATALOGO ERROR] {ex.Message}");
            MensajeEstado = $"Error al cargar menú: {ex.Message}";
        }
    }

    partial void OnCategoriaSeleccionadaChanged(string value) => AplicarFiltros();
    partial void OnTextoBusquedaChanged(string value) => AplicarFiltros();

    private void AplicarFiltros()
    {
        IEnumerable<MenuItemViewModel> query = ItemsCatalogo;

        if (!string.IsNullOrWhiteSpace(CategoriaSeleccionada) && CategoriaSeleccionada != "Todas")
        {
            query = query.Where(i => string.Equals(i.CategoriaNombre, CategoriaSeleccionada, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            string search = TextoBusqueda.Trim().ToLower();
            query = query.Where(i => i.Descripcion.ToLower().Contains(search) || i.CategoriaNombre.ToLower().Contains(search));
        }

        ItemsFiltrados = new ObservableCollection<MenuItemViewModel>(query.ToList());
    }

    [RelayCommand]
    public void FiltrarPorCategoria(string categoria)
    {
        CategoriaSeleccionada = categoria;
    }

    [RelayCommand]
    public void SeleccionarProducto(MenuItemViewModel? item)
    {
        if (item == null) return;
        ItemEnEdicion = item;
        CantidadParaAgregar = 1m;
        MensajeValidacion = string.Empty;
        MostrarFormularioCantidad = true;
    }

    [RelayCommand]
    public void CerrarFormularioCantidad()
    {
        MostrarFormularioCantidad = false;
        ItemEnEdicion = null;
        MensajeValidacion = string.Empty;
    }

    [RelayCommand]
    public void AgregarAlCarrito()
    {
        if (!ValidarCantidad()) return;

        int cant = (int)Math.Floor(CantidadParaAgregar!.Value);
        var existente = ItemsCarrito.FirstOrDefault(i => i.IdItem == ItemEnEdicion!.IdItem);

        if (existente != null)
        {
            existente.Cantidad += cant;
        }
        else
        {
            ItemsCarrito.Add(new CarritoItemViewModel
            {
                IdItem = ItemEnEdicion!.IdItem,
                Descripcion = ItemEnEdicion.Descripcion,
                CategoriaNombre = ItemEnEdicion.CategoriaNombre,
                PrecioUnitario = ItemEnEdicion.Precio,
                Cantidad = cant
            });
        }

        NotificarCambiosCarrito();
        MostrarFormularioCantidad = false;
        MensajeEstado = $"✓ Se agregó {cant}x {ItemEnEdicion!.Descripcion} al pedido.";
        ItemEnEdicion = null;
    }

    [RelayCommand]
    public async Task AgregarDirectoAMesaAsync()
    {
        if (!ValidarCantidad()) return;

        // Añadir el ítem actual al carrito
        int cant = (int)Math.Floor(CantidadParaAgregar!.Value);
        var existente = ItemsCarrito.FirstOrDefault(i => i.IdItem == ItemEnEdicion!.IdItem);

        if (existente != null)
        {
            existente.Cantidad += cant;
        }
        else
        {
            ItemsCarrito.Add(new CarritoItemViewModel
            {
                IdItem = ItemEnEdicion!.IdItem,
                Descripcion = ItemEnEdicion.Descripcion,
                CategoriaNombre = ItemEnEdicion.CategoriaNombre,
                PrecioUnitario = ItemEnEdicion.Precio,
                Cantidad = cant
            });
        }

        NotificarCambiosCarrito();
        MostrarFormularioCantidad = false;

        // Confirmar todo el carrito inmediatamente a la mesa
        await ConfirmarTodoAMesaAsync();
    }

    private bool ValidarCantidad()
    {
        MensajeValidacion = string.Empty;

        if (ItemEnEdicion == null)
        {
            MensajeValidacion = "⚠️ Seleccione un producto.";
            return false;
        }

        if (!CantidadParaAgregar.HasValue)
        {
            MensajeValidacion = "⚠️ Ingrese una cantidad válida (mínimo 1).";
            return false;
        }

        if (CantidadParaAgregar.Value <= 0)
        {
            MensajeValidacion = "⚠️ La cantidad debe ser mayor que cero.";
            return false;
        }

        if (CantidadParaAgregar.Value > 99)
        {
            MensajeValidacion = "⚠️ La cantidad no puede superar 99 unidades.";
            return false;
        }

        return true;
    }

    [RelayCommand]
    public void IncrementarItemCarrito(CarritoItemViewModel? item)
    {
        if (item == null) return;
        if (item.Cantidad < 99)
        {
            item.Cantidad++;
            NotificarCambiosCarrito();
        }
    }

    [RelayCommand]
    public void DecrementarItemCarrito(CarritoItemViewModel? item)
    {
        if (item == null) return;
        if (item.Cantidad > 1)
        {
            item.Cantidad--;
            NotificarCambiosCarrito();
        }
        else
        {
            ItemsCarrito.Remove(item);
            NotificarCambiosCarrito();
        }
    }

    [RelayCommand]
    public void EliminarItemCarrito(CarritoItemViewModel? item)
    {
        if (item == null) return;
        ItemsCarrito.Remove(item);
        NotificarCambiosCarrito();
    }

    [RelayCommand]
    public void VaciarCarrito()
    {
        ItemsCarrito.Clear();
        NotificarCambiosCarrito();
        MensajeEstado = "El pedido ha sido vaciado.";
    }

    private void NotificarCambiosCarrito()
    {
        TotalCarrito = ItemsCarrito.Sum(i => i.Subtotal);
        TotalCarritoTexto = $"${TotalCarrito:N2}";
        TotalArticulosCarrito = ItemsCarrito.Sum(i => i.Cantidad);
        TieneItems = ItemsCarrito.Count > 0;
        EstaVacio = ItemsCarrito.Count == 0;
    }

    [RelayCommand]
    public async Task ConfirmarTodoAMesaAsync()
    {
        if (ItemsCarrito.Count == 0)
        {
            MensajeEstado = "⚠️ El pedido está vacío. Agregue productos antes de enviar a la mesa.";
            return;
        }

        EsProcesando = true;
        MensajeEstado = "Enviando comanda a la mesa...";

        try
        {
            foreach (var item in ItemsCarrito)
            {
                await _pagoService.AgregarItemACuentaAsync(
                    Cuenta.CuentaId,
                    item.IdItem,
                    item.Cantidad);
            }

            RequestClose?.Invoke(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIRMAR PEDIDO MESA ERROR] {ex.Message}");
            MensajeEstado = $"Error al registrar en mesa: {ex.Message}";
        }
        finally
        {
            EsProcesando = false;
        }
    }

    [RelayCommand]
    public void CancelarVentana()
    {
        RequestClose?.Invoke(false);
    }
}
