using CasinoErp.Dominio.Personal;
using static CasinoErp.Dominio.Nominas.Liquidacion;

namespace CasinoErp.Dominio.Nominas;

/// <summary>
/// Contrato laboral (indefinido o a término fijo): recargo nocturno, auxilio de transporte hasta 2 SMMLV,
/// salud y pensión del empleado, fondo de solidaridad desde 4 SMMLV y aportes del empleador.
/// </summary>
public class ReglaContratoLaboral : IReglaLiquidacion
{
    public bool Aplica(TipoContrato tipo) => tipo is TipoContrato.Indefinido or TipoContrato.TerminoFijo;

    public LiquidacionEmpleado Liquidar(Empleado empleado, ParametrosNomina p)
    {
        var l = Para(empleado);
        var b = empleado.SalarioBase;

        l.RecargoNocturno = empleado.Turno == Turno.Noche ? R(b * p.RecargoNocturno) : 0;
        var ibc = b + l.RecargoNocturno;
        l.AuxilioTransporte = ibc <= 2 * p.SalarioMinimo ? p.AuxilioTransporte : 0;
        l.DeduccionSalud = R(ibc * p.SaludEmpleado);
        l.DeduccionPension = R(ibc * p.PensionEmpleado);
        l.FondoSolidaridad = ibc >= 4 * p.SalarioMinimo ? R(ibc * p.FondoSolidaridad) : 0;
        l.AportesEmpleador = R(ibc * p.SaludEmpleador) + R(ibc * p.PensionEmpleador) + R(ibc * p.Arl)
                             + R(ibc * p.CajaCompensacion) + R(ibc * p.Icbf) + R(ibc * p.Sena);
        l.Neto = ibc + l.AuxilioTransporte - l.DeduccionSalud - l.DeduccionPension - l.FondoSolidaridad;
        return l;
    }
}
