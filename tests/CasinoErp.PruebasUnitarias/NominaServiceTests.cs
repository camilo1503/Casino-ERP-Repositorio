using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Nominas;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-04 · Servicio de nómina: orquesta el motor, los parámetros del año y la persistencia (Plan-Felipe F3).</summary>
public class NominaServiceTests
{
    private static readonly DateTime Ahora = new(2026, 10, 26, 15, 0, 0, DateTimeKind.Utc);

    private readonly EmpleadosEnMemoria _empleados = new();
    private readonly ParametrosEnMemoria _parametros = new();
    private readonly NominasEnMemoria _nominas = new();
    private readonly NominaService _servicio;

    public NominaServiceTests()
    {
        _empleados.Datos.AddRange(PayrollEngineTests.Plantilla());
        _parametros.Datos.Add(ParametrosNomina.Decreto2026());
        var motor = new PayrollEngine([new ReglaContratoLaboral(), new ReglaPrestacionServicios()]);
        _servicio = new NominaService(motor, _empleados, _parametros, _nominas, new RelojFijo());
    }

    [Fact]
    public async Task Calcula_y_guarda_en_borrador()
    {
        var n = await _servicio.CalcularAsync(2026, 10);

        n.Estado.Should().Be(EstadoNomina.Borrador);
        n.CalculadaEn.Should().Be(Ahora);
        n.Detalle.Should().HaveCount(3);
        _nominas.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Recalcular_un_borrador_lo_reemplaza()
    {
        await _servicio.CalcularAsync(2026, 10);
        _empleados.Datos.RemoveAll(e => e.Nombre == "Ana Torres");

        var n = await _servicio.CalcularAsync(2026, 10);

        (await _servicio.ListarAsync()).Should().ContainSingle();
        n.Detalle.Should().HaveCount(2);
    }

    [Fact]
    public async Task No_recalcula_un_mes_aprobado()
    {
        var n = await _servicio.CalcularAsync(2026, 10);
        await _servicio.AprobarAsync(n.Id);

        var accion = () => _servicio.CalcularAsync(2026, 10);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La nómina de 10/2026 ya está aprobada.");
        _nominas.Datos.Should().ContainSingle().Which.Estado.Should().Be(EstadoNomina.Aprobada);
    }

    [Fact]
    public async Task El_mensaje_de_aprobada_usa_dos_digitos_para_el_mes()
    {
        var n = await _servicio.CalcularAsync(2026, 3);
        await _servicio.AprobarAsync(n.Id);

        var accion = () => _servicio.CalcularAsync(2026, 3);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La nómina de 03/2026 ya está aprobada.");
    }

    [Fact]
    public async Task Aprueba_la_nomina()
    {
        var calculada = await _servicio.CalcularAsync(2026, 10);

        var n = await _servicio.AprobarAsync(calculada.Id);

        n.Estado.Should().Be(EstadoNomina.Aprobada);
        n.AprobadaEn.Should().Be(Ahora);
    }

    [Fact]
    public async Task No_aprueba_dos_veces()
    {
        var n = await _servicio.CalcularAsync(2026, 10);
        await _servicio.AprobarAsync(n.Id);

        var accion = () => _servicio.AprobarAsync(n.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La nómina ya está aprobada.");
    }

    [Fact]
    public async Task Aprobar_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.AprobarAsync(999);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Nómina no encontrada");
    }

    [Fact]
    public async Task Falla_si_no_hay_parametros_del_anio()
    {
        var accion = () => _servicio.CalcularAsync(2027, 1);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No hay parámetros de nómina para 2027.");
        _nominas.Datos.Should().BeEmpty();
    }

    [Fact]
    public async Task Un_error_del_motor_no_borra_el_borrador_anterior()
    {
        await _servicio.CalcularAsync(2026, 10);
        _empleados.Datos.ForEach(e => e.Activo = false);

        var accion = () => _servicio.CalcularAsync(2026, 10);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No hay empleados activos para liquidar.");
        _nominas.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Meses_distintos_conviven()
    {
        await _servicio.CalcularAsync(2026, 9);
        await _servicio.CalcularAsync(2026, 10);

        (await _servicio.ListarAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task Consulta_la_nomina_del_periodo()
    {
        await _servicio.CalcularAsync(2026, 10);

        var n = await _servicio.ConsultarAsync(2026, 10);

        n.Mes.Should().Be(10);
        await ((Func<Task>)(() => _servicio.ConsultarAsync(2026, 11)))
            .Should().ThrowAsync<NoEncontradoException>().WithMessage("Nómina no encontrada");
    }

    [Fact]
    public async Task Guardar_parametros_valida_tasas()
    {
        var p = ParametrosNomina.Decreto2026();
        p.SaludEmpleado = 1.5m;

        var accion = () => _servicio.GuardarParametrosAsync(p);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Las tasas deben estar entre 0 y 1.");
    }

    [Fact]
    public async Task Guardar_parametros_valida_tasas_negativas()
    {
        var p = ParametrosNomina.Decreto2026();
        p.Sena = -0.01m;

        var accion = () => _servicio.GuardarParametrosAsync(p);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Las tasas deben estar entre 0 y 1.");
    }

    [Fact]
    public async Task Guardar_parametros_valida_salario_minimo()
    {
        var p = ParametrosNomina.Decreto2026();
        p.SalarioMinimo = 0;

        var accion = () => _servicio.GuardarParametrosAsync(p);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El salario mínimo debe ser mayor que cero.");
    }

    [Fact]
    public async Task Guardar_parametros_valida_auxilio()
    {
        var p = ParametrosNomina.Decreto2026();
        p.AuxilioTransporte = -1;

        var accion = () => _servicio.GuardarParametrosAsync(p);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El auxilio de transporte no puede ser negativo.");
    }

    [Fact]
    public async Task Guardar_parametros_valida_el_anio()
    {
        var p = ParametrosNomina.Decreto2026();
        p.Anio = 2019;

        var accion = () => _servicio.GuardarParametrosAsync(p);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El año no es válido.");
    }

    [Fact]
    public async Task Guardar_y_consultar_parametros()
    {
        var p = ParametrosNomina.Decreto2026();
        p.AuxilioTransporte = 250_000;

        await _servicio.GuardarParametrosAsync(p);

        (await _servicio.ConsultarParametrosAsync(2026)).AuxilioTransporte.Should().Be(250_000);
        _parametros.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Consultar_parametros_inexistentes_lanza_no_encontrado()
    {
        var accion = () => _servicio.ConsultarParametrosAsync(2030);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("No hay parámetros de nómina para 2030.");
    }
}
