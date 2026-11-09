namespace RestoApp.Entities;

public class UsuarioAutorizanteItem
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string PasswordEsperada { get; set; } = string.Empty;

    public string DisplayText => $"{NombreCompleto} ({Rol})";

    public override string ToString() => DisplayText;
}

