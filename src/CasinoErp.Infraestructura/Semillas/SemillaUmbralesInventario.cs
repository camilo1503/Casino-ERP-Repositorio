using CasinoErp.Dominio.Inventario;

namespace CasinoErp.Infraestructura.Semillas;

/// <summary>
/// Baja el umbral de FICHA-500K a 400 para que la demo arranque por encima del mínimo (stock 420)
/// y una compra de 25 fichas de $500.000 lo cruce (queda en 395). Las demás denominaciones siguen en 1000.
/// </summary>
public class SemillaUmbralesInventario : ISemilla
{
    public int Orden => 5;

    public void Cargar(CasinoErpDbContext db)
    {
        var ficha = db.Set<Ficha>().FirstOrDefault(f => f.Codigo == "FICHA-500K");
        if (ficha is null || ficha.StockMinimo != 1000) return;

        ficha.StockMinimo = 400;
        db.SaveChanges();
    }
}
