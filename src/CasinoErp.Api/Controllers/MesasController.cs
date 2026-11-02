using CasinoErp.Dominio.Mesas;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record RegistrarMesaRequest(string Numero, string Juego, long ApuestaMinima, long ApuestaMaxima, string? Zona);
public record LimitesRequest(long ApuestaMinima, long ApuestaMaxima, string? Zona);

[ApiController]
[Route("api/mesas")]
public class MesasController(MesaService servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Mesa>> Listar() => servicio.ListarAsync();

    [HttpGet("juegos")]
    public IReadOnlyList<string> Juegos() => MesaService.Juegos;

    [HttpGet("{numero}")]
    public Task<Mesa> Consultar(string numero) => servicio.ConsultarAsync(numero);

    [HttpPost]
    public async Task<ActionResult<Respuesta<Mesa>>> Registrar(RegistrarMesaRequest r)
    {
        var mesa = await servicio.RegistrarAsync(r.Numero, r.Juego, r.ApuestaMinima, r.ApuestaMaxima, r.Zona ?? "Zona A");
        return CreatedAtAction(nameof(Consultar), new { numero = mesa.Numero }, new Respuesta<Mesa>(MesaService.MensajeRegistrada, mesa));
    }

    [HttpPut("{numero}/limites")]
    public async Task<Respuesta<Mesa>> ActualizarLimites(string numero, LimitesRequest r) =>
        new(MesaService.MensajeActualizada, await servicio.ActualizarLimitesAsync(numero, r.ApuestaMinima, r.ApuestaMaxima, r.Zona));

    [HttpPost("{numero}/abrir")]
    public async Task<Respuesta<Mesa>> Abrir(string numero) => new(MesaService.MensajeAbierta, await servicio.AbrirAsync(numero));

    [HttpPost("{numero}/cerrar")]
    public async Task<Respuesta<Mesa>> Cerrar(string numero) => new(MesaService.MensajeCerrada, await servicio.CerrarAsync(numero));

    [HttpPost("{numero}/mantenimiento")]
    public async Task<Respuesta<Mesa>> Mantenimiento(string numero) =>
        new(MesaService.MensajeMantenimiento, await servicio.EnviarAMantenimientoAsync(numero));

    [HttpPost("{numero}/reparada")]
    public async Task<Respuesta<Mesa>> Reparada(string numero) =>
        new(MesaService.MensajeReparada, await servicio.MarcarReparadaAsync(numero));

    [HttpPost("{numero}/retirar")]
    public async Task<Respuesta<Mesa>> Retirar(string numero, MotivoRequest r) =>
        new(MesaService.MensajeRetirada, await servicio.RetirarAsync(numero, r.Motivo));
}
