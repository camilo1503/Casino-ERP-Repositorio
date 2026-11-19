using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Inventario;

namespace CasinoErp.Dominio.Compras;

/// <summary>
/// Ciclo de vida de las órdenes: Borrador → Enviada → Recibida, o Cancelada desde Borrador o Enviada.
/// Al recibir, registra la entrada en el inventario, lo que atiende la alerta abierta.
/// </summary>
public class OrdenCompraService(IOrdenCompraRepositorio ordenes, IProveedorRepositorio proveedores,
                                InventarioFichasService inventario, TimeProvider reloj)
{
    public const string MensajeCreada = "Orden de compra creada", MensajeEnviada = "Orden enviada",
                        MensajeRecibida = "Orden recibida", MensajeCancelada = "Orden cancelada";

    private const string EstadoInvalido = "La orden no está en un estado válido para esta acción.";

    public async Task<OrdenCompra> CrearManualAsync(string codigoFicha, int cantidad, string? nit)
    {
        var ficha = await inventario.ConsultarAsync(codigoFicha);
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor que cero");
        var proveedor = string.IsNullOrWhiteSpace(nit) ? null : await ProveedorActivoAsync(nit);

        var orden = new OrdenCompra
        {
            CodigoFicha = ficha.Codigo,
            Cantidad = cantidad,
            ProveedorNit = proveedor?.Nit,
            Origen = OrdenCompra.OrigenManual,
            Estado = EstadoOrden.Borrador,
            CreadaEn = Ahora(),
        };
        await ordenes.AgregarAsync(orden);
        return orden;
    }

    public async Task<OrdenCompra> EnviarAsync(int id, string? nit)
    {
        var orden = await ObtenerAsync(id);
        if (orden.Estado != EstadoOrden.Borrador) throw new ReglaNegocioException(EstadoInvalido);
        if (!string.IsNullOrWhiteSpace(nit)) orden.ProveedorNit = (await ProveedorActivoAsync(nit)).Nit;
        if (orden.ProveedorNit is null) throw new ReglaNegocioException("Asigna un proveedor antes de enviar la orden.");
        return await CambiarEstadoAsync(orden, EstadoOrden.Enviada);
    }

    public async Task<OrdenCompra> RecibirAsync(int id)
    {
        var orden = await ObtenerAsync(id);
        if (orden.Estado != EstadoOrden.Enviada) throw new ReglaNegocioException(EstadoInvalido);
        await inventario.RegistrarEntradaAsync(orden.CodigoFicha, orden.Cantidad);
        return await CambiarEstadoAsync(orden, EstadoOrden.Recibida);
    }

    public async Task<OrdenCompra> CancelarAsync(int id)
    {
        var orden = await ObtenerAsync(id);
        if (orden.Estado is not (EstadoOrden.Borrador or EstadoOrden.Enviada)) throw new ReglaNegocioException(EstadoInvalido);
        return await CambiarEstadoAsync(orden, EstadoOrden.Cancelada);
    }

    public Task<IReadOnlyList<OrdenCompra>> ListarAsync() => ordenes.ListarAsync();

    private async Task<OrdenCompra> ObtenerAsync(int id) =>
        await ordenes.ObtenerAsync(id) ?? throw new NoEncontradoException("Orden de compra no encontrada");

    private async Task<Proveedor> ProveedorActivoAsync(string nit)
    {
        var proveedor = await proveedores.ObtenerPorNitAsync(nit.Trim()) ?? throw new NoEncontradoException("Proveedor no encontrado");
        if (!proveedor.Activo) throw new ReglaNegocioException("El proveedor está inactivo.");
        return proveedor;
    }

    private async Task<OrdenCompra> CambiarEstadoAsync(OrdenCompra orden, EstadoOrden estado)
    {
        orden.Estado = estado;
        orden.ActualizadaEn = Ahora();
        await ordenes.GuardarCambiosAsync();
        return orden;
    }

    private DateTime Ahora() => reloj.GetUtcNow().UtcDateTime;
}
