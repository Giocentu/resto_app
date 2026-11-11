namespace RestoApp.Entities;

public class MozoItemDto
{
    public long Dni { get; set; }
    public int IdRol { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string RolDescripcion { get; set; } = "Mozo";

    public string DisplayTexto => NombreCompleto;
}

