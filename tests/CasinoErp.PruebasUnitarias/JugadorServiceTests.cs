using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;
using Moq;

namespace CasinoErp.PruebasUnitarias;

/// <summary>pruebas/features/perfiles_jugador.feature</summary>
public class JugadorServiceTests
{
    private readonly JugadoresEnMemoria _repo = new();
    private readonly JugadorService _servicio;

    public JugadorServiceTests() => _servicio = new JugadorService(_repo, new RelojFijo());

    private Task<Jugador> RegistrarMaria() =>
        _servicio.RegistrarAsync("1020456789", "María Fernanda López", "maria.lopez@correo.com", new DateOnly(1990, 5, 14));

    [Fact]
    public async Task Registrar_jugador_valido_lo_crea_activo()
    {
        var jugador = await RegistrarMaria();

        jugador.Estado.Should().Be(EstadoJugador.Activo);
        jugador.Puntos.Should().Be(0);
        _repo.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Registrar_normaliza_nombre_y_correo()
    {
        var jugador = await _servicio.RegistrarAsync("1020456789", "  María López ", " MARIA@Correo.com ", new DateOnly(1990, 5, 14));

        jugador.Nombre.Should().Be("María López");
        jugador.Correo.Should().Be("maria@correo.com");
    }

    [Fact]
    public async Task Rechaza_documento_duplicado_y_conserva_el_original()
    {
        await RegistrarMaria();

        var accion = () => _servicio.RegistrarAsync("1020456789", "Otra Persona", "otra@correo.com", new DateOnly(1988, 1, 10));

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El documento ya está registrado");
        _repo.Datos.Should().ContainSingle().Which.Nombre.Should().Be("María Fernanda López");
    }

    [Fact]
    public async Task Rechaza_jugador_menor_de_edad()
    {
        var accion = () => _servicio.RegistrarAsync("1099000111", "Pedro Gil", "pedro.gil@correo.com", new DateOnly(2010, 3, 1));

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El jugador debe ser mayor de 18 años");
    }

    [Fact]
    public async Task Acepta_jugador_que_cumple_18_hoy()
    {
        var jugador = await _servicio.RegistrarAsync("1099000111", "Pedro Gil", "pedro@correo.com", new DateOnly(2008, 10, 26));

        jugador.Estado.Should().Be(EstadoJugador.Activo);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("ABC123456")]
    [InlineData("")]
    public async Task Rechaza_documento_invalido(string documento)
    {
        var accion = () => _servicio.RegistrarAsync(documento, "Ana", "ana@correo.com", new DateOnly(1990, 1, 1));

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El documento no es válido");
    }

    [Fact]
    public async Task Rechaza_nombre_vacio()
    {
        var accion = () => _servicio.RegistrarAsync("1020456789", "  ", "ana@correo.com", new DateOnly(1990, 1, 1));

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El nombre es obligatorio");
    }

    [Theory]
    [InlineData("maria.lopez@correo.com", true)]
    [InlineData("sin-arroba.com", false)]
    [InlineData("usuario@", false)]
    [InlineData("usuario@dominio", false)]
    public void Valida_formato_de_correo(string correo, bool esValido)
    {
        var accion = () => JugadorService.ValidarCorreo(correo);

        if (esValido) accion.Should().NotThrow();
        else accion.Should().Throw<ReglaNegocioException>().WithMessage("Formato de correo inválido");
    }

    [Theory]
    [InlineData("1990-05-14", "2026-10-26", 36)]
    [InlineData("2008-10-27", "2026-10-26", 17)]
    [InlineData("2008-10-26", "2026-10-26", 18)]
    public void Calcula_la_edad_cumplida(string nacimiento, string hoy, int esperada)
    {
        JugadorService.CalcularEdad(DateOnly.Parse(nacimiento), DateOnly.Parse(hoy)).Should().Be(esperada);
    }

    [Fact]
    public async Task Actualiza_nombre_y_correo()
    {
        await RegistrarMaria();

        var jugador = await _servicio.ActualizarAsync("1020456789", "María F. López", "nuevo@correo.com");

        jugador.Nombre.Should().Be("María F. López");
        jugador.Correo.Should().Be("nuevo@correo.com");
    }

    [Fact]
    public async Task Consultar_jugador_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.ConsultarAsync("999999999");

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Jugador no encontrado");
    }

    [Fact]
    public async Task Desactivar_con_motivo_cambia_a_inactivo()
    {
        await RegistrarMaria();

        var jugador = await _servicio.DesactivarAsync("1020456789", "Solicitud del titular");

        jugador.Estado.Should().Be(EstadoJugador.Inactivo);
        jugador.MotivoEstado.Should().Be("Solicitud del titular");
    }

