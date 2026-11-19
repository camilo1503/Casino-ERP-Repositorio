using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CasinoErp.PruebasIntegracion;

/// <summary>
/// Levanta la API con una base SQLite temporal y los datos de demostración (contrato C3).
/// Cada instancia usa su propio archivo, que se borra al desechar la factory.
/// </summary>
public class CasinoErpFactory : WebApplicationFactory<Program>
{
    private readonly string _archivo = Path.Combine(Path.GetTempPath(), $"casino-erp-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("BaseDatos:Proveedor", "Sqlite");
        builder.UseSetting("BaseDatos:CargarDatosDemo", "true");
        builder.UseSetting("BaseDatos:RecrearAlIniciar", "false");
        builder.UseSetting("ConnectionStrings:CasinoErp", $"Data Source={_archivo};Pooling=False");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_archivo)) File.Delete(_archivo);
    }
}
