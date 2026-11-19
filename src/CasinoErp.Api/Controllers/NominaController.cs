using CasinoErp.Dominio.Nominas;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record CalcularNominaRequest(int Anio, int Mes);

/// <summary>HU-04 Cálculo de nómina: calcular, consultar, aprobar y administrar los parámetros del año.</summary>
[ApiController]
[Route("api/nomina")]
public class NominaController(NominaService servicio) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<Respuesta<Nomina>> Calcular(CalcularNominaRequest r) =>
        new(NominaService.MensajeCalculada, await servicio.CalcularAsync(r.Anio, r.Mes));

    [HttpGet]
    public Task<IReadOnlyList<Nomina>> Listar() => servicio.ListarAsync();

    [HttpGet("{anio:int}/{mes:int}")]
    public Task<Nomina> Consultar(int anio, int mes) => servicio.ConsultarAsync(anio, mes);

    [HttpPost("{id:int}/aprobar")]
    public async Task<Respuesta<Nomina>> Aprobar(int id) =>
        new(NominaService.MensajeAprobada, await servicio.AprobarAsync(id));

    [HttpGet("parametros/{anio:int}")]
    public Task<ParametrosNomina> ConsultarParametros(int anio) => servicio.ConsultarParametrosAsync(anio);

    [HttpPut("parametros/{anio:int}")]
    public async Task<Respuesta<ParametrosNomina>> GuardarParametros(int anio, ParametrosNomina p)
    {
        p.Anio = anio;
        return new(NominaService.MensajeParametros, await servicio.GuardarParametrosAsync(p));
    }
}
