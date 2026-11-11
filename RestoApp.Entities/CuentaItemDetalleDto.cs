namespace RestoApp.Entities;

public class CuentaItemDetalleDto
{
    public int IdCuentaBody { get; set; }
    public int? CuentaId { get; set; }
    public int IdItem { get; set; }

    private string? _descripcion;
    public string? Descripcion
    {
        get => _descripcion ?? _descripcionItem;
        set => _descripcion = value;
    }

    private string? _descripcionItem;
    public string? DescripcionItem
    {
        get => _descripcionItem ?? _descripcion;
        set => _descripcionItem = value;
    }

    public string NombreProducto => !string.IsNullOrWhiteSpace(DescripcionItem)
        ? DescripcionItem!
        : (!string.IsNullOrWhiteSpace(Descripcion) ? Descripcion! : "Producto");

    public string Categoria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal { get; set; }

    public string PrecioUnitarioTexto => $"$ {PrecioUnitario:N2}";
    public string SubtotalTexto => $"$ {Subtotal:N2}";
}

