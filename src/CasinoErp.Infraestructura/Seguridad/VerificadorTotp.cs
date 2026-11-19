using CasinoErp.Dominio.Seguridad;
using Microsoft.Extensions.Configuration;
using OtpNet;

namespace CasinoErp.Infraestructura.Seguridad;

/// <summary>
/// TOTP RFC 6238 de 6 dígitos con Otp.NET (ventana de ±1 paso). Si <c>Seguridad:CodigoTotpDemo</c> está configurado
/// (solo Development y pruebas), ese código también se acepta.
/// </summary>
public class VerificadorTotp(IConfiguration configuracion) : IVerificadorTotp
{
    public string NuevoSecreto() => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public bool Verificar(string secreto, string codigo)
    {
        var demo = configuracion["Seguridad:CodigoTotpDemo"];
        if (!string.IsNullOrEmpty(demo) && demo == codigo) return true;
        if (string.IsNullOrEmpty(secreto) || string.IsNullOrEmpty(codigo)) return false;
        return new Totp(Base32Encoding.ToBytes(secreto)).VerifyTotp(codigo, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }

    public string UriConfiguracion(string secreto, string correo) =>
        $"otpauth://totp/PitBoss:{Uri.EscapeDataString(correo)}?secret={secreto}&issuer=PitBoss";
}
