using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Personal;

namespace CasinoErp.Dominio.Nominas;

/// <summary>
/// Motor de nómina (HU-04). Solo calcula: no usa base de datos ni reloj (SRP).
/// Depende de la abstracción <see cref="IReglaLiquidacion"/>, inyectada como colección (DIP);
/// un tipo de contrato nuevo es una regla más, sin modificar esta clase (OCP).
/// </summary>
public class PayrollEngine(IEnumerable<IReglaLiquidacion> reglas)
{
    private readonly IReadOnlyList<IReglaLiquidacion> _reglas = reglas.ToList();

    public Nomina Calcular(int anio, int mes, IEnumerable<Empleado> empleados, ParametrosNomina parametros)
    {
        if (mes is < 1 or > 12) throw new ReglaNegocioException("El mes debe estar entre 1 y 12.");

        var activos = empleados.Where(e => e.Activo).ToList();
        if (activos.Count == 0) throw new ReglaNegocioException("No hay empleados activos para liquidar.");

        var bajoElMinimo = activos.FirstOrDefault(e =>
            e.TipoContrato != TipoContrato.PrestacionServicios && e.SalarioBase < parametros.SalarioMinimo);
        if (bajoElMinimo is not null)
            throw new ReglaNegocioException($"El salario de {bajoElMinimo.Nombre} es menor al salario mínimo de {anio}.");

        var detalle = activos
            .Select(e => ReglaPara(e.TipoContrato).Liquidar(e, parametros))
            .OrderBy(l => l.EmpleadoNombre, StringComparer.CurrentCulture)
            .ToList();

        return new Nomina { Anio = anio, Mes = mes, Estado = EstadoNomina.Borrador, Detalle = detalle };
    }

    /// <summary>Que falte una regla es un error de configuración, no de negocio.</summary>
    private IReglaLiquidacion ReglaPara(TipoContrato tipo) =>
        _reglas.FirstOrDefault(r => r.Aplica(tipo))
        ?? throw new InvalidOperationException($"No hay regla de liquidación para {tipo}");
}
