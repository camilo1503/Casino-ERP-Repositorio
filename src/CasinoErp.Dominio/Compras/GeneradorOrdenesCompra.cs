using CasinoErp.Dominio.Inventario;

namespace CasinoErp.Dominio.Compras;

/// <summary>
/// Observador de stock crítico: cuando el inventario abre una alerta, crea un borrador de orden de compra
/// para reponer la denominación hasta el 140 % del mínimo. Está separado de <see cref="OrdenCompraService"/>
/// para que el inventario no dependa de las órdenes y el contenedor de DI no entre en un ciclo.
/// </summary>
public class GeneradorOrdenesCompra(IOrdenCompraRepositorio ordenes, IProveedorRepositorio proveedores, TimeProvider reloj)
    : IObservadorStockCritico
{
    public async Task StockCriticoAsync(AlertaStock alerta, Ficha ficha)
    {
        if (await ordenes.ExisteAbiertaAsync(alerta.CodigoFicha)) return;

        var cantidad = Math.Max(1, (int)Math.Ceiling(alerta.StockMinimo * 1.4m) - alerta.StockActual);
        var proveedor = (await proveedores.ListarAsync(soloActivos: true)).FirstOrDefault();

        await ordenes.AgregarAsync(new OrdenCompra
        {
            CodigoFicha = alerta.CodigoFicha,
            Cantidad = cantidad,
            ProveedorNit = proveedor?.Nit,
            Origen = OrdenCompra.OrigenAlerta,
            Estado = EstadoOrden.Borrador,
            CreadaEn = reloj.GetUtcNow().UtcDateTime,
        });
    }
}
