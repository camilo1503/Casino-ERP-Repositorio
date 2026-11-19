using CasinoErp.Dominio.Nominas;
using CasinoErp.Dominio.Personal;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-04 · Reglas de liquidación por tipo de contrato con los parámetros 2026 (Plan-Felipe F2, D-003).</summary>
public class ReglasLiquidacionTests
{
    private static readonly ParametrosNomina P = ParametrosNomina.Decreto2026();

    private static Empleado E(Cargo c, Turno t, TipoContrato k, long b, string nombre = "Ana Torres") => new()
    {
        Documento = "1010200300",
        Nombre = nombre,
        Cargo = c,
        Turno = t,
        TipoContrato = k,
        SalarioBase = b,
    };

    private static LiquidacionEmpleado Laboral(long b, Turno t = Turno.Tarde, Cargo c = Cargo.Crupier) =>
        new ReglaContratoLaboral().Liquidar(E(c, t, TipoContrato.Indefinido, b), P);

    [Fact]
    public void Parametros_del_decreto_2026()
    {
        P.Anio.Should().Be(2026);
        P.SalarioMinimo.Should().Be(1_750_905);
        P.AuxilioTransporte.Should().Be(249_095);
        P.SaludEmpleado.Should().Be(0.04m);
        P.PensionEmpleado.Should().Be(0.04m);
        P.FondoSolidaridad.Should().Be(0.01m);
        P.RecargoNocturno.Should().Be(0.35m);
        P.RetencionServicios.Should().Be(0.10m);
        P.SaludEmpleador.Should().Be(0.085m);
        P.PensionEmpleador.Should().Be(0.12m);
        P.Arl.Should().Be(0.00522m);
        P.CajaCompensacion.Should().Be(0.04m);
        P.Icbf.Should().Be(0.03m);
        P.Sena.Should().Be(0.02m);
    }

    [Fact]
    public void Crupier_de_tarde_recibe_auxilio_de_transporte()
    {
        var l = Laboral(2_600_000);

        l.RecargoNocturno.Should().Be(0);
        l.AuxilioTransporte.Should().Be(249_095);
        l.DeduccionSalud.Should().Be(104_000);
        l.DeduccionPension.Should().Be(104_000);
        l.FondoSolidaridad.Should().Be(0);
        l.RetencionFuente.Should().Be(0);
        l.Neto.Should().Be(2_641_095);
        l.AportesEmpleador.Should().Be(780_572);
        l.TotalDeducciones.Should().Be(208_000);
    }

    [Fact]
    public void Crupier_de_noche_suma_recargo_del_35_y_pierde_el_auxilio()
    {
        // Coincide con el mockup: 2.600.000 + 910.000 - 280.800 = 3.229.200
        var l = Laboral(2_600_000, Turno.Noche);

        l.RecargoNocturno.Should().Be(910_000);
        l.AuxilioTransporte.Should().Be(0);            // IBC 3.510.000 > 2 SMMLV (3.501.810)
        l.DeduccionSalud.Should().Be(140_400);
        l.DeduccionPension.Should().Be(140_400);
        l.Neto.Should().Be(3_229_200);
        l.AportesEmpleador.Should().Be(1_053_772);
    }

    [Fact]
    public void Jefe_de_piso_sin_auxilio()
    {
        var l = Laboral(4_200_000, c: Cargo.JefeDePiso);

        l.AuxilioTransporte.Should().Be(0);
        l.DeduccionSalud.Should().Be(168_000);
        l.Neto.Should().Be(3_864_000);                 // igual que el mockup
    }

    [Fact]
    public void Cajero_de_tarde()
    {
        var l = Laboral(2_300_000, c: Cargo.Cajero);

        l.AuxilioTransporte.Should().Be(249_095);
        l.DeduccionSalud.Should().Be(92_000);
        l.Neto.Should().Be(2_365_095);
    }

    [Fact]
    public void El_turno_de_manana_no_tiene_recargo()
    {
        Laboral(2_600_000, Turno.Manana).RecargoNocturno.Should().Be(0);
    }

    [Fact]
    public void Auxilio_aplica_en_exactamente_dos_salarios_minimos()
    {
        Laboral(3_501_810).AuxilioTransporte.Should().Be(249_095);
    }

