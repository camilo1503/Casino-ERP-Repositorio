namespace CasinoErp.Dominio.Seguridad;

/// <summary>Cuenta de acceso al ERP (HU-06, épica E-06 del mockup).</summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";              // minúsculas
    public Rol Rol { get; set; }
    public bool Activo { get; set; } = true;
    public string HashContrasena { get; set; } = "";
    public string SecretoTotp { get; set; } = "";
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public bool DebeCambiarContrasena { get; set; }
    public string? EmpleadoDocumento { get; set; }
    public DateTime? UltimoIngreso { get; set; }
}

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerAsync(int id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);   // compara en minúsculas
    Task<bool> ExisteCorreoAsync(string correo);
    Task<IReadOnlyList<Usuario>> ListarAsync();             // por Id
    Task AgregarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}

/// <summary>Hash de contraseñas (en Infraestructura: PBKDF2 de ASP.NET Identity).</summary>
public interface IHasherContrasena { string Hash(string contrasena); bool Verificar(string hash, string contrasena); }

/// <summary>Segundo factor TOTP de 6 dígitos (en Infraestructura: Otp.NET).</summary>
public interface IVerificadorTotp
{
    string NuevoSecreto();                                  // Base32
    bool Verificar(string secreto, string codigo);
    string UriConfiguracion(string secreto, string correo); // otpauth://totp/PitBoss:{correo}?secret={secreto}&issuer=PitBoss
}
