using CasinoErp.Dominio.Compras;

namespace CasinoErp.Infraestructura.Semillas;

/// <summary>Los dos proveedores de fichas de la demo. Idempotente: si ya hay proveedores, no hace nada.</summary>
public class SemillaCompras : ISemilla
{
    public int Orden => 30;

    public void Cargar(CasinoErpDbContext db)
    {
        if (db.Set<Proveedor>().Any()) return;

        db.Set<Proveedor>().AddRange(
            new Proveedor { Nit = "901234567-1", Nombre = "Fichas Andinas S.A.S.", Correo = "ventas@fichasandinas.co", Telefono = "+57 601 555 0101" },
            new Proveedor { Nit = "900765432-8", Nombre = "Suministros de Juego Ltda.", Correo = "pedidos@suministrosdejuego.co", Telefono = "+57 604 555 0202" });
        db.SaveChanges();
    }
}