    [Fact]
    public async Task No_desactiva_sin_motivo()
    {
        await RegistrarMaria();

        var accion = () => _servicio.DesactivarAsync("1020456789", "");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El motivo es obligatorio");
        _repo.Datos[0].Estado.Should().Be(EstadoJugador.Activo);
    }

    [Fact]
    public async Task Autoexcluido_no_se_puede_reactivar()
    {
        await RegistrarMaria();
        await _servicio.AutoexcluirAsync("1020456789", "Juego responsable");

        var accion = () => _servicio.ReactivarAsync("1020456789");

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task Inactivo_se_puede_reactivar()
    {
        await RegistrarMaria();
        await _servicio.DesactivarAsync("1020456789", "Viaje");

        var jugador = await _servicio.ReactivarAsync("1020456789");

        jugador.Estado.Should().Be(EstadoJugador.Activo);
        jugador.MotivoEstado.Should().BeNull();
    }

    [Theory]
    [InlineData(0, NivelLealtad.Bronce)]
    [InlineData(2000, NivelLealtad.Plata)]
    [InlineData(5000, NivelLealtad.Oro)]
    [InlineData(10000, NivelLealtad.Diamante)]
    public void Nivel_depende_de_los_puntos(int puntos, NivelLealtad nivel)
    {
        new Jugador { Puntos = puntos }.Nivel.Should().Be(nivel);
    }

    [Fact]
    public async Task No_guarda_nada_si_el_correo_es_invalido()
    {
        var repo = new Mock<IJugadorRepositorio>();
        var servicio = new JugadorService(repo.Object, new RelojFijo());

        var accion = () => servicio.RegistrarAsync("1020456789", "Ana", "sin-arroba.com", new DateOnly(1990, 1, 1));

        await accion.Should().ThrowAsync<ReglaNegocioException>();
        repo.Verify(r => r.AgregarAsync(It.IsAny<Jugador>()), Times.Never);
    }

    [Fact]
    public async Task Rechaza_correo_ya_registrado()
    {
        await RegistrarMaria();

        var accion = () => _servicio.RegistrarAsync("1099000111", "Otra María", "MARIA.LOPEZ@correo.com", new DateOnly(1991, 1, 1));

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Ese correo ya está registrado");
    }

    [Fact]
    public async Task Registra_jugador_con_pasaporte()
    {
        var jugador = await _servicio.RegistrarAsync("ab123456", "John Smith", "john@mail.com", new DateOnly(1980, 1, 1), "Pasaporte");

        jugador.TipoDocumento.Should().Be("Pasaporte");
        jugador.Documento.Should().Be("AB123456");
    }

    [Fact]
    public async Task Cedula_con_letras_no_es_valida()
    {
        var accion = () => _servicio.RegistrarAsync("AB123456", "John Smith", "john@mail.com", new DateOnly(1980, 1, 1), "CC");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El documento no es válido");
    }

    [Fact]
    public async Task Rechaza_tipo_de_documento_desconocido()
    {
        var accion = () => _servicio.RegistrarAsync("1020456789", "Ana", "ana@correo.com", new DateOnly(1990, 1, 1), "NIT");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El tipo de documento no es válido");
    }

    [Fact]
    public async Task Actualiza_contacto_y_preferencias()
    {
        await RegistrarMaria();

        var jugador = await _servicio.ActualizarPerfilAsync("1020456789", " +57 310 555 0142 ", "Chía", "Tarde", 120_000, ["Ruleta", "Blackjack", "Ruleta"]);

        jugador.Telefono.Should().Be("+57 310 555 0142");
        jugador.Ciudad.Should().Be("Chía");
        jugador.Franja.Should().Be("Tarde");
        jugador.ApuestaPromedio.Should().Be(120_000);
        jugador.Preferencias.Should().Equal("Ruleta", "Blackjack");
    }

    [Theory]
    [InlineData("+57 300", "Noche", 50_000, new string[0], "Elige al menos un juego preferido")]
    [InlineData("+57 300", "Noche", 50_000, new[] { "Bingo" }, "Juego preferido no válido")]
    [InlineData(" ", "Noche", 50_000, new[] { "Ruleta" }, "El teléfono es obligatorio")]
    [InlineData("+57 300", "Madrugada", 50_000, new[] { "Ruleta" }, "El horario preferido no es válido")]
    [InlineData("+57 300", "Noche", 500, new[] { "Ruleta" }, "La apuesta promedio debe ser de al menos $1.000")]
    public async Task Rechaza_perfil_invalido(string telefono, string franja, long apuesta, string[] preferencias, string mensaje)
    {
        await RegistrarMaria();

        var accion = () => _servicio.ActualizarPerfilAsync("1020456789", telefono, "Bogotá", franja, apuesta, preferencias);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage(mensaje);
    }
}
