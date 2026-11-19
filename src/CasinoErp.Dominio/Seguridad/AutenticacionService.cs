using CasinoErp.Dominio.Comun;

namespace CasinoErp.Dominio.Seguridad;

/// <summary>Login con contraseña y TOTP, con bloqueo de 15 minutos tras 5 intentos fallidos (HU-06, loginView del mockup).</summary>
public class AutenticacionService(IUsuarioRepositorio usuarios, IHasherContrasena hasher, IVerificadorTotp totp, TimeProvider reloj)
{
    public const int MaximoIntentos = 5;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private const string MensajeCredenciales = "Correo o contraseña incorrectos.";
    private const string MensajeBloqueada = "Cuenta bloqueada 15 minutos por 5 intentos fallidos. Un administrador puede restablecerla.";

    public async Task<Usuario> IniciarSesionAsync(string correo, string contrasena, string codigo)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var u = await usuarios.ObtenerPorCorreoAsync(correo.Trim().ToLowerInvariant())
            ?? throw new AutenticacionException(MensajeCredenciales);

        if (u.BloqueadoHasta > ahora) throw new AutenticacionException(MensajeBloqueada);
        if (u.BloqueadoHasta <= ahora)
        {
            u.BloqueadoHasta = null;
            u.IntentosFallidos = 0;
        }

        if (!u.Activo) throw new AutenticacionException("Esta cuenta está desactivada.");

        if (!hasher.Verificar(u.HashContrasena, contrasena))
        {
            var bloqueada = await Fallar(u, ahora);
            throw new AutenticacionException(bloqueada ? MensajeBloqueada : MensajeCredenciales);
        }

        if (!totp.Verificar(u.SecretoTotp, codigo))
        {
            var bloqueada = await Fallar(u, ahora);
            throw new AutenticacionException(bloqueada
                ? "Código incorrecto. Cuenta bloqueada 15 minutos."
                : $"Código incorrecto. Intento {u.IntentosFallidos} de 5.");
        }

        u.IntentosFallidos = 0;
        u.UltimoIngreso = ahora;
        await usuarios.GuardarCambiosAsync();
        return u;
    }

    /// <summary>Suma un intento; al quinto bloquea la cuenta y reinicia el contador. Devuelve si quedó bloqueada.</summary>
    private async Task<bool> Fallar(Usuario u, DateTime ahora)
    {
        u.IntentosFallidos++;
        var bloqueada = u.IntentosFallidos >= MaximoIntentos;
        if (bloqueada)
        {
            u.BloqueadoHasta = ahora + DuracionBloqueo;
            u.IntentosFallidos = 0;
        }
        await usuarios.GuardarCambiosAsync();
        return bloqueada;
    }
}
