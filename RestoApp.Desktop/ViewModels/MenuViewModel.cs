using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestoApp.Business.Services;
using RestoApp.Entities;

namespace RestoApp.Desktop.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private readonly MenuService? _menuService;

    public bool PuedeGestionar => SesionGlobal.RolActual == RolUsuario.Dueno 
                               || SesionGlobal.RolActual == RolUsuario.Gerente;

    [ObservableProperty]
    private ObservableCollection<CategoriaMenu> _categorias = new();

    [ObservableProperty]
    private CategoriaMenu? _categoriaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _itemsMenu = new();

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _itemsFiltrados = new();

    [ObservableProperty]
    private string _filtroTexto = string.Empty;

    // Selector de Estado de 3 opciones: "Activos", "Desactivados", "Todos"
    [ObservableProperty]
    private string _filtroEstado = "Activos";

    public bool EsFiltroActivos => FiltroEstado == "Activos";
    public bool EsFiltroDesactivados => FiltroEstado == "Desactivados";
    public bool EsFiltroTodos => FiltroEstado == "Todos";

    [ObservableProperty]
    private string _mensajeInformativo = string.Empty;

    [ObservableProperty]
    private bool _mostrarMensajeInformativo;

    public MenuViewModel(MenuService? menuService)
    {
        _menuService = menuService;
        _ = CargarDatosAsync();
    }

    [RelayCommand]
    public async Task CargarDatosAsync()
    {
        if (_menuService == null) return;

        if (Categorias.Count == 0)
        {
            var categorias = await _menuService.ObtenerCategoriasAsync();
            var listaCategorias = categorias.ToList();
            
            // Agregar opción ficticia "Todas las categorías"
            var todas = new CategoriaMenu { IdCategoria = 0, Nombre = "Todas las Categorías" };
            listaCategorias.Insert(0, todas);
            Categorias = new ObservableCollection<CategoriaMenu>(listaCategorias);
            CategoriaSeleccionada = todas;
        }

        // Si puede gestionar (admin), traemos todos (activos e inactivos) de la base de datos
        // para permitir filtrar dinámicamente según el checkbox de Solo Activos.
        // Si no puede gestionar (mozo), solo traemos activos.
        var soloActivosDb = !PuedeGestionar;
        var items = await _menuService.ObtenerMenuAsync(soloActivos: soloActivosDb);

        ItemsMenu = new ObservableCollection<MenuItemViewModel>(items.Select(i => new MenuItemViewModel(i)));
        AplicarFiltros();
    }

    partial void OnFiltroTextoChanged(string value) => AplicarFiltros();
    partial void OnCategoriaSeleccionadaChanged(CategoriaMenu? value) => AplicarFiltros();

    [RelayCommand]
    private void CambiarFiltroEstado(string nuevoFiltro)
    {
        if (FiltroEstado == nuevoFiltro) return;
        FiltroEstado = nuevoFiltro;
        OnPropertyChanged(nameof(EsFiltroActivos));
        OnPropertyChanged(nameof(EsFiltroDesactivados));
        OnPropertyChanged(nameof(EsFiltroTodos));
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var query = ItemsMenu.AsEnumerable();

        if (CategoriaSeleccionada != null && CategoriaSeleccionada.IdCategoria > 0)
        {
            query = query.Where(i => i.IdCategoria == CategoriaSeleccionada.IdCategoria);
        }

        if (!string.IsNullOrWhiteSpace(FiltroTexto))
        {
            query = query.Where(i => i.Descripcion.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase)
                                  || i.CategoriaNombre.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase));
        }

        // OPCIÓN C: SELECTOR DE 3 VISTAS
        if (FiltroEstado == "Activos")
        {
            // Vista 1: Solo productos activos
            query = query.Where(i => i.Activo)
                         .OrderBy(i => i.CategoriaNombre)
                         .ThenBy(i => i.Descripcion);

            MostrarMensajeInformativo = false;
            MensajeInformativo = string.Empty;
            ItemsFiltrados = new ObservableCollection<MenuItemViewModel>(query);
        }
        else if (FiltroEstado == "Desactivados")
        {
            // Vista 2: Solo productos desactivados
            var desactivados = query.Where(i => !i.Activo)
                                    .OrderBy(i => i.CategoriaNombre)
                                    .ThenBy(i => i.Descripcion)
                                    .ToList();

            if (desactivados.Count == 0)
            {
                MensajeInformativo = CategoriaSeleccionada != null && CategoriaSeleccionada.IdCategoria > 0
                    ? $"No hay productos desactivados en la categoría '{CategoriaSeleccionada.Nombre}'."
                    : "No hay productos desactivados en el menú.";
                MostrarMensajeInformativo = true;
                ItemsFiltrados = new ObservableCollection<MenuItemViewModel>();
            }
            else
            {
                MostrarMensajeInformativo = false;
                MensajeInformativo = string.Empty;
                ItemsFiltrados = new ObservableCollection<MenuItemViewModel>(desactivados);
            }
        }
        else // "Todos"
        {
            // Vista 3: Todos (activos y desactivados, con los desactivados destacados primero)
            query = query.OrderBy(i => i.Activo) // false (Desactivado) primero que true (Activo)
                         .ThenBy(i => i.CategoriaNombre)
                         .ThenBy(i => i.Descripcion);

            MostrarMensajeInformativo = false;
            MensajeInformativo = string.Empty;
            ItemsFiltrados = new ObservableCollection<MenuItemViewModel>(query);
        }
    }

    // Método público invocado desde el code-behind después de que la ventana devuelve los datos
    public async Task GuardarDesdeVentanaAsync(int idItem, string descripcion, double precio, int idCategoria, bool activo)
    {
        if (_menuService == null) return;
        await _menuService.GuardarItemAsync(idItem, descripcion, precio, idCategoria, activo);
        await CargarDatosAsync();
    }

    [RelayCommand]
    private async Task ToggleEstadoItemAsync(MenuItemViewModel item)
    {
        if (!PuedeGestionar || _menuService == null) return;
        bool nuevoEstado = !item.Activo;
        await _menuService.CambiarEstadoAsync(item.IdItem, nuevoEstado);
        item.Activo = nuevoEstado;
        AplicarFiltros();
    }

    [RelayCommand]
    private void CerrarMensajeInformativo()
    {
        MostrarMensajeInformativo = false;
        MensajeInformativo = string.Empty;
        CambiarFiltroEstado("Activos");
    }

}
