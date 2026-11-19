using CasinoErp.Dominio.Nominas;

namespace CasinoErp.Infraestructura.Semillas;

/// <summary>Parámetros de nómina 2026 (Decretos 1469 y 1470 de 2025, D-003) si la tabla está vacía.</summary>
public class SemillaNomina : ISemilla
{
    public int Orden => 20;

    public void Cargar(CasinoErpDbContext db)
    {
        if (db.Set<ParametrosNomina>().Any()) return;

        db.Set<ParametrosNomina>().Add(ParametrosNomina.Decreto2026());
        db.SaveChanges();
    }
}
