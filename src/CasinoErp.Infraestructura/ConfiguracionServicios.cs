using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Inventario;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Dominio.Retencion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.Infraestructura;

public static class ConfiguracionServicios
{
    /// <summary>
    /// Registra la base de datos, los repositorios y los servicios del dominio.
    /// "BaseDatos:Proveedor" = "SqlServer" usa SQL Server / Azure SQL; cualquier otro valor usa SQLite (local).
    /// </summary>
    public static IServiceCollection AgregarCasinoErp(this IServiceCollection services, IConfiguration configuracion)
    {
        var proveedor = configuracion["BaseDatos:Proveedor"] ?? "Sqlite";
        var cadena = configuracion.GetConnectionString("CasinoErp") ?? "Data Source=casino-erp.db";

        services.AddDbContext<CasinoErpDbContext>(opciones =>
        {
            if (proveedor.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                // Reintentos: Azure SQL (nivel gratuito) se pausa cuando no se usa y tarda unos segundos en reanudar.
                opciones.UseSqlServer(cadena, sql => sql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null));
            else
                opciones.UseSqlite(cadena);
        });

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IJugadorRepositorio, JugadorRepositorio>();
        services.AddScoped<IMesaRepositorio, MesaRepositorio>();
        services.AddScoped<ITransaccionRepositorio, TransaccionRepositorio>();
        services.AddScoped<IOfertaRepositorio, OfertaRepositorio>();
        services.AddScoped<IRellenoRepositorio, RellenoRepositorio>();
        services.AddScoped<IFichaRepositorio, FichaRepositorio>();

        services.AddScoped<JugadorService>();
        services.AddScoped<MesaService>();
        services.AddScoped<CajaService>();
        services.AddScoped<RetencionService>();
        services.AddScoped<RellenoService>();
        services.AddScoped<InventarioFichasService>();
        return services;
    }

    /// <summary>Crea las tablas si no existen y carga los datos de demostración.</summary>
    public static void PrepararBaseDatos(this IServiceProvider proveedor, bool cargarDatosDemo)
    {
        using var scope = proveedor.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasinoErpDbContext>();
        db.Database.CreateExecutionStrategy().Execute(() =>
        {
            db.Database.EnsureCreated();
            if (cargarDatosDemo) DatosIniciales.Cargar(db);
        });
    }
}
