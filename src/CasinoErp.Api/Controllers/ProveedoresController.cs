using CasinoErp.Api.Seguridad;
using CasinoErp.Dominio.Compras;
using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

public record ProveedorRequest(string Nit, string Nombre, string Correo, string Telefono);

/// <summary>HU-05: CRUD de proveedores de fichas con borrado lógico.</summary>
[ApiController]
[Route("api/proveedores")]
[AutorizarModulo(Modulos.Proveedores)]
public class ProveedoresController(ProveedorService servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Proveedor>> Listar([FromQuery] bool soloActivos = false) => servicio.ListarAsync(soloActivos);

    [HttpGet("{nit}")]
    public Task<Proveedor> Consultar(string nit) => servicio.ConsultarAsync(nit);

    [HttpPost]
    public async Task<ActionResult<Respuesta<Proveedor>>> Registrar(ProveedorRequest r)
    {
        var proveedor = await servicio.RegistrarAsync(r.Nit, r.Nombre, r.Correo, r.Telefono);
        return CreatedAtAction(nameof(Consultar), new { nit = proveedor.Nit }, new Respuesta<Proveedor>(ProveedorService.MensajeRegistrado, proveedor));
    }

    /// <summary>El NIT del cuerpo se ignora: manda el de la ruta.</summary>
    [HttpPut("{nit}")]
    public async Task<Respuesta<Proveedor>> Actualizar(string nit, ProveedorRequest r) =>
        new(ProveedorService.MensajeActualizado, await servicio.ActualizarAsync(nit, r.Nombre, r.Correo, r.Telefono));

    [HttpPost("{nit}/desactivar")]
    public async Task<Respuesta<Proveedor>> Desactivar(string nit) =>
        new(ProveedorService.MensajeDesactivado, await servicio.DesactivarAsync(nit));

    [HttpPost("{nit}/reactivar")]
    public async Task<Respuesta<Proveedor>> Reactivar(string nit) =>
        new(ProveedorService.MensajeReactivado, await servicio.ReactivarAsync(nit));
}
