using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Inventario;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Dominio.Retencion;
using CasinoErp.Infraestructura.Modulos;
using CasinoErp.Infraestructura.Semillas;
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
        services.AddScoped<ISemilla, SemillaSprint1>();

        // Sprint 2: cada módulo se registra en su propio archivo de Modulos/ (contrato C1, decisión D-004).
        services.AgregarPersonal().AgregarNomina().AgregarCompras().AgregarSeguridad();
        return services;
    }

    /// <summary>
    /// Prepara la base al arrancar: la borra si "BaseDatos:RecrearAlIniciar" es true (D-006), crea las tablas
    /// que falten y, si "BaseDatos:CargarDatosDemo" es true, ejecuta las semillas de todos los módulos en orden.
    /// </summary>
    public static void PrepararBaseDatos(this IServiceProvider proveedor, IConfiguration configuracion)
    {
        var recrear = Booleano(configuracion["BaseDatos:RecrearAlIniciar"], porDefecto: false);
        var cargarDatosDemo = Booleano(configuracion["BaseDatos:CargarDatosDemo"], porDefecto: true);

        using var scope = proveedor.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasinoErpDbContext>();
        var semillas = scope.ServiceProvider.GetServices<ISemilla>().OrderBy(s => s.Orden).ToList();
        db.Database.CreateExecutionStrategy().Execute(() =>
        {
            if (recrear) db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            if (!cargarDatosDemo) return;
            foreach (var semilla in semillas) semilla.Cargar(db);
        });
    }

    private static bool Booleano(string? valor, bool porDefecto) => bool.TryParse(valor, out var resultado) ? resultado : porDefecto;
}