    [Fact]
    public void Auxilio_no_aplica_un_peso_por_encima()
    {
        Laboral(3_501_811).AuxilioTransporte.Should().Be(0);
    }

    [Fact]
    public void Fondo_de_solidaridad_en_exactamente_cuatro_salarios_minimos()
    {
        Laboral(7_003_620).FondoSolidaridad.Should().Be(70_036);
    }

    [Fact]
    public void Fondo_de_solidaridad_no_aplica_un_peso_por_debajo()
    {
        Laboral(7_003_619).FondoSolidaridad.Should().Be(0);
    }

    [Fact]
    public void Salario_alto_paga_solidaridad()
    {
        var l = Laboral(8_000_000);

        l.FondoSolidaridad.Should().Be(80_000);
        l.Neto.Should().Be(7_280_000);
        l.TotalDeducciones.Should().Be(720_000);
    }

    [Fact]
    public void Termino_fijo_usa_la_regla_laboral()
    {
        var regla = new ReglaContratoLaboral();

        regla.Aplica(TipoContrato.TerminoFijo).Should().BeTrue();
        regla.Aplica(TipoContrato.Indefinido).Should().BeTrue();
        regla.Aplica(TipoContrato.PrestacionServicios).Should().BeFalse();
    }

    [Fact]
    public void La_regla_de_servicios_solo_aplica_a_prestacion()
    {
        var regla = new ReglaPrestacionServicios();

        regla.Aplica(TipoContrato.PrestacionServicios).Should().BeTrue();
        regla.Aplica(TipoContrato.Indefinido).Should().BeFalse();
        regla.Aplica(TipoContrato.TerminoFijo).Should().BeFalse();
    }

    [Fact]
    public void Prestacion_de_servicios_solo_retiene_el_10()
    {
        var l = new ReglaPrestacionServicios().Liquidar(E(Cargo.Crupier, Turno.Noche, TipoContrato.PrestacionServicios, 3_000_000), P);

        l.RecargoNocturno.Should().Be(0);
        l.AuxilioTransporte.Should().Be(0);
        l.DeduccionSalud.Should().Be(0);
        l.DeduccionPension.Should().Be(0);
        l.FondoSolidaridad.Should().Be(0);
        l.RetencionFuente.Should().Be(300_000);
        l.AportesEmpleador.Should().Be(0);
        l.Neto.Should().Be(2_700_000);
        l.TotalDeducciones.Should().Be(300_000);
    }

    [Theory]
    [InlineData(TipoContrato.Indefinido)]
    [InlineData(TipoContrato.PrestacionServicios)]
    public void La_liquidacion_copia_los_datos_del_empleado(TipoContrato contrato)
    {
        var empleado = E(Cargo.Cajero, Turno.Noche, contrato, 2_300_000, "Laura Gómez");
        IReglaLiquidacion regla = contrato == TipoContrato.PrestacionServicios ? new ReglaPrestacionServicios() : new ReglaContratoLaboral();

        var l = regla.Liquidar(empleado, P);

        l.EmpleadoDocumento.Should().Be("1010200300");
        l.EmpleadoNombre.Should().Be("Laura Gómez");
        l.Cargo.Should().Be(Cargo.Cajero);
        l.Turno.Should().Be(Turno.Noche);
        l.TipoContrato.Should().Be(contrato);
        l.SalarioBase.Should().Be(2_300_000);
    }

    [Fact]
    public void Redondea_hacia_arriba_en_medio_peso()
    {
        // 1.750.910 * 0,35 = 612.818,5 -> 612.819 (con redondeo bancario daría 612.818)
        Laboral(1_750_910, Turno.Noche).RecargoNocturno.Should().Be(612_819);
    }

    [Fact]
    public void Los_totales_de_la_nomina_suman_el_detalle()
    {
        var nomina = new Nomina { Detalle = [Laboral(2_600_000), Laboral(2_600_000, Turno.Noche)] };

        nomina.Estado.Should().Be(EstadoNomina.Borrador);
        nomina.TotalNeto.Should().Be(2_641_095 + 3_229_200);
        nomina.TotalRecargos.Should().Be(910_000);
        nomina.TotalDeducciones.Should().Be(208_000 + 280_800);
        nomina.TotalAportesEmpleador.Should().Be(780_572 + 1_053_772);
    }
}
