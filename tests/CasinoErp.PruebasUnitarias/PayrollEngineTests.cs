using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Nominas;
using CasinoErp.Dominio.Personal;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-04 · Motor de nómina: cálculo puro, sin base de datos ni reloj (Plan-Felipe F3).</summary>
public class PayrollEngineTests
{
    private static readonly ParametrosNomina P = ParametrosNomina.Decreto2026();
    private readonly PayrollEngine _motor = new([new ReglaContratoLaboral(), new ReglaPrestacionServicios()]);

    internal static Empleado E(string nombre, Turno turno, long salario, Cargo cargo = Cargo.Crupier,
        TipoContrato contrato = TipoContrato.Indefinido, bool activo = true) => new()
    {
        Documento = (1_000_000_000 + nombre.Select((c, i) => c * (i + 1)).Sum()).ToString(),
        Nombre = nombre,
        Cargo = cargo,
        Turno = turno,
        SalarioBase = salario,
        TipoContrato = contrato,
        Activo = activo,
    };

    /// <summary>Ana Torres (crupier tarde 2,6 M), Bruno Lara (crupier noche 2,6 M), Daniela Vargas (jefe de piso 4,2 M) y Pedro Ruiz (retirado).</summary>
    internal static List<Empleado> Plantilla() =>
    [
        E("Daniela Vargas", Turno.Tarde, 4_200_000, Cargo.JefeDePiso),
        E("Bruno Lara", Turno.Noche, 2_600_000),
        E("Pedro Ruiz", Turno.Tarde, 2_600_000, activo: false),
        E("Ana Torres", Turno.Tarde, 2_600_000),
    ];

    [Fact]
    public void Liquida_solo_empleados_activos_y_suma_totales()
    {
        var n = _motor.Calcular(2026, 10, Plantilla(), P);

        n.Detalle.Should().HaveCount(3);
        n.Detalle.Should().NotContain(d => d.EmpleadoNombre == "Pedro Ruiz");
        n.Detalle.Select(d => d.EmpleadoNombre).Should().BeInAscendingOrder();
        n.TotalNeto.Should().Be(2_641_095 + 3_229_200 + 3_864_000);
        n.TotalRecargos.Should().Be(910_000);
        n.Estado.Should().Be(EstadoNomina.Borrador);
        n.Anio.Should().Be(2026);
        n.Mes.Should().Be(10);
    }

    [Fact]
    public void No_asigna_fechas_ni_id()
    {
        var n = _motor.Calcular(2026, 10, Plantilla(), P);

        n.Id.Should().Be(0);
        n.CalculadaEn.Should().Be(default);
        n.AprobadaEn.Should().BeNull();
    }

    [Fact]
    public void Usa_la_regla_de_cada_tipo_de_contrato()
    {
        var empleados = Plantilla();
        empleados.Add(E("Carlos Vega", Turno.Noche, 3_000_000, contrato: TipoContrato.PrestacionServicios));

        var n = _motor.Calcular(2026, 10, empleados, P);

        n.Detalle.Single(d => d.EmpleadoNombre == "Carlos Vega").RetencionFuente.Should().Be(300_000);
        n.Detalle.Where(d => d.TipoContrato != TipoContrato.PrestacionServicios)
            .Should().OnlyContain(d => d.RetencionFuente == 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Rechaza_mes_invalido(int mes)
    {
        var accion = () => _motor.Calcular(2026, mes, Plantilla(), P);

        accion.Should().Throw<ReglaNegocioException>().WithMessage("El mes debe estar entre 1 y 12.");
    }

    [Fact]
    public void Rechaza_cuando_no_hay_activos()
    {
        var accion = () => _motor.Calcular(2026, 10, [E("Pedro Ruiz", Turno.Tarde, 2_600_000, activo: false)], P);

        accion.Should().Throw<ReglaNegocioException>().WithMessage("No hay empleados activos para liquidar.");
    }

    [Fact]
    public void Rechaza_cuando_la_lista_esta_vacia()
    {
        var accion = () => _motor.Calcular(2026, 10, [], P);

        accion.Should().Throw<ReglaNegocioException>().WithMessage("No hay empleados activos para liquidar.");
    }

    [Theory]
    [InlineData(TipoContrato.Indefinido)]
    [InlineData(TipoContrato.TerminoFijo)]
    public void Rechaza_salario_laboral_bajo_el_minimo(TipoContrato contrato)
    {
        var accion = () => _motor.Calcular(2026, 10, [E("Ana Torres", Turno.Tarde, 1_500_000, contrato: contrato)], P);

        accion.Should().Throw<ReglaNegocioException>().WithMessage("El salario de Ana Torres es menor al salario mínimo de 2026.");
    }

    [Fact]
    public void Acepta_salario_laboral_igual_al_minimo()
    {
        var n = _motor.Calcular(2026, 10, [E("Ana Torres", Turno.Tarde, 1_750_905)], P);

        n.Detalle.Should().ContainSingle();
    }

    [Fact]
    public void Un_retirado_bajo_el_minimo_no_bloquea_la_nomina()
    {
        var empleados = Plantilla();
        empleados.Add(E("Pedro Gil", Turno.Tarde, 1_000_000, activo: false));

        var n = _motor.Calcular(2026, 10, empleados, P);

        n.Detalle.Should().HaveCount(3);
    }

    [Fact]
    public void Prestacion_de_servicios_bajo_el_minimo_si_se_liquida()
    {
        var n = _motor.Calcular(2026, 10, [E("Ana Torres", Turno.Tarde, 1_000_000, contrato: TipoContrato.PrestacionServicios)], P);

        n.Detalle.Single().Neto.Should().Be(900_000);
    }

    [Fact]
    public void Sin_regla_para_el_contrato_es_error_de_configuracion()
    {
        var motor = new PayrollEngine([new ReglaContratoLaboral()]);

        var accion = () => motor.Calcular(2026, 10, [E("Carlos Vega", Turno.Tarde, 3_000_000, contrato: TipoContrato.PrestacionServicios)], P);

        accion.Should().Throw<InvalidOperationException>().WithMessage("No hay regla de liquidación para PrestacionServicios");
    }

    [Fact]
    public void Un_contrato_nuevo_es_solo_una_regla_mas()
    {
        // OCP: el motor no cambia; se inyecta otra implementación de IReglaLiquidacion.
        var motor = new PayrollEngine([new ReglaFija(123_456)]);

        var n = motor.Calcular(2026, 10, [E("Ana Torres", Turno.Tarde, 2_600_000)], P);

        n.Detalle.Single().Neto.Should().Be(123_456);
    }

    private sealed class ReglaFija(long neto) : IReglaLiquidacion
    {
        public bool Aplica(TipoContrato tipo) => true;
        public LiquidacionEmpleado Liquidar(Empleado empleado, ParametrosNomina p) =>
            new() { EmpleadoDocumento = empleado.Documento, EmpleadoNombre = empleado.Nombre, Neto = neto };
    }
}
