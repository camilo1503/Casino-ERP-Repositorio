using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Retencion;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record RespuestaOfertaRequest(string Documento, int MinutosSesion, string Senal, string? Origen, bool Aceptada, TipoOferta? Tipo);
public record DescuentoRequest(long Monto);

[ApiController]
[Route("api/retencion")]
public class RetencionController(RetencionService servicio) : ControllerBase
{
    [HttpGet("ofertas")]
    public Task<IReadOnlyList<OfertaRetencion>> Listar() => servicio.ListarAsync();

    /// <summary>Registra la respuesta del jugador a la oferta de retención (aceptada o rechazada).</summary>
    [HttpPost("ofertas")]
    public async Task<Respuesta<OfertaRetencion>> Registrar(RespuestaOfertaRequest r)
    {
        var oferta = await servicio.RegistrarRespuestaAsync(r.Documento, r.MinutosSesion, r.Senal, r.Origen ?? "Mesa", r.Aceptada, r.Tipo);
        return new(r.Aceptada ? RetencionService.MensajeEmitida : RetencionService.MensajeRechazada, oferta);
    }

    [HttpPost("descuento")]
    public async Task<Respuesta<long>> CalcularDescuento(DescuentoRequest r)
    {
        var descuento = await servicio.CalcularDescuentoAsync(r.Monto);
        return new($"Descuento de {Pesos.Formatear(descuento)}", descuento);
    }

    [HttpGet("configuracion")]
    public Task<ConfiguracionRetencion> Configuracion() => servicio.ConsultarConfiguracionAsync();

    [HttpPut("configuracion")]
    public async Task<Respuesta<ConfiguracionRetencion>> ActualizarConfiguracion(ConfiguracionRetencion config) =>
        new(RetencionService.MensajeConfiguracion, await servicio.ActualizarConfiguracionAsync(config));
}
