using CasinoErp.Dominio.Personal;
using static CasinoErp.Dominio.Nominas.Liquidacion;

namespace CasinoErp.Dominio.Nominas;

/// <summary>Prestación de servicios: solo se retiene en la fuente; sin recargos, auxilio, aportes ni parafiscales.</summary>
public class ReglaPrestacionServicios : IReglaLiquidacion
{
    public bool Aplica(TipoContrato tipo) => tipo == TipoContrato.PrestacionServicios;

    public LiquidacionEmpleado Liquidar(Empleado empleado, ParametrosNomina p)
    {
        var l = Para(empleado);
        l.RetencionFuente = R(empleado.SalarioBase * p.RetencionServicios);
        l.Neto = empleado.SalarioBase - l.RetencionFuente;
        return l;
    }
}
