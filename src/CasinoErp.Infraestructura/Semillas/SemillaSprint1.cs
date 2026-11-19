namespace CasinoErp.Infraestructura.Semillas;

/// <summary>Datos de demostración del Sprint 1 (jugadores, mesas, caja, retención, rellenos y fichas).</summary>
public class SemillaSprint1 : ISemilla
{
    public int Orden => 0;

    public void Cargar(CasinoErpDbContext db) => DatosIniciales.Cargar(db);
}
