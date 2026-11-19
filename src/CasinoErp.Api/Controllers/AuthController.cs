using CasinoErp.Api.Seguridad;
using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasinoErp.Api.Controllers;

/// <summary>HU-06 · Login con 2FA, sesión actual y cambio de la propia contraseña.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController(AutenticacionService autenticacion, UsuarioService usuarios, JwtEmisor emisor, TimeProvider reloj) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<LoginResponse> Login(LoginRequest r)
    {
        var u = await autenticacion.IniciarSesionAsync(r.Correo ?? "", r.Contrasena ?? "", r.Codigo ?? "");
        var (token, expira) = emisor.Emitir(u);
        return new LoginResponse(token, expira, UsuarioDto.De(u, Ahora), MatrizAcceso.ModulosDe(u.Rol).ToList());
    }

    [HttpGet("yo")]
    [Authorize]
    public async Task<object> Yo()
    {
        var u = await usuarios.ConsultarAsync(IdSesion());
        return new { usuario = UsuarioDto.De(u, Ahora), modulos = MatrizAcceso.ModulosDe(u.Rol).ToList() };
    }

    /// <summary>
    /// Una contraseña actual errada responde 400 y no 401: el front cierra la sesión ante cualquier 401
    /// (PitBossApi.alNoAutorizado), y aquí el token sigue siendo válido.
    /// </summary>
    [HttpPost("cambiar-contrasena")]
    [Authorize]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaRequest r)
    {
        try
        {
            await usuarios.CambiarContrasenaAsync(IdSesion(), r.Actual ?? "", r.Nueva ?? "");
        }
        catch (AutenticacionException e)
        {
            return BadRequest(new { mensaje = e.Message });
        }
        return Ok(new { mensaje = UsuarioService.MensajeContrasena });
    }

    private DateTime Ahora => reloj.GetUtcNow().UtcDateTime;

    private int IdSesion() =>
        int.TryParse(User.FindFirst(JwtEmisor.ClaimId)?.Value, out var id) ? id : throw new AutenticacionException("Sesión no vigente");
}
