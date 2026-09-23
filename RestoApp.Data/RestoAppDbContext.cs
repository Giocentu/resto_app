using Microsoft.EntityFrameworkCore;
using RestoApp.Entities;

namespace RestoApp.Data;

public class RestoAppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<RolEmpleado> RolesEmpleado { get; set; }
    public DbSet<TurnoEmpleado> TurnosEmpleado { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<EstadoReserva> EstadosReserva { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<UbicacionMesa> UbicacionesMesa { get; set; }
    public DbSet<Mesa> Mesas { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<Pago> Pagos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Reemplaza "TU_SERVIDOR" por el nombre de tu instancia (ej. localhost\SQLEXPRESS)
optionsBuilder.UseSqlServer(@"Server=HERNAN\SQLEXPRESS;Database=resto_DB;Integrated Security=True;TrustServerCertificate=True;");    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clave primaria compuesta para Empleado (dni_empleado, id_rol)
        modelBuilder.Entity<Empleado>()
            .HasKey(e => new { e.DniEmpleado, e.IdRol });

        // Relación opcional entre Reserva y Empleado (usando la clave compuesta de Empleado)
        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Empleado)
            .WithMany()
            .HasForeignKey(r => new { r.DniEmpleado, r.IdRol })
            .IsRequired(false);

        // Relación opcional entre Reserva y Evento
        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Evento)
            .WithMany()
            .HasForeignKey(r => r.IdEvento)
            .IsRequired(false);

        // Relación opcional entre Pago y Reserva
        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Reserva)
            .WithMany()
            .HasForeignKey(p => p.IdReserva)
            .IsRequired(false);

        // Configuración fluida para la tabla intermedia muchos a muchos (reserva_mesa)
        modelBuilder.Entity<Reserva>()
            .HasMany(r => r.Mesas)
            .WithMany(m => m.Reservas)
            .UsingEntity<Dictionary<string, object>>(
                "reserva_mesa", // Nombre de la tabla intermedia en tu DER
                j => j.HasOne<Mesa>().WithMany().HasForeignKey("id_mesa"),
                j => j.HasOne<Reserva>().WithMany().HasForeignKey("id_reserva")
            );
    }
}