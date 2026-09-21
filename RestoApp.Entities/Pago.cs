using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Entities;

[Table("pagos")]
public class Pago
{
    [Key]
    [Column("id_pago")]
    public int IdPago { get; set; }

    [Column("monto", TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Column("fecha_pago")]
    public DateTime FechaPago { get; set; }

    [Column("id_metodo")]
    public int IdMetodo { get; set; }

    [ForeignKey("IdMetodo")]
    public MetodoPago? MetodoPago { get; set; }

    [Column("id_reserva")]
    public int? IdReserva { get; set; }

    [ForeignKey("IdReserva")]
    public Reserva? Reserva { get; set; }
}