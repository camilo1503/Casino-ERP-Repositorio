namespace CasinoErp.Infraestructura.Semillas;

/// <summary>
/// Datos de demostración de un módulo (contrato C1). Cada módulo registra la suya en su archivo
/// Modulos/Modulo&lt;Nombre&gt;.cs y PrepararBaseDatos las ejecuta en orden ascendente de <see cref="Orden"/>.
/// </summary>
public interface ISemilla
{
    /// <summary>Sprint1 = 0, Personal = 10, Nomina = 20, Compras = 30, Seguridad = 40.</summary>
    int Orden { get; }

    /// <summary>Debe ser idempotente: si los datos ya existen, no hace nada.</summary>
    void Cargar(CasinoErpDbContext db);
}
