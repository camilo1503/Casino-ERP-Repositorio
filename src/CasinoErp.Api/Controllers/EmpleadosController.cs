using CasinoErp.Dominio.Personal;
using Microsoft.AspNetCore.Mvc;
using CasinoErp.Api.Seguridad;
using CasinoErp.Dominio.Seguridad;

namespace CasinoErp.Api.Controllers;

public record RegistrarEmpleadoRequest(string Documento, string Nombre, Cargo Cargo, string? Juego,
    TipoContrato TipoContrato = TipoContrato.Indefinido, long? SalarioBase = null);
public record ActualizarEmpleadoRequest(Turno Turno, string Area, TipoContrato TipoContrato, long SalarioBase);
public record MotivoEmpleadoRequest(string? Motivo);

/// <summary>HU-04 Talento Humano: CRUD de empleados con borrado lógico.</summary>
[ApiController]
[Route("api/empleados")]
[AutorizarModulo(Modulos.Competencias, Modulos.Rrhh, Modulos.Mesas)]
public class EmpleadosController(EmpleadoService servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Empleado>> Listar([FromQuery] bool soloActivos = false) => servicio.ListarAsync(soloActivos);

    [HttpGet("{documento}")]
    public Task<Empleado> Consultar(string documento) => servicio.ConsultarAsync(documento);

    [HttpPost]
    public async Task<ActionResult<Respuesta<Empleado>>> Registrar(RegistrarEmpleadoRequest r)
    {
        var empleado = await servicio.RegistrarAsync(r.Documento, r.Nombre, r.Cargo, r.Juego, r.TipoContrato, r.SalarioBase);
        return CreatedAtAction(nameof(Consultar), new { documento = empleado.Documento },
            new Respuesta<Empleado>(EmpleadoService.MensajeRegistrado, empleado));
    }

    [HttpPut("{documento}")]
    public async Task<Respuesta<Empleado>> Actualizar(string documento, ActualizarEmpleadoRequest r) =>
        new(EmpleadoService.MensajeActualizado, await servicio.ActualizarAsync(documento, r.Turno, r.Area, r.TipoContrato, r.SalarioBase));

    [HttpPost("{documento}/retirar")]
    public async Task<Respuesta<Empleado>> Retirar(string documento, MotivoEmpleadoRequest r) =>
        new(EmpleadoService.MensajeRetirado, await servicio.RetirarAsync(documento, r.Motivo));
}
