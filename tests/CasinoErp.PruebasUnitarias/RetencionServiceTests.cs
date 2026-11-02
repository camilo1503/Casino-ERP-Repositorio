using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Retencion;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;
using Moq;

namespace CasinoErp.PruebasUnitarias;

/// <summary>pruebas/features/retencion.feature</summary>
public class RetencionServiceTests
{
    private readonly JugadoresEnMemoria _jugadores = new();
    private readonly OfertasEnMemoria _ofertas = new();
    private readonly RelojFijo _reloj = new();
    private readonly RetencionService _servicio;

    public RetencionServiceTests()
    {
        _servicio = new RetencionService(_ofertas, new JugadorService(_jugadores, _reloj), _reloj);
        _jugadores.Datos.Add(new Jugador { Documento = "1020456789", Puntos = 6200, Estado = EstadoJugador.Activo });
        _jugadores.Datos.Add(new Jugador { Documento = "80554219", Puntos = 2300, Estado = EstadoJugador.Autoexcluido });
    }

    [Theory]
    [InlineData(500_000, 100_000)]
    [InlineData(2_500_000, 200_000)]
    [InlineData(1_000_000, 200_000)]
    public async Task Descuento_es_el_20_por_ciento_con_tope(long monto, long descuento)
    {
        (await _servicio.CalcularDescuentoAsync(monto)).Should().Be(descuento);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public async Task Descuento_con_monto_invalido(long monto)
    {
        var accion = () => _servicio.CalcularDescuentoAsync(monto);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Monto inválido");
    }

    [Fact]
    public async Task Jugador_oro_recibe_fichas_bono_por_un_dia()
    {
        var oferta = await _servicio.EvaluarSenalAsync("1020456789", 95, "Quiere retirarse");

        oferta.Resultado.Should().Be(ResultadoOferta.Aceptada);
        oferta.Tipo.Should().Be(TipoOferta.FichasBono);
        oferta.Valor.Should().Be(50_000);
        oferta.Fecha.Should().Be(new DateOnly(2026, 10, 26));
        oferta.Vence.Should().Be(new DateOnly(2026, 10, 27));
    }

    [Fact]
    public async Task Bono_reclamable_vence_a_los_30_dias()
    {
        var oferta = await _servicio.RegistrarRespuestaAsync("1020456789", 95, "Cambio a fichas grandes", "Mesa M-04", true, TipoOferta.BonoReclamable);

        oferta.Valor.Should().Be(100_000);
        oferta.Vence.Should().Be(new DateOnly(2026, 11, 25));
        oferta.Origen.Should().Be("Mesa M-04");
    }

    [Fact]
    public async Task Rechazo_queda_registrado_sin_oferta_y_cuenta_para_el_limite()
    {
        var rechazo = await _servicio.RegistrarRespuestaAsync("1020456789", 45, "Solicitud de cobro en caja", "Caja 2", false);

        rechazo.Resultado.Should().Be(ResultadoOferta.Rechazada);
        rechazo.Tipo.Should().BeNull();
        rechazo.Valor.Should().Be(0);
        await ((Func<Task>)(() => _servicio.EvaluarSenalAsync("1020456789", 95, "Quiere retirarse")))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("El jugador ya recibió una oferta hoy");
    }

    [Theory]
    [InlineData(NivelLealtad.Bronce, TipoOferta.DescuentoFichas)]
    [InlineData(NivelLealtad.Plata, TipoOferta.DescuentoFichas)]
    [InlineData(NivelLealtad.Oro, TipoOferta.FichasBono)]
    [InlineData(NivelLealtad.Diamante, TipoOferta.BonoReclamable)]
    public void Oferta_sugerida_segun_nivel(NivelLealtad nivel, TipoOferta tipo)
    {
        RetencionService.OfertaSugerida(nivel).Should().Be(tipo);
    }

    [Fact]
    public async Task Bloquea_oferta_a_jugador_autoexcluido_sin_guardar()
    {
        var repo = new Mock<IOfertaRepositorio>();
        repo.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(new ConfiguracionRetencion());
        var servicio = new RetencionService(repo.Object, new JugadorService(_jugadores, _reloj), _reloj);

        var accion = () => servicio.EvaluarSenalAsync("80554219", 60, "Quiere retirarse");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No se ofrece retención: juego responsable");
        repo.Verify(r => r.AgregarAsync(It.IsAny<OfertaRetencion>()), Times.Never);
    }

    [Fact]
    public async Task Solo_una_oferta_por_jugador_al_dia()
    {
        await _servicio.EvaluarSenalAsync("1020456789", 95, "Quiere retirarse");

        var accion = () => _servicio.EvaluarSenalAsync("1020456789", 120, "Pide la cuenta");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El jugador ya recibió una oferta hoy");
        _ofertas.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task No_ofrece_con_menos_de_30_minutos()
    {
        var accion = () => _servicio.EvaluarSenalAsync("1020456789", 22, "Quiere retirarse");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La sesión debe durar al menos 30 minutos");
        _ofertas.Datos.Should().BeEmpty();
    }

    [Fact]
    public async Task Usa_la_configuracion_guardada()
    {
        _ofertas.Configuracion = new ConfiguracionRetencion { MinutosMinimos = 10, FichasBono = 80_000 };

        var oferta = await _servicio.EvaluarSenalAsync("1020456789", 15, "Quiere retirarse");

        oferta.Valor.Should().Be(80_000);
    }

    [Fact]
    public async Task Actualiza_la_configuracion()
    {
        var nueva = new ConfiguracionRetencion { PorcentajeDescuento = 25, OfertasPorDia = 2, SenalCobroCaja = false };

        var config = await _servicio.ActualizarConfiguracionAsync(nueva);

        config.PorcentajeDescuento.Should().Be(25);
        config.OfertasPorDia.Should().Be(2);
        config.SenalCobroCaja.Should().BeFalse();
        (await _servicio.CalcularDescuentoAsync(400_000)).Should().Be(100_000);
    }

    [Theory]
    [InlineData(0, 1, "El descuento debe estar entre 1% y 50%")]
    [InlineData(60, 1, "El descuento debe estar entre 1% y 50%")]
    [InlineData(20, 0, "Debe permitirse al menos una oferta por día")]
    public async Task Rechaza_configuracion_invalida(int porcentaje, int ofertasPorDia, string mensaje)
    {
        var accion = () => _servicio.ActualizarConfiguracionAsync(new ConfiguracionRetencion { PorcentajeDescuento = porcentaje, OfertasPorDia = ofertasPorDia });

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage(mensaje);
        _ofertas.Configuracion.PorcentajeDescuento.Should().Be(20);
    }
}
