using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Mesas;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>pruebas/features/catalogo_mesas.feature</summary>
public class MesaServiceTests
{
    private readonly MesasEnMemoria _repo = new();
    private readonly MesaService _servicio;

    public MesaServiceTests() => _servicio = new MesaService(_repo);

    private void ExisteMesa(string numero, EstadoMesa estado) =>
        _repo.Datos.Add(new Mesa { Numero = numero, Juego = "Ruleta", ApuestaMinima = 10_000, ApuestaMaxima = 200_000, Estado = estado });

    [Fact]
    public async Task Registrar_mesa_valida_queda_cerrada()
    {
        var mesa = await _servicio.RegistrarAsync("M-19", "Blackjack", 25_000, 500_000);

        mesa.Estado.Should().Be(EstadoMesa.Cerrada);
        _repo.Datos.Should().ContainSingle(m => m.Numero == "M-19");
    }

    [Fact]
    public async Task Rechaza_numero_duplicado()
    {
        ExisteMesa("M-01", EstadoMesa.Abierta);

        var accion = () => _servicio.RegistrarAsync("M-01", "Ruleta", 10_000, 200_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El número de mesa ya está registrado");
    }

    [Theory]
    [InlineData(500_000, 100_000, "La apuesta mínima debe ser menor que la máxima")]
    [InlineData(100_000, 100_000, "La apuesta mínima debe ser menor que la máxima")]
    [InlineData(0, 100_000, "La apuesta mínima debe ser mayor que cero")]
    public async Task Rechaza_limites_invalidos(long minima, long maxima, string mensaje)
    {
        var accion = () => _servicio.RegistrarAsync("M-20", "Póker", minima, maxima);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage(mensaje);
    }

    [Theory]
    [InlineData("M19")]
    [InlineData("M-1")]
    [InlineData("X-10")]
    public async Task Rechaza_numero_con_formato_invalido(string numero)
    {
        var accion = () => _servicio.RegistrarAsync(numero, "Ruleta", 10_000, 20_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El número de mesa debe tener el formato M-00");
    }

    [Fact]
    public async Task Rechaza_juego_desconocido()
    {
        var accion = () => _servicio.RegistrarAsync("M-30", "Bingo", 10_000, 20_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El juego es obligatorio");
    }

    [Fact]
    public async Task Actualiza_los_limites()
    {
        ExisteMesa("M-05", EstadoMesa.Cerrada);

        var mesa = await _servicio.ActualizarLimitesAsync("M-05", 50_000, 1_000_000);

        mesa.ApuestaMinima.Should().Be(50_000);
        mesa.ApuestaMaxima.Should().Be(1_000_000);
    }

    [Fact]
    public async Task Retira_una_mesa_cerrada()
    {
        ExisteMesa("M-13", EstadoMesa.Cerrada);

        var mesa = await _servicio.RetirarAsync("M-13", "Paño dañado sin reparación");

        mesa.Estado.Should().Be(EstadoMesa.Retirada);
        mesa.MotivoRetiro.Should().Be("Paño dañado sin reparación");
    }

    [Fact]
    public async Task No_retira_una_mesa_abierta()
    {
        ExisteMesa("M-01", EstadoMesa.Abierta);

        var accion = () => _servicio.RetirarAsync("M-01", "Cambio de distribución");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No se puede retirar una mesa abierta");
        _repo.Datos[0].Estado.Should().Be(EstadoMesa.Abierta);
    }

    [Fact]
    public async Task No_retira_sin_motivo()
    {
        ExisteMesa("M-13", EstadoMesa.Cerrada);

        var accion = () => _servicio.RetirarAsync("M-13", " ");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El motivo es obligatorio");
    }

    [Fact]
    public async Task Abrir_y_cerrar_una_mesa()
    {
        ExisteMesa("M-05", EstadoMesa.Cerrada);

        (await _servicio.AbrirAsync("M-05")).Estado.Should().Be(EstadoMesa.Abierta);
        (await _servicio.CerrarAsync("M-05")).Estado.Should().Be(EstadoMesa.Cerrada);
    }

    [Fact]
    public async Task No_abre_una_mesa_ya_abierta()
    {
        ExisteMesa("M-01", EstadoMesa.Abierta);

        var accion = () => _servicio.AbrirAsync("M-01");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La mesa ya está abierta");
    }

    [Fact]
    public async Task Una_mesa_retirada_no_se_modifica()
    {
        ExisteMesa("M-13", EstadoMesa.Retirada);

        var accion = () => _servicio.AbrirAsync("M-13");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La mesa está retirada");
    }

    [Fact]
    public async Task Consultar_mesa_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.ConsultarAsync("M-99");

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task Registra_banco_de_tragamonedas_con_20_puestos()
    {
        var mesa = await _servicio.RegistrarAsync("tr-2", "Tragamonedas", 1_000, 50_000, "Zona B");

        mesa.Numero.Should().Be("TR-2");
        mesa.Asientos.Should().Be(20);
        mesa.Zona.Should().Be("Zona B");
    }

    [Fact]
    public async Task Rechaza_zona_desconocida()
    {
        var accion = () => _servicio.RegistrarAsync("M-30", "Ruleta", 10_000, 20_000, "Terraza");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La ubicación no es válida");
    }

    [Fact]
    public async Task Mantenimiento_y_reparacion()
    {
        ExisteMesa("M-05", EstadoMesa.Cerrada);

        (await _servicio.EnviarAMantenimientoAsync("M-05")).Estado.Should().Be(EstadoMesa.Mantenimiento);
        await ((Func<Task>)(() => _servicio.AbrirAsync("M-05")))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("La mesa está en mantenimiento");
        (await _servicio.MarcarReparadaAsync("M-05")).Estado.Should().Be(EstadoMesa.Cerrada);
    }

    [Fact]
    public async Task Mesa_abierta_no_va_a_mantenimiento()
    {
        ExisteMesa("M-01", EstadoMesa.Abierta);

        var accion = () => _servicio.EnviarAMantenimientoAsync("M-01");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Cierra la mesa antes de enviarla a mantenimiento");
    }

    [Fact]
    public async Task Solo_se_repara_una_mesa_en_mantenimiento()
    {
        ExisteMesa("M-05", EstadoMesa.Cerrada);

        var accion = () => _servicio.MarcarReparadaAsync("M-05");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La mesa no está en mantenimiento");
    }

    [Fact]
    public async Task Actualiza_la_zona_junto_con_los_limites()
    {
        ExisteMesa("M-05", EstadoMesa.Cerrada);

        var mesa = await _servicio.ActualizarLimitesAsync("M-05", 50_000, 1_000_000, "Salón VIP");

        mesa.Zona.Should().Be("Salón VIP");
    }
}
