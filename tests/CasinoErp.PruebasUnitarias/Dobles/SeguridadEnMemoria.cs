using CasinoErp.Dominio.Seguridad;

namespace CasinoErp.PruebasUnitarias.Dobles;

/// <summary>Repositorio de usuarios en memoria para las pruebas de Seguridad (HU-06).</summary>
public class UsuariosEnMemoria : IUsuarioRepositorio
{
    public readonly List<Usuario> Datos = [];
    public int Guardados { get; private set; }

    public Task<Usuario?> ObtenerAsync(int id) => Task.FromResult(Datos.FirstOrDefault(u => u.Id == id));
    public Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
        Task.FromResult(Datos.FirstOrDefault(u => u.Correo == correo.ToLowerInvariant()));
    public Task<bool> ExisteCorreoAsync(string correo) => Task.FromResult(Datos.Any(u => u.Correo == correo.ToLowerInvariant()));
    public Task<IReadOnlyList<Usuario>> ListarAsync() => Task.FromResult<IReadOnlyList<Usuario>>(Datos.OrderBy(u => u.Id).ToList());
    public Task AgregarAsync(Usuario usuario) { usuario.Id = Datos.Count + 1; Datos.Add(usuario); Guardados++; return Task.CompletedTask; }
    public Task GuardarCambiosAsync() { Guardados++; return Task.CompletedTask; }
}

/// <summary>Hash predecible: <c>Hash(x) = "H:" + x</c>.</summary>
public class HasherFalso : IHasherContrasena
{
    public string Hash(string contrasena) => "H:" + contrasena;
    public bool Verificar(string hash, string contrasena) => hash == "H:" + contrasena;
}

/// <summary>TOTP falso: solo acepta el código de demo <c>482913</c>.</summary>
public class TotpFalso : IVerificadorTotp
{
    public const string CodigoValido = "482913";
    private int _secretos;

    public string NuevoSecreto() => $"SECRETO{++_secretos}";
    public bool Verificar(string secreto, string codigo) => codigo == CodigoValido;
    public string UriConfiguracion(string secreto, string correo) => $"otpauth://totp/PitBoss:{correo}?secret={secreto}&issuer=PitBoss";
}

/// <summary>Reloj que arranca en 2026-10-26 15:00 UTC (igual que <see cref="RelojFijo"/>) y se puede avanzar.</summary>
public class RelojMovible : TimeProvider
{
    public DateTimeOffset Ahora { get; set; } = new(2026, 10, 26, 15, 0, 0, TimeSpan.Zero);

    public void Avanzar(TimeSpan tiempo) => Ahora += tiempo;
    public override DateTimeOffset GetUtcNow() => Ahora;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
