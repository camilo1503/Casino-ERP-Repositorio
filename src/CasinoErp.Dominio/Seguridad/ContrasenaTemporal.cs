using System.Security.Cryptography;

namespace CasinoErp.Dominio.Seguridad;

/// <summary>Contraseñas temporales y reglas de fortaleza (pwRules del mockup).</summary>
public static class ContrasenaTemporal
{
    private const string Mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Minusculas = "abcdefghijkmnopqrstuvwxyz";
    private const string Digitos = "23456789";
    private const string Especiales = "!@#$%&*?";
    private const int Longitud = 12;

    /// <summary>12 caracteres con al menos una mayúscula, una minúscula, un dígito y un carácter de <c>!@#$%&amp;*?</c>.</summary>
    public static string Generar()
    {
        var caracteres = new char[Longitud];
        caracteres[0] = Elegir(Mayusculas);
        caracteres[1] = Elegir(Minusculas);
        caracteres[2] = Elegir(Digitos);
        caracteres[3] = Elegir(Especiales);
        const string todos = Mayusculas + Minusculas + Digitos + Especiales;
        for (var i = 4; i < Longitud; i++) caracteres[i] = Elegir(todos);
        RandomNumberGenerator.Shuffle(caracteres.AsSpan());
        return new string(caracteres);
    }

    /// <summary>Al menos 8 caracteres, una mayúscula, un dígito y un carácter no alfanumérico.</summary>
    public static bool EsSegura(string contrasena) =>
        contrasena.Length >= 8
        && contrasena.Any(char.IsUpper)
        && contrasena.Any(char.IsDigit)
        && contrasena.Any(c => !char.IsLetterOrDigit(c));

    private static char Elegir(string conjunto) => conjunto[RandomNumberGenerator.GetInt32(conjunto.Length)];
}
