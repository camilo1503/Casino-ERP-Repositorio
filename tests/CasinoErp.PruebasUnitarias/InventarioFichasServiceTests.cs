using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Inventario;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>pruebas/features/inventario_fichas.feature (Taller 4 y Ejercicio práctico 1)</summary>
public class InventarioFichasServiceTests
{
    private readonly FichasEnMemoria _repo = new();
    private readonly InventarioFichasService _servicio;

    public InventarioFichasServiceTests() => _servicio = new InventarioFichasService(_repo, new RelojFijo());

    private void Existe(string codigo, int stock) =>
        _repo.Datos.Add(new Ficha { Codigo = codigo, Denominacion = 5_000, Categoria = "Roja", Stock = stock });

    [Fact]
    public async Task Registra_una_denominacion()
    {
        var ficha = await _servicio.RegistrarAsync("FICHA-50K", 50_000, "Naranja", 2600);

        ficha.Stock.Should().Be(2600);
        ficha.ValorTotal.Should().Be(130_000_000);
    }

    [Fact]
    public async Task Rechaza_stock_inicial_negativo()
    {
        var accion = () => _servicio.RegistrarAsync("FICHA-1M", 1_000_000, "Morado", -5);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El stock inicial no puede ser negativo");
    }

    [Fact]
    public async Task Rechaza_ficha_sin_categoria()
    {
        var accion = () => _servicio.RegistrarAsync("FICHA-2K", 2_000, "", 100);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La categoría es obligatoria");
    }

    [Fact]
    public async Task Rechaza_codigo_duplicado()
    {
        Existe("FICHA-5K", 10);

        var accion = () => _servicio.RegistrarAsync("FICHA-5K", 5_000, "Roja", 1);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El código de ficha ya está registrado");
    }

    [Theory]
    [InlineData("5K")]
    [InlineData("FICHA-")]
    [InlineData("ficha-5k")]
    public async Task Rechaza_codigo_invalido(string codigo)
    {
        var accion = () => _servicio.RegistrarAsync(codigo, 5_000, "Roja", 1);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El código de ficha no es válido");
    }

    [Fact]
    public async Task Rechaza_denominacion_cero()
    {
        var accion = () => _servicio.RegistrarAsync("FICHA-0", 0, "Roja", 1);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La denominación debe ser mayor que cero");
    }

    [Theory]
    [InlineData("FICHA-5K", 10, 5, 15)]
    [InlineData("FICHA-10K", 0, 20, 20)]
    [InlineData("FICHA-25K", 50, 10, 60)]
    public async Task Entrada_suma_al_stock_y_queda_en_el_historial(string codigo, int stock, int cantidad, int final)
    {
        Existe(codigo, stock);

        var ficha = await _servicio.RegistrarEntradaAsync(codigo, cantidad);

        ficha.Stock.Should().Be(final);
        _repo.Movimientos.Should().ContainSingle(m => m.Tipo == TipoMovimiento.Entrada && m.StockFinal == final);
    }

    [Theory]
    [InlineData("FICHA-5K", 10, 3, 7)]
    [InlineData("FICHA-10K", 5, 5, 0)]
    public async Task Retiro_exitoso_descuenta_el_stock(string codigo, int stock, int retiro, int final)
    {
        Existe(codigo, stock);

        var ficha = await _servicio.RetirarAsync(codigo, retiro);

        ficha.Stock.Should().Be(final);
        _repo.Movimientos.Should().ContainSingle(m => m.Tipo == TipoMovimiento.Salida && m.Cantidad == retiro);
    }

    [Fact]
    public async Task Retiro_mayor_al_stock_lanza_stock_insuficiente_y_no_cambia_nada()
    {
        Existe("FICHA-25K", 2);

        var accion = () => _servicio.RetirarAsync("FICHA-25K", 5);

        await accion.Should().ThrowAsync<StockInsuficienteException>().WithMessage("Stock insuficiente");
        _repo.Datos[0].Stock.Should().Be(2);
        _repo.Movimientos.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Cantidad_debe_ser_positiva(int cantidad)
    {
        Existe("FICHA-5K", 10);

        await ((Func<Task>)(() => _servicio.RetirarAsync("FICHA-5K", cantidad)))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("La cantidad debe ser mayor que cero");
        await ((Func<Task>)(() => _servicio.RegistrarEntradaAsync("FICHA-5K", cantidad)))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("La cantidad debe ser mayor que cero");
    }

    [Fact]
    public async Task Ficha_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.RetirarAsync("FICHA-9K", 1);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Ficha no encontrada");
    }

    [Fact]
    public void Formatea_pesos_colombianos()
    {
        Pesos.Formatear(200_000).Should().Be("$200.000");
        Pesos.Formatear(5_000_000).Should().Be("$5.000.000");
    }
}
