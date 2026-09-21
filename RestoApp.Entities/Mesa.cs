using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("mesa")]
public class Mesa
{
    [Key]
    [Column("id_mesa")]
    public int IdMesa { get; set; }

    [Column("nro_mesa")]
    public int NroMesa { get; set; }

    [Column("capacidad")]
    public int Capacidad { get; set; }

    [Column("id_ubicacion")]
    public int IdUbicacion { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = "LIBRE";

    [Column("es_activo")]
    public bool EsActivo { get; set; } = true;

    [ForeignKey("IdUbicacion")]
    public UbicacionMesa? Ubicacion { get; set; }

    // Relación N:M gestionada por EF Core
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}