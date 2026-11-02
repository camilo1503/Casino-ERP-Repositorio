using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>pruebas/features/compra_fichas.feature y módulo Caja / POS del front.</summary>
public class CajaServiceTests
{
    private readonly JugadoresEnMemoria _jugadores = new();
    private readonly TransaccionesEnMemoria _transacciones = new();
    private readonly CajaService _servicio;

    public CajaServiceTests()
    {
        var reloj = new RelojFijo();
        _servicio = new CajaService(_transacciones, new JugadorService(_jugadores, reloj), reloj);
        _jugadores.Datos.Add(new Jugador { Documento = "1020456789", Nombre = "María", Estado = EstadoJugador.Activo });
        _jugadores.Datos.Add(new Jugador { Documento = "80554219", Nombre = "Diego", Estado = EstadoJugador.Autoexcluido });
        _jugadores.Datos.Add(new Jugador { Documento = "1026441093", Nombre = "Valentina", Estado = EstadoJugador.Inactivo });
    }

    private Task<TransaccionCaja> Compra(string? documento, long monto, long descuento = 0) =>
        _servicio.RegistrarAsync(TipoTransaccion.Compra, documento, monto, descuento);

    [Fact]
    public async Task Compra_bajo_el_umbral_queda_aprobada()
    {
        var compra = await Compra("1020456789", 100_000);

        compra.Estado.Should().Be(EstadoTransaccion.Aprobada);
        compra.EstadoCumplimiento.Should().BeNull();
        CajaService.Mensaje(compra).Should().Be("Compra registrada");
    }

    [Theory]
    [InlineData(5_000_000)]
    [InlineData(8_750_000)]
    public async Task Compra_desde_el_umbral_va_a_revision(long monto)
    {
        var compra = await Compra("1020456789", monto);

        compra.Estado.Should().Be(EstadoTransaccion.EnRevision);
        compra.EstadoCumplimiento.Should().Be("En revisión");
        CajaService.Mensaje(compra).Should().Be("Compra enviada a revisión de cumplimiento");
    }

    [Fact]
    public async Task Pago_sobre_el_umbral_tambien_va_a_revision()
    {
        var pago = await _servicio.RegistrarAsync(TipoTransaccion.Pago, "1020456789", 8_750_000);

        pago.Estado.Should().Be(EstadoTransaccion.EnRevision);
        CajaService.Mensaje(pago).Should().Be("Pago enviado a revisión de cumplimiento");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public async Task Rechaza_monto_cero_o_negativo_sin_registrar(long monto)
    {
        var accion = () => Compra("1020456789", monto);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El monto debe ser mayor que cero");
        _transacciones.Datos.Should().BeEmpty();
    }

    [Fact]
    public async Task No_vende_fichas_a_un_autoexcluido()
    {
        var accion = () => Compra("80554219", 100_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No se venden fichas a un jugador autoexcluido");
        _transacciones.Datos.Should().BeEmpty();
    }

    [Fact]
    public async Task No_vende_fichas_a_un_jugador_inactivo()
    {
        var accion = () => Compra("1026441093", 100_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El jugador no está activo");
    }

    [Fact]
    public async Task Jugador_no_registrado_puede_comprar()
    {
        var compra = await Compra(null, 300_000);

        compra.Documento.Should().BeNull();
        compra.Estado.Should().Be(EstadoTransaccion.Aprobada);
    }

    [Fact]
    public async Task Documento_inexistente_lanza_no_encontrado()
    {
        var accion = () => Compra("999999999", 100_000);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task Compra_suma_un_punto_por_cada_10000()
    {
        await Compra("1020456789", 2_500_000);

        _jugadores.Datos[0].Puntos.Should().Be(250);
    }

    [Fact]
    public async Task Pago_no_suma_puntos()
    {
        await _servicio.RegistrarAsync(TipoTransaccion.Pago, "1020456789", 1_200_000);

        _jugadores.Datos[0].Puntos.Should().Be(0);
    }

    [Fact]
    public async Task Descuento_de_retencion_se_guarda_con_la_compra()
    {
        var compra = await Compra("1020456789", 1_000_000, descuento: 200_000);

        compra.Descuento.Should().Be(200_000);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1_000_000)]
    public async Task Descuento_invalido(long descuento)
    {
        var accion = () => Compra("1020456789", 1_000_000, descuento);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El descuento no es válido");
    }

    [Fact]
    public async Task Aprueba_una_transaccion_en_revision()
    {
        var pago = await _servicio.RegistrarAsync(TipoTransaccion.Pago, "1020456789", 8_750_000);

        var aprobada = await _servicio.AprobarRevisionAsync(pago.Id);

        aprobada.Estado.Should().Be(EstadoTransaccion.Aprobada);
        await ((Func<Task>)(() => _servicio.AprobarRevisionAsync(pago.Id)))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("La transacción no está en revisión");
    }

    [Fact]
    public async Task Anula_solo_transacciones_aprobadas()
    {
        var compra = await Compra("1020456789", 300_000);

        (await _servicio.AnularAsync(compra.Id)).Estado.Should().Be(EstadoTransaccion.Anulada);
        await ((Func<Task>)(() => _servicio.AnularAsync(compra.Id)))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("Solo se anulan transacciones aprobadas");
    }

    [Fact]
    public async Task Cumplimiento_reporta_una_transaccion_sobre_el_umbral()
    {
        var compra = await Compra("1020456789", 6_000_000);

        var reportada = await _servicio.CambiarEstadoCumplimientoAsync(compra.Id, "Reportada");

        reportada.EstadoCumplimiento.Should().Be("Reportada");
    }

    [Fact]
    public async Task Cumplimiento_no_aplica_bajo_el_umbral()
    {
        var compra = await Compra("1020456789", 100_000);

        var accion = () => _servicio.CambiarEstadoCumplimientoAsync(compra.Id, "Escalada");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La transacción no supera el umbral de cumplimiento");
    }

    [Theory]
    [InlineData(50_000, 4, 200_000)]
    [InlineData(5_000, 1, 5_000)]
    public void Calcula_el_total_por_denominacion(long denominacion, int cantidad, long total)
    {
        CajaService.CalcularTotal(denominacion, cantidad).Should().Be(total);
    }

    [Fact]
    public void Rechaza_denominacion_que_no_existe()
    {
        var accion = () => CajaService.CalcularTotal(7_000, 2);

        accion.Should().Throw<ReglaNegocioException>().WithMessage("Denominación no válida");
    }
}
