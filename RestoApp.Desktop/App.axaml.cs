using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RestoApp.Desktop.Views;
using RestoApp.Business.Services;

using RestoApp.Data;
using RestoApp.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using RestoApp.Desktop.Services;

namespace RestoApp.Desktop;

public partial class App : Application
{
    // Exponer el proveedor de servicios para que Avalonia pueda acceder a él
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        ConfigureServices(collection);
        Services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Probar la conexión a la base de datos al iniciar
            try
            {
                using var scope = Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<RestoAppDbContext>();
                var conn = dbContext.Database.GetDbConnection();
                conn.Open();
                conn.Close();
                Console.WriteLine("==================================================");
                Console.WriteLine("[DB SUCCESS] ¡Conexión exitosa a la base de datos resto_DB!");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine("==================================================");
                Console.WriteLine($"[DB ERROR DETAIL] {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DB INNER ERROR] {ex.InnerException.Message}");
                }
                Console.WriteLine("==================================================");
                _ = AlertaService.MostrarAlertaConexionAsync();
            }

            // Aquí luego inyectaremos el ViewModel principal
            desktop.MainWindow = new LoginWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. Registrar Base de Datos como Transient para evitar colisiones de hilos (DbContext concurrencia)
        services.AddDbContext<RestoAppDbContext>(ServiceLifetime.Transient);

        // 2. Registrar Repositorios como Transient
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddTransient<IReservaRepository, ReservaRepository>();
        services.AddTransient<IClienteRepository, ClienteRepository>();
        services.AddTransient<IEmpleadoRepository, EmpleadoRepository>();
        services.AddTransient<IMesaRepository, MesaRepository>();
        services.AddTransient<IEventoRepository, EventoRepository>();
        services.AddTransient<IPagoRepository, PagoRepository>();
        services.AddTransient<IUbicacionRepository, UbicacionRepository>();
        services.AddTransient<ITurnoRepository, TurnoRepository>();

        // 3. Registrar Servicios de Negocio como Transient
        services.AddTransient<ReservaService>();
        services.AddTransient<ClienteService>();
        services.AddTransient<EmpleadoService>();
        services.AddTransient<MesaService>();
        services.AddTransient<EventoService>();
        services.AddTransient<PagoService>();
        services.AddTransient<UbicacionService>();
        services.AddTransient<TurnoService>();
    }
}