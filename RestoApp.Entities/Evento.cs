using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("evento")]
public class Evento
{
    [Key]
    [Column("id_evento")]
    public int IdEvento { get; set; }

    [Column("nombre_evento")]
    public string NombreEvento { get; set; } = string.Empty;

    [Column("fecha_evento")]
    public DateTime? FechaEvento { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("es_activo")]
    public bool EsActivo { get; set; } = true;

    [NotMapped]
    public int CantReservasVinculadas { get; set; } = 0;
}