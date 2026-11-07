using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("menu")]
public class MenuItem
{
    [Key]
    [Column("id_item")]
    public int IdItem { get; set; }

    [Column("descripcion")]
    [MaxLength(30)]
    public string Descripcion { get; set; } = string.Empty;

    [Column("precio")]
    public double Precio { get; set; }

    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [ForeignKey("IdCategoria")]
    public CategoriaMenu? Categoria { get; set; }
}

