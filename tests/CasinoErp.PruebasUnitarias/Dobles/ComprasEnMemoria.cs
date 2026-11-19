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
