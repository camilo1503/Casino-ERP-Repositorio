using System.Security.Claims;
using System.Text;
using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CasinoErp.Api.Seguridad;

/// <summary>Emite el JWT de la sesión (HS256, vigencia de un turno) y valida en cada petición que siga vigente.</summary>
public class JwtEmisor(IOptions<OpcionesJwt> opciones, TimeProvider reloj)
{
    public const string ClaimId = "sub", ClaimNombre = "name", ClaimCorreo = "email";

    /// <summary>Claims: <c>sub</c> = Id, <c>name</c> = Nombre, <c>email</c> = Correo y <see cref="ClaimTypes.Role"/> = Rol.</summary>
    public (string Token, DateTime Expira) Emitir(Usuario usuario)
    {
        var o = opciones.Value;
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddHours(o.HorasVigencia);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = o.Emisor,
            Audience = o.Audiencia,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimId, usuario.Id.ToString()),
                new Claim(ClaimNombre, usuario.Nombre),
                new Claim(ClaimCorreo, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            ]),
            SigningCredentials = new SigningCredentials(Llave(o), SecurityAlgorithms.HmacSha256),
        };
        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }

    public static SymmetricSecurityKey Llave(OpcionesJwt o) => new(Encoding.UTF8.GetBytes(o.Clave));

    /// <summary>
    /// Un token deja de servir si el usuario ya no existe, está desactivado o su rol cambió
    /// (revocación efectiva consultando la base en cada petición).
    /// </summary>
    public static async Task ValidarUsuarioVigente(TokenValidatedContext ctx)
    {
        var principal = ctx.Principal;
        if (!int.TryParse(principal?.FindFirst(ClaimId)?.Value, out var id))
        {
            ctx.Fail("Sesión no vigente");
            return;
        }

        var usuarios = ctx.HttpContext.RequestServices.GetRequiredService<IUsuarioRepositorio>();
        var u = await usuarios.ObtenerAsync(id);
        if (u is null || !u.Activo || u.Rol.ToString() != principal!.FindFirst(ClaimTypes.Role)?.Value)
            ctx.Fail("Sesión no vigente");
    }
}
