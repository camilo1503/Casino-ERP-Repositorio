using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Identity;

namespace CasinoErp.Infraestructura.Seguridad;

/// <summary>Hash PBKDF2 de ASP.NET Core Identity (formato v3, con sal y versión en el propio hash).</summary>
public class HasherContrasena : IHasherContrasena
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string contrasena) => _hasher.HashPassword(null!, contrasena);

    public bool Verificar(string hash, string contrasena) =>
        _hasher.VerifyHashedPassword(null!, hash, contrasena) != PasswordVerificationResult.Failed;
}
