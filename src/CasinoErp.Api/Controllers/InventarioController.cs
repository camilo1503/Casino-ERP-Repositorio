using CasinoErp.Dominio.Inventario;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record RegistrarFichaRequest(string Codigo, long Denominacion, string Categoria, int StockInicial);
public record CantidadRequest(int Cantidad);

[ApiController]
[Route("api/inventario")]
public class InventarioController(InventarioFichasService servicio) : ControllerBase
{
    [HttpGet("fichas")]
    public Task<IReadOnlyList<Ficha>> Listar() => servicio.ListarAsync();

    [HttpGet("fichas/{codigo}")]
    public Task<Ficha> Consultar(string codigo) => servicio.ConsultarAsync(codigo);

    [HttpPost("fichas")]
    public async Task<ActionResult<Respuesta<Ficha>>> Registrar(RegistrarFichaRequest r)
    {
        var ficha = await servicio.RegistrarAsync(r.Codigo, r.Denominacion, r.Categoria, r.StockInicial);
        return CreatedAtAction(nameof(Consultar), new { codigo = ficha.Codigo }, new Respuesta<Ficha>(InventarioFichasService.MensajeRegistrada, ficha));
    }

    [HttpPost("fichas/{codigo}/entradas")]
    public async Task<Respuesta<Ficha>> Entrada(string codigo, CantidadRequest r) =>
        new(InventarioFichasService.MensajeEntrada, await servicio.RegistrarEntradaAsync(codigo, r.Cantidad));

    [HttpPost("fichas/{codigo}/retiros")]
    public async Task<Respuesta<Ficha>> Retiro(string codigo, CantidadRequest r) =>
        new(InventarioFichasService.MensajeRetiro, await servicio.RetirarAsync(codigo, r.Cantidad));

    [HttpGet("movimientos")]
    public Task<IReadOnlyList<MovimientoInventario>> Movimientos([FromQuery] string? codigo) => servicio.ListarMovimientosAsync(codigo);
}
