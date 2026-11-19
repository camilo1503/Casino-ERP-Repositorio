using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Personal;

namespace CasinoErp.Dominio.Nominas;

/// <summary>
/// Orquesta el cálculo de la nómina (HU-04): carga empleados activos y parámetros del año,
/// ejecuta el motor, persiste el borrador y lo aprueba. No calcula nada por sí mismo (SRP).
/// </summary>
public class NominaService(PayrollEngine motor, IEmpleadoRepositorio empleados, IParametrosNominaRepositorio parametros,
    INominaRepositorio nominas, TimeProvider reloj)
{
    public const string MensajeCalculada = "Nómina calculada";
    public const string MensajeAprobada = "Nómina aprobada";
    public const string MensajeParametros = "Parámetros de nómina guardados";

    public async Task<Nomina> CalcularAsync(int anio, int mes)
    {
        var anterior = await nominas.ObtenerPorPeriodoAsync(anio, mes);
        if (anterior?.Estado == EstadoNomina.Aprobada)
            throw new ReglaNegocioException($"La nómina de {mes:D2}/{anio} ya está aprobada.");

        var p = await parametros.ObtenerAsync(anio)
                ?? throw new ReglaNegocioException($"No hay parámetros de nómina para {anio}.");

        var nomina = motor.Calcular(anio, mes, await empleados.ListarAsync(soloActivos: true), p);

        if (anterior is not null) await nominas.EliminarAsync(anterior);
        nomina.CalculadaEn = reloj.GetUtcNow().UtcDateTime;
        await nominas.AgregarAsync(nomina);
        return nomina;
    }

    public async Task<Nomina> AprobarAsync(int id)
    {
        var nomina = await nominas.ObtenerAsync(id) ?? throw new NoEncontradoException("Nómina no encontrada");
        if (nomina.Estado == EstadoNomina.Aprobada) throw new ReglaNegocioException("La nómina ya está aprobada.");

        nomina.Estado = EstadoNomina.Aprobada;
        nomina.AprobadaEn = reloj.GetUtcNow().UtcDateTime;
        await nominas.GuardarCambiosAsync();
        return nomina;
    }

    public async Task<Nomina> ConsultarAsync(int anio, int mes) =>
        await nominas.ObtenerPorPeriodoAsync(anio, mes) ?? throw new NoEncontradoException("Nómina no encontrada");

    public Task<IReadOnlyList<Nomina>> ListarAsync() => nominas.ListarAsync();

    public async Task<ParametrosNomina> ConsultarParametrosAsync(int anio) =>
        await parametros.ObtenerAsync(anio) ?? throw new NoEncontradoException($"No hay parámetros de nómina para {anio}.");

    public async Task<ParametrosNomina> GuardarParametrosAsync(ParametrosNomina p)
    {
        decimal[] tasas = [p.SaludEmpleado, p.PensionEmpleado, p.FondoSolidaridad, p.RecargoNocturno, p.RetencionServicios,
            p.SaludEmpleador, p.PensionEmpleador, p.Arl, p.CajaCompensacion, p.Icbf, p.Sena];
        if (tasas.Any(t => t is < 0 or > 1)) throw new ReglaNegocioException("Las tasas deben estar entre 0 y 1.");
        if (p.SalarioMinimo <= 0) throw new ReglaNegocioException("El salario mínimo debe ser mayor que cero.");
        if (p.AuxilioTransporte < 0) throw new ReglaNegocioException("El auxilio de transporte no puede ser negativo.");
        if (p.Anio < 2020) throw new ReglaNegocioException("El año no es válido.");

        await parametros.GuardarAsync(p);
        return p;
    }
}
