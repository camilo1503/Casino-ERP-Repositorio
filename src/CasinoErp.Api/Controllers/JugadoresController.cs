using CasinoErp.Dominio.Jugadores;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record RegistrarJugadorRequest(string Documento, string Nombre, string Correo, DateOnly FechaNacimiento, string? TipoDocumento);
public record PerfilJugadorRequest(string? Telefono, string? Ciudad, string Franja, long ApuestaPromedio, List<string> Preferencias);
public record ActualizarJugadorRequest(string Nombre, string Correo);
public record MotivoRequest(string? Motivo);

[ApiController]
[Route("api/jugadores")]
public class JugadoresController(JugadorService servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Jugador>> Listar([FromQuery] string? busqueda) => servicio.ListarAsync(busqueda);

    [HttpGet("{documento}")]
    public Task<Jugador> Consultar(string documento) => servicio.ConsultarAsync(documento);

    [HttpPost]
    public async Task<ActionResult<Respuesta<Jugador>>> Registrar(RegistrarJugadorRequest r)
    {
        var jugador = await servicio.RegistrarAsync(r.Documento, r.Nombre, r.Correo, r.FechaNacimiento, r.TipoDocumento ?? "CC");
        return CreatedAtAction(nameof(Consultar), new { documento = jugador.Documento },
            new Respuesta<Jugador>(JugadorService.MensajeRegistrado, jugador));
    }

    [HttpPut("{documento}")]
    public async Task<Respuesta<Jugador>> Actualizar(string documento, ActualizarJugadorRequest r) =>
        new(JugadorService.MensajeActualizado, await servicio.ActualizarAsync(documento, r.Nombre, r.Correo));

    [HttpPut("{documento}/perfil")]
    public async Task<Respuesta<Jugador>> ActualizarPerfil(string documento, PerfilJugadorRequest r) =>
        new(JugadorService.MensajeActualizado,
            await servicio.ActualizarPerfilAsync(documento, r.Telefono, r.Ciudad, r.Franja, r.ApuestaPromedio, r.Preferencias ?? []));

    [HttpPost("{documento}/desactivar")]
    public async Task<Respuesta<Jugador>> Desactivar(string documento, MotivoRequest r) =>
        new(JugadorService.MensajeDesactivado, await servicio.DesactivarAsync(documento, r.Motivo));

    [HttpPost("{documento}/autoexcluir")]
    public async Task<Respuesta<Jugador>> Autoexcluir(string documento, MotivoRequest r) =>
        new(JugadorService.MensajeAutoexcluido, await servicio.AutoexcluirAsync(documento, r.Motivo));

    [HttpPost("{documento}/reactivar")]
    public async Task<Respuesta<Jugador>> Reactivar(string documento) =>
        new(JugadorService.MensajeReactivado, await servicio.ReactivarAsync(documento));
}
