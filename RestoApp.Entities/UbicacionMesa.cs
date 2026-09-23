using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("ubicacion_mesa")]
public class UbicacionMesa
{
    [Key]
    [Column("id_ubicacion")]
    public int IdUbicacion { get; set; }

    [Column("ubicacion")]
    public string Ubicacion { get; set; } = string.Empty;

    [NotMapped]
    public bool EsActivo { get; set; } = true;
}