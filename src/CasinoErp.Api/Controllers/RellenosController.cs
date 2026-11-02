using CasinoErp.Dominio.Rellenos;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record SolicitarRellenoRequest(string MesaNumero, long Monto, string? SolicitadoPor);
public record AprobarRellenoRequest(string? AprobadoPor);
public record RechazarRellenoRequest(string? Motivo, string? RechazadoPor);
public record DepositoRequest(long Monto);

[ApiController]
[Route("api")]
public class RellenosController(RellenoService servicio) : ControllerBase
{
    [HttpGet("rellenos")]
    public Task<IReadOnlyList<Relleno>> Listar() => servicio.ListarAsync();

    [HttpPost("rellenos")]
    public async Task<Respuesta<Relleno>> Solicitar(SolicitarRellenoRequest r) =>
        new(RellenoService.MensajeSolicitado, await servicio.SolicitarAsync(r.MesaNumero, r.Monto, r.SolicitadoPor));

    [HttpPost("rellenos/{id:int}/aprobar")]
    public async Task<Respuesta<Relleno>> Aprobar(int id, AprobarRellenoRequest r) =>
        new(RellenoService.MensajeAprobado, await servicio.AprobarAsync(id, r.AprobadoPor));

    [HttpPost("rellenos/{id:int}/rechazar")]
    public async Task<Respuesta<Relleno>> Rechazar(int id, RechazarRellenoRequest r) =>
        new(RellenoService.MensajeRechazado, await servicio.RechazarAsync(id, r.Motivo, r.RechazadoPor));

    [HttpGet("boveda")]
    public Task<Boveda> Boveda() => servicio.ConsultarBovedaAsync();

    [HttpPost("boveda/depositos")]
    public async Task<Respuesta<Boveda>> Depositar(DepositoRequest r) =>
        new(RellenoService.MensajeDeposito, await servicio.DepositarAsync(r.Monto));
}
