using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("persona")]
public class Persona
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]//Le decimos que no es una clave autoincremental
    [Column("dni")]
    public long Dni {get; set;}

    [Column("nombre")]
    public string Nombre {get; set;} = string.Empty;

    [Column("apellido")]
    public string Apellido {get; set;} = string.Empty;

    [Column("email")]
    public string Email {get; set;} = string.Empty;

    [Column("telefono")]
    public long Telefono {get; set;}

    [Column("password")]
    public string Password {get; set;} = string.Empty;

}
