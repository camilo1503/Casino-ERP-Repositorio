using CasinoErp.Api.Seguridad;
using CasinoErp.Dominio.Compras;
using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record OrdenManualRequest(string CodigoFicha, int Cantidad, string? Nit);
public record EnviarOrdenRequest(string? Nit);

/// <summary>HU-05: órdenes de compra de fichas (las crea una alerta de stock crítico o un usuario).</summary>
[ApiController]
[Route("api/compras/ordenes")]
[AutorizarModulo(Modulos.Proveedores, Modulos.Inventario)]
public class ComprasController(OrdenCompraService servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OrdenCompra>> Listar() => servicio.ListarAsync();

    [HttpPost]
    public async Task<ActionResult<Respuesta<OrdenCompra>>> CrearManual(OrdenManualRequest r)
    {
        var orden = await servicio.CrearManualAsync(r.CodigoFicha, r.Cantidad, r.Nit);
        return StatusCode(StatusCodes.Status201Created, new Respuesta<OrdenCompra>(OrdenCompraService.MensajeCreada, orden));
    }

    [HttpPost("{id:int}/enviar")]
    public async Task<Respuesta<OrdenCompra>> Enviar(int id, EnviarOrdenRequest? r) =>
        new(OrdenCompraService.MensajeEnviada, await servicio.EnviarAsync(id, r?.Nit));

    [HttpPost("{id:int}/recibir")]
    public async Task<Respuesta<OrdenCompra>> Recibir(int id) =>
        new(OrdenCompraService.MensajeRecibida, await servicio.RecibirAsync(id));

    [HttpPost("{id:int}/cancelar")]
    public async Task<Respuesta<OrdenCompra>> Cancelar(int id) =>
        new(OrdenCompraService.MensajeCancelada, await servicio.CancelarAsync(id));
}
