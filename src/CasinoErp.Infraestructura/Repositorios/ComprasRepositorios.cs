using CasinoErp.Dominio.Compras;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Infraestructura.Repositorios;

/// <summary>Proveedores con Entity Framework Core (HU-05).</summary>
public class ProveedorRepositorio(CasinoErpDbContext db) : IProveedorRepositorio
{
    private DbSet<Proveedor> Proveedores => db.Set<Proveedor>();

    public Task<bool> ExisteNitAsync(string nit) => Proveedores.AnyAsync(p => p.Nit == nit);

    public Task<Proveedor?> ObtenerPorNitAsync(string nit) => Proveedores.FirstOrDefaultAsync(p => p.Nit == nit);

    public async Task<IReadOnlyList<Proveedor>> ListarAsync(bool soloActivos = false)
    {
        var consulta = Proveedores.AsQueryable();
        if (soloActivos) consulta = consulta.Where(p => p.Activo);
        return await consulta.OrderBy(p => p.Id).ToListAsync();
    }

    public async Task AgregarAsync(Proveedor proveedor)
    {
        Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

/// <summary>Órdenes de compra con Entity Framework Core (HU-05).</summary>
public class OrdenCompraRepositorio(CasinoErpDbContext db) : IOrdenCompraRepositorio
{
    private DbSet<OrdenCompra> Ordenes => db.Set<OrdenCompra>();

    public Task<OrdenCompra?> ObtenerAsync(int id) => Ordenes.FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IReadOnlyList<OrdenCompra>> ListarAsync() =>
        await Ordenes.OrderByDescending(o => o.CreadaEn).ThenByDescending(o => o.Id).ToListAsync();

    public Task<bool> ExisteAbiertaAsync(string codigoFicha) =>
        Ordenes.AnyAsync(o => o.CodigoFicha == codigoFicha && (o.Estado == EstadoOrden.Borrador || o.Estado == EstadoOrden.Enviada));

    public async Task AgregarAsync(OrdenCompra orden)
    {
        Ordenes.Add(orden);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
