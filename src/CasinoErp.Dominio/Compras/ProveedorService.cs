using System.Text.RegularExpressions;
using CasinoErp.Dominio.Comun;

namespace CasinoErp.Dominio.Compras;

/// <summary>CRUD de proveedores con borrado lógico (desactivar / reactivar).</summary>
public class ProveedorService(IProveedorRepositorio repo)
{
    public const string MensajeRegistrado = "Proveedor registrado", MensajeActualizado = "Proveedor actualizado",
                        MensajeDesactivado = "Proveedor desactivado", MensajeReactivado = "Proveedor reactivado";

    private static readonly Regex FormatoNit = new(@"^\d{9}-\d$");
    private static readonly Regex FormatoCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public async Task<Proveedor> RegistrarAsync(string nit, string nombre, string correo, string telefono)
    {
        nit = (nit ?? "").Trim();
        if (!FormatoNit.IsMatch(nit)) throw new ReglaNegocioException("El NIT no es válido.");
        var datos = Validar(nombre, correo, telefono);
        if (await repo.ExisteNitAsync(nit)) throw new ReglaNegocioException("Ya existe un proveedor con ese NIT.");

        var proveedor = new Proveedor { Nit = nit, Nombre = datos.Nombre, Correo = datos.Correo, Telefono = datos.Telefono };
        await repo.AgregarAsync(proveedor);
        return proveedor;
    }

    public async Task<Proveedor> ActualizarAsync(string nit, string nombre, string correo, string telefono)
    {
        var datos = Validar(nombre, correo, telefono);
        var proveedor = await ConsultarAsync(nit);
        proveedor.Nombre = datos.Nombre;
        proveedor.Correo = datos.Correo;
        proveedor.Telefono = datos.Telefono;
        await repo.GuardarCambiosAsync();
        return proveedor;
    }

    public async Task<Proveedor> DesactivarAsync(string nit)
    {
        var proveedor = await ConsultarAsync(nit);
        if (!proveedor.Activo) throw new ReglaNegocioException("El proveedor ya está inactivo.");
        proveedor.Activo = false;
        await repo.GuardarCambiosAsync();
        return proveedor;
    }

    public async Task<Proveedor> ReactivarAsync(string nit)
    {
        var proveedor = await ConsultarAsync(nit);
        if (proveedor.Activo) throw new ReglaNegocioException("El proveedor ya está activo.");
        proveedor.Activo = true;
        await repo.GuardarCambiosAsync();
        return proveedor;
    }

    public async Task<Proveedor> ConsultarAsync(string nit) =>
        await repo.ObtenerPorNitAsync((nit ?? "").Trim()) ?? throw new NoEncontradoException("Proveedor no encontrado");

    public Task<IReadOnlyList<Proveedor>> ListarAsync(bool soloActivos = false) => repo.ListarAsync(soloActivos);

    private static (string Nombre, string Correo, string Telefono) Validar(string nombre, string correo, string telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre es obligatorio.");
        correo = (correo ?? "").Trim().ToLowerInvariant();
        if (!FormatoCorreo.IsMatch(correo)) throw new ReglaNegocioException("El correo no es válido.");
        if (string.IsNullOrWhiteSpace(telefono)) throw new ReglaNegocioException("El teléfono es obligatorio.");
        return (nombre.Trim(), correo, telefono.Trim());
    }
}
