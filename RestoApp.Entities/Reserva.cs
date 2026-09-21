using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("reserva")]
public class Reserva
{
    [Key]
    [Column("id_reserva")]
    public int IdReserva { get; set; }
    
    [Column("fecha_reserva")]
    public DateTime FechaReserva { get; set; }
    
    [Column("cant_personas")]
    public int CantPersonas { get; set; }
    
    [Column("id_estado")]
    public int IdEstado { get; set; } = 1;

    [ForeignKey("IdEstado")]
    public EstadoReserva? Estado { get; set; }

    [Column("id_evento")]
    public int? IdEvento { get; set; }

    [ForeignKey("IdEvento")]
    public Evento? Evento { get; set; }

    [Column("dni_cliente")]
    public long DniCliente { get; set; }
    
    [ForeignKey("DniCliente")]
    public Cliente? Cliente { get; set; }

    [Column("dni_empleado")]
    public long? DniEmpleado { get; set; }

    [Column("id_rol")]
    public int? IdRol { get; set; }

    [ForeignKey("DniEmpleado, IdRol")]
    public Empleado? Empleado { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("fecha_max_cancelacion")]
    public DateTime? FechaMaxCancelacion { get; set; }

    public ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}