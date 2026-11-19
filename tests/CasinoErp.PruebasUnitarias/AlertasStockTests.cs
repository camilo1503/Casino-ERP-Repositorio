using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Inventario;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-05: umbral mínimo por denominación y alerta automática de stock crítico.</summary>
public class AlertasStockTests
{
    private static readonly DateTime Ahora = new(2026, 10, 26, 15, 0, 0, DateTimeKind.Utc);

    private readonly FichasEnMemoria _fichas = new();
    private readonly AlertasEnMemoria _alertas = new();
    private readonly ObservadorEspia _espia = new();
    private readonly InventarioFichasService _servicio;

    public AlertasStockTests() =>
        _servicio = new InventarioFichasService(_fichas, _alertas, [_espia], new RelojFijo());

    private void Existe(string codigo, int stock, int minimo) =>
        _fichas.Datos.Add(new Ficha { Codigo = codigo, Denominacion = 500_000, Categoria = "Negra", Stock = stock, StockMinimo = minimo });

    [Fact]
    public async Task Un_retiro_que_deja_el_stock_bajo_el_minimo_crea_alerta_y_avisa()
    {
        Existe("FICHA-500K", stock: 420, minimo: 400);

        var ficha = await _servicio.RetirarAsync("FICHA-500K", 25);

        ficha.Stock.Should().Be(395);
        ficha.EnNivelCritico.Should().BeTrue();
        var alerta = (await _servicio.ListarAlertasAsync()).Single();
        alerta.CodigoFicha.Should().Be("FICHA-500K");
        alerta.StockActual.Should().Be(395);
        alerta.StockMinimo.Should().Be(400);
        alerta.Estado.Should().Be(EstadoAlerta.Abierta);
        alerta.CreadaEn.Should().Be(Ahora);
        _espia.Avisos.Should().ContainSingle();
    }

    [Fact]
    public async Task Retiros_repetidos_bajo_el_minimo_no_duplican_la_alerta_ni_el_aviso()
    {
        Existe("FICHA-500K", stock: 420, minimo: 400);

        await _servicio.RetirarAsync("FICHA-500K", 25);
        await _servicio.RetirarAsync("FICHA-500K", 10);

        var alerta = (await _servicio.ListarAlertasAsync()).Single();
        alerta.StockActual.Should().Be(385);
        _espia.Avisos.Should().ContainSingle();
    }

    [Fact]
    public async Task Una_entrada_que_supera_el_minimo_atiende_la_alerta()
    {
        Existe("FICHA-500K", stock: 420, minimo: 400);
        await _servicio.RetirarAsync("FICHA-500K", 25);

        await _servicio.RegistrarEntradaAsync("FICHA-500K", 165);

        (await _servicio.ListarAlertasAsync()).Should().BeEmpty();
        var alerta = (await _servicio.ListarAlertasAsync(false)).Single();
        alerta.Estado.Should().Be(EstadoAlerta.Atendida);
        alerta.AtendidaEn.Should().Be(Ahora);
        alerta.StockActual.Should().Be(560);
    }

    [Fact]
    public async Task Una_entrada_que_no_alcanza_el_minimo_mantiene_la_alerta_abierta()
    {
        Existe("FICHA-500K", stock: 420, minimo: 400);
        await _servicio.RetirarAsync("FICHA-500K", 25);

        await _servicio.RegistrarEntradaAsync("FICHA-500K", 3);

        var alerta = (await _servicio.ListarAlertasAsync()).Single();
        alerta.Estado.Should().Be(EstadoAlerta.Abierta);
        alerta.StockActual.Should().Be(398);
    }

    [Fact]
    public async Task Quedar_exactamente_en_el_minimo_no_es_critico()
    {
        Existe("FICHA-500K", stock: 420, minimo: 400);

        var ficha = await _servicio.RetirarAsync("FICHA-500K", 20);

        ficha.Stock.Should().Be(400);
        ficha.EnNivelCritico.Should().BeFalse();
        (await _servicio.ListarAlertasAsync(false)).Should().BeEmpty();
        _espia.Avisos.Should().BeEmpty();
    }

    [Fact]
    public async Task Subir_el_minimo_por_encima_del_stock_dispara_la_alerta()
    {
        Existe("FICHA-5K", stock: 4_200, minimo: 1_000);

        var ficha = await _servicio.ActualizarStockMinimoAsync("FICHA-5K", 5_000);

        ficha.StockMinimo.Should().Be(5_000);
        var alerta = (await _servicio.ListarAlertasAsync()).Single();
        alerta.StockActual.Should().Be(4_200);
        alerta.StockMinimo.Should().Be(5_000);
        _espia.Avisos.Should().ContainSingle();
    }

    [Fact]
    public async Task Bajar_el_minimo_por_debajo_del_stock_atiende_la_alerta()
    {
        Existe("FICHA-5K", stock: 4_200, minimo: 1_000);
        await _servicio.ActualizarStockMinimoAsync("FICHA-5K", 5_000);

        await _servicio.ActualizarStockMinimoAsync("FICHA-5K", 4_000);

        (await _servicio.ListarAlertasAsync()).Should().BeEmpty();
        (await _servicio.ListarAlertasAsync(false)).Single().Estado.Should().Be(EstadoAlerta.Atendida);
    }

    [Fact]
    public async Task Rechaza_minimo_negativo()
    {
        Existe("FICHA-5K", stock: 4_200, minimo: 1_000);

        var accion = () => _servicio.ActualizarStockMinimoAsync("FICHA-5K", -1);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El stock mínimo no puede ser negativo");
    }

    [Fact]
    public async Task Actualizar_minimo_de_ficha_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.ActualizarStockMinimoAsync("FICHA-9K", 100);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Ficha no encontrada");
    }

    [Fact]
    public async Task Registrar_una_ficha_con_stock_inicial_bajo_el_minimo_crea_alerta()
    {
        var ficha = await _servicio.RegistrarAsync("FICHA-1M", 1_000_000, "Dorada", 10);

        ficha.StockMinimo.Should().Be(1_000);
        var alerta = (await _servicio.ListarAlertasAsync()).Single();
        alerta.CodigoFicha.Should().Be("FICHA-1M");
        alerta.StockActual.Should().Be(10);
        _espia.Avisos.Should().ContainSingle();
    }

    [Fact]
    public async Task Avisa_a_todos_los_observadores()
    {
        var otro = new ObservadorEspia();
        var servicio = new InventarioFichasService(_fichas, _alertas, [_espia, otro], new RelojFijo());
        Existe("FICHA-500K", stock: 420, minimo: 400);

        await servicio.RetirarAsync("FICHA-500K", 25);

        _espia.Avisos.Should().ContainSingle();
        otro.Avisos.Should().ContainSingle();
    }
}
