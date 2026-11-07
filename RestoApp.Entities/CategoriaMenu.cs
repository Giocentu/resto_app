using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("categoria_menu")]
public class CategoriaMenu
{
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("nombre")]
    [MaxLength(30)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}

