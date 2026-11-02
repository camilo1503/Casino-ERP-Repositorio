using System.Globalization;

namespace CasinoErp.Dominio.Comun;

public static class Pesos
{
    private static readonly NumberFormatInfo Formato = new() { NumberGroupSeparator = ".", NumberDecimalDigits = 0 };

    /// <summary>Formatea pesos colombianos: 200000 -> "$200.000".</summary>
    public static string Formatear(long valor) => "$" + Math.Abs(valor).ToString("N0", Formato);
}
