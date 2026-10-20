using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("turno_empleado")]
public class TurnoEmpleado
{
    [Key]
    [Column("id_turno")]
    public int IdTurno { get; set; }

    [Column("inicio_turno")]
    public TimeSpan InicioTurno { get; set; }

    [Column("fin_turno")]
    public TimeSpan FinTurno { get; set; }

    [Column("es_activo")]
    public bool EsActivo { get; set; } = true;
}