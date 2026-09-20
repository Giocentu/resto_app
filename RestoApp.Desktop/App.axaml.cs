using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RestoApp.Desktop.Views;
//using RestoApp.Business.Services;
using RestoApp.Data;
using RestoApp.Data.Repositories;
using System;

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
            // Aquí luego inyectaremos el ViewModel principal
            desktop.MainWindow = new LoginWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. Registrar Base de Datos
        services.AddDbContext<RestoAppDbContext>();

        // 2. Registrar Repositorios (Genérico y Específicos)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IMesaRepository, MesaRepository>();

        // 3. Registrar Servicios de Negocio (Ejemplo)
        //services.AddScoped<ReservaService>();
    }
}