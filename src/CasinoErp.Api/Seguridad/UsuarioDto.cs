using CasinoErp.Dominio.Seguridad;

namespace CasinoErp.Api.Seguridad;

/// <summary>Lo que la API muestra de una cuenta: nunca el hash de la contraseña ni el secreto TOTP.</summary>
public record UsuarioDto(int Id, string Nombre, string Correo, Rol Rol, bool Activo, bool Bloqueado, string? EmpleadoDocumento,
                         DateTime? UltimoIngreso, bool DebeCambiarContrasena)
{
    public static UsuarioDto De(Usuario u, DateTime ahora) =>
        new(u.Id, u.Nombre, u.Correo, u.Rol, u.Activo, u.BloqueadoHasta > ahora, u.EmpleadoDocumento, u.UltimoIngreso, u.DebeCambiarContrasena);
}

public record LoginRequest(string Correo, string Contrasena, string Codigo);
public record LoginResponse(string Token, DateTime Expira, UsuarioDto Usuario, IReadOnlyCollection<string> Modulos);
public record CrearUsuarioRequest(string Nombre, string Correo, Rol Rol, string? EmpleadoDocumento);
public record CambiarRolRequest(Rol Rol);
public record CambiarContrasenaRequest(string Actual, string Nueva);
