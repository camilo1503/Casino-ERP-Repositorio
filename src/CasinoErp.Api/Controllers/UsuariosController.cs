using CasinoErp.Api.Seguridad;
using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

/// <summary>HU-06 · Usuarios y roles (épica E-06 del mockup). Solo el rol con el módulo "usuarios" (Administrador).</summary>
[ApiController]
[Route("api/usuarios")]
[AutorizarModulo(Modulos.Usuarios)]
public class UsuariosController(UsuarioService servicio, TimeProvider reloj) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<UsuarioDto>> Listar() =>
        (await servicio.ListarAsync()).Select(Dto).ToList();

    [HttpPost]
    public async Task<IActionResult> Crear(CrearUsuarioRequest r)
    {
        var creado = await servicio.CrearAsync(r.Nombre ?? "", r.Correo ?? "", r.Rol, r.EmpleadoDocumento);
        var datos = new { usuario = Dto(creado.Usuario), contrasenaTemporal = creado.ContrasenaTemporal, uriTotp = creado.UriTotp };
        return StatusCode(StatusCodes.Status201Created, new Respuesta<object>(UsuarioService.MensajeCreado, datos));
    }

    [HttpPut("{id:int}/rol")]
    public async Task<Respuesta<UsuarioDto>> CambiarRol(int id, CambiarRolRequest r) =>
        new(UsuarioService.MensajeRol, Dto(await servicio.CambiarRolAsync(id, r.Rol)));

    [HttpPost("{id:int}/restablecer")]
    public async Task<Respuesta<object>> Restablecer(int id)
    {
        var r = await servicio.RestablecerContrasenaAsync(id);
        return new(UsuarioService.MensajeRestablecida, new { usuario = Dto(r.Usuario), contrasenaTemporal = r.ContrasenaTemporal });
    }

    [HttpPost("{id:int}/desactivar")]
    public async Task<Respuesta<UsuarioDto>> Desactivar(int id) =>
        new(UsuarioService.MensajeDesactivado, Dto(await servicio.DesactivarAsync(id)));

    [HttpPost("{id:int}/reactivar")]
    public async Task<Respuesta<UsuarioDto>> Reactivar(int id) =>
        new(UsuarioService.MensajeReactivado, Dto(await servicio.ReactivarAsync(id)));

    private UsuarioDto Dto(Usuario u) => UsuarioDto.De(u, reloj.GetUtcNow().UtcDateTime);
}
