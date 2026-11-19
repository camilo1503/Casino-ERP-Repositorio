using CasinoErp.Dominio.Personal;

namespace CasinoErp.Dominio.Nominas;

/// <summary>
/// Liquida a un empleado según su tipo de contrato. El motor depende de esta abstracción (DIP):
/// agregar un contrato nuevo es una clase más que la implementa, sin tocar el motor (OCP).
/// </summary>
public interface IReglaLiquidacion
{
    bool Aplica(TipoContrato tipo);
    LiquidacionEmpleado Liquidar(Empleado empleado, ParametrosNomina p);
}

/// <summary>Utilidades comunes de las reglas de liquidación.</summary>
internal static class Liquidacion
{
    /// <summary>Redondeo a pesos con MidpointRounding.AwayFromZero (612.818,5 -> 612.819).</summary>
    public static long R(decimal valor) => (long)Math.Round(valor, MidpointRounding.AwayFromZero);

    /// <summary>Liquidación vacía con los datos del empleado copiados.</summary>
    public static LiquidacionEmpleado Para(Empleado empleado) => new()
    {
        EmpleadoDocumento = empleado.Documento,
        EmpleadoNombre = empleado.Nombre,
        Cargo = empleado.Cargo,
        Turno = empleado.Turno,
        TipoContrato = empleado.TipoContrato,
        SalarioBase = empleado.SalarioBase,
    };
}
