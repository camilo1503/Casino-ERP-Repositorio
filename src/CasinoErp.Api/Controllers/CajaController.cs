using CasinoErp.Dominio.Caja;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record RegistrarTransaccionRequest(TipoTransaccion Tipo, string? Documento, long Monto, long Descuento, string? Caja, string? Ubicacion);
public record CumplimientoRequest(string Estado);

[ApiController]
[Route("api/caja")]
public class CajaController(CajaService servicio) : ControllerBase
{
    [HttpGet("transacciones")]
    public Task<IReadOnlyList<TransaccionCaja>> Listar([FromQuery] string? documento) => servicio.ListarAsync(documento);

    [HttpPost("transacciones")]
    public async Task<Respuesta<TransaccionCaja>> Registrar(RegistrarTransaccionRequest r)
    {
        var transaccion = await servicio.RegistrarAsync(r.Tipo, r.Documento, r.Monto, r.Descuento, r.Caja ?? "Caja 2", r.Ubicacion);
        return new(CajaService.Mensaje(transaccion), transaccion);
    }

    [HttpPost("transacciones/{id:int}/aprobar")]
    public async Task<Respuesta<TransaccionCaja>> Aprobar(int id) =>
        new(CajaService.MensajeAprobadaRevision, await servicio.AprobarRevisionAsync(id));

    [HttpPost("transacciones/{id:int}/anular")]
    public async Task<Respuesta<TransaccionCaja>> Anular(int id) =>
        new(CajaService.MensajeAnulada, await servicio.AnularAsync(id));

    [HttpPost("transacciones/{id:int}/cumplimiento")]
    public async Task<Respuesta<TransaccionCaja>> Cumplimiento(int id, CumplimientoRequest r) =>
        new($"Transacción {r.Estado.ToLowerInvariant()}", await servicio.CambiarEstadoCumplimientoAsync(id, r.Estado));

    [HttpGet("denominaciones")]
    public IReadOnlyList<long> Denominaciones() => CajaService.Denominaciones;

    [HttpGet("total")]
    public object Total([FromQuery] long denominacion, [FromQuery] int cantidad) =>
        new { total = CajaService.CalcularTotal(denominacion, cantidad) };
}
