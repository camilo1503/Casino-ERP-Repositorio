using CasinoErp.Dominio.Compras;

namespace CasinoErp.PruebasUnitarias.Dobles;

/// <summary>Repositorio de proveedores en memoria (HU-05, Compras).</summary>
public class ProveedoresEnMemoria : IProveedorRepositorio
{
    public readonly List<Proveedor> Datos = [];

    public Task<bool> ExisteNitAsync(string nit) => Task.FromResult(Datos.Any(p => p.Nit == nit));
    public Task<Proveedor?> ObtenerPorNitAsync(string nit) => Task.FromResult(Datos.FirstOrDefault(p => p.Nit == nit));
    public Task<IReadOnlyList<Proveedor>> ListarAsync(bool soloActivos = false) =>
        Task.FromResult<IReadOnlyList<Proveedor>>(Datos.Where(p => !soloActivos || p.Activo).OrderBy(p => p.Id).ToList());
    public Task AgregarAsync(Proveedor proveedor) { proveedor.Id = Datos.Count + 1; Datos.Add(proveedor); return Task.CompletedTask; }
    public Task GuardarCambiosAsync() => Task.CompletedTask;
}

/// <summary>Repositorio de órdenes de compra en memoria.</summary>
public class OrdenesEnMemoria : IOrdenCompraRepositorio
{
    public readonly List<OrdenCompra> Datos = [];

    public Task<OrdenCompra?> ObtenerAsync(int id) => Task.FromResult(Datos.FirstOrDefault(o => o.Id == id));
    public Task<IReadOnlyList<OrdenCompra>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<OrdenCompra>>(Datos.OrderByDescending(o => o.CreadaEn).ThenByDescending(o => o.Id).ToList());
    public Task<bool> ExisteAbiertaAsync(string codigoFicha) =>
        Task.FromResult(Datos.Any(o => o.CodigoFicha == codigoFicha && o.Estado is EstadoOrden.Borrador or EstadoOrden.Enviada));
    public Task AgregarAsync(OrdenCompra orden) { orden.Id = Datos.Count + 1; Datos.Add(orden); return Task.CompletedTask; }
    public Task GuardarCambiosAsync() => Task.CompletedTask;
}
