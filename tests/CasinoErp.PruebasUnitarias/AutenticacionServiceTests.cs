using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Seguridad;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-06 · Login con contraseña, TOTP y bloqueo por intentos (Plan-Ivan I2, loginView del mockup).</summary>
public class AutenticacionServiceTests
{
    private const string Correo = "laura.gomez@pitboss.co";
    private const string Contrasena = "Casino2026";
    private const string Codigo = "482913";
    private const string MensajeGenerico = "Correo o contraseña incorrectos.";
    private const string MensajeBloqueada = "Cuenta bloqueada 15 minutos por 5 intentos fallidos. Un administrador puede restablecerla.";

    private static readonly DateTime Ahora = new(2026, 10, 26, 15, 0, 0, DateTimeKind.Utc);

    private readonly UsuariosEnMemoria _usuarios = new();
    private readonly RelojMovible _reloj = new();
    private readonly AutenticacionService _auth;
    private readonly Usuario _laura;

    public AutenticacionServiceTests()
    {
        _laura = new Usuario
        {
            Nombre = "Laura Gómez", Correo = Correo, Rol = Rol.Cajero,
            HashContrasena = "H:" + Contrasena, SecretoTotp = "SECRETO-LAURA",
        };
        _usuarios.AgregarAsync(_laura).Wait();
        _auth = new AutenticacionService(_usuarios, new HasherFalso(), new TotpFalso(), _reloj);
    }

    private Task<Usuario> Ingresar(string correo = Correo, string contrasena = Contrasena, string codigo = Codigo) =>
        _auth.IniciarSesionAsync(correo, contrasena, codigo);

    private async Task<string> Fallar(string contrasena = Contrasena, string codigo = Codigo)
    {
        try { await Ingresar(contrasena: contrasena, codigo: codigo); }
        catch (AutenticacionException e) { return e.Message; }
        throw new InvalidOperationException("Se esperaba un fallo de autenticación");
    }

    [Fact]
    public async Task Ingresa_con_contrasena_y_codigo_correctos()
    {
        var u = await Ingresar();

        u.Should().BeSameAs(_laura);
        u.UltimoIngreso.Should().Be(Ahora);
        u.IntentosFallidos.Should().Be(0);
    }

    [Fact]
    public async Task El_correo_no_distingue_mayusculas()
    {
        var u = await Ingresar(correo: "LAURA.GOMEZ@PITBOSS.CO ");

        u.Should().BeSameAs(_laura);
    }

    [Fact]
    public async Task Correo_inexistente_da_mensaje_generico()
    {
        var accion = () => Ingresar(correo: "nadie@pitboss.co");

        await accion.Should().ThrowAsync<AutenticacionException>().WithMessage(MensajeGenerico);
    }

    [Fact]
    public async Task Contrasena_errada_suma_un_intento()
    {
        (await Fallar(contrasena: "Otra2026!")).Should().Be(MensajeGenerico);

        _laura.IntentosFallidos.Should().Be(1);
    }

    [Fact]
    public async Task Codigo_errado_informa_el_intento()
    {
        (await Fallar(codigo: "000000")).Should().Be("Código incorrecto. Intento 1 de 5.");
    }

    [Fact]
    public async Task Cinco_codigos_errados_bloquean_15_minutos()
    {
        for (var i = 1; i <= 4; i++) await Fallar(codigo: "000000");
        var accion = () => _auth.IniciarSesionAsync("laura.gomez@pitboss.co", "Casino2026", "000000");

        await accion.Should().ThrowAsync<AutenticacionException>().WithMessage("Código incorrecto. Cuenta bloqueada 15 minutos.");
        _laura.BloqueadoHasta.Should().Be(Ahora.AddMinutes(15));
    }

    [Fact]
    public async Task Cinco_contrasenas_erradas_tambien_bloquean()
    {
        for (var i = 1; i <= 4; i++) (await Fallar(contrasena: "Otra2026!")).Should().Be(MensajeGenerico);

        (await Fallar(contrasena: "Otra2026!")).Should().Be(MensajeBloqueada);
        _laura.BloqueadoHasta.Should().Be(Ahora.AddMinutes(15));
        _laura.IntentosFallidos.Should().Be(0);
    }

    [Fact]
    public async Task Bloqueada_no_ingresa_ni_con_datos_correctos()
    {
        for (var i = 1; i <= 5; i++) await Fallar(codigo: "000000");

        var accion = () => Ingresar();

        await accion.Should().ThrowAsync<AutenticacionException>().WithMessage(MensajeBloqueada);
    }

    [Fact]
    public async Task El_bloqueo_vence_a_los_15_minutos()
    {
        for (var i = 1; i <= 5; i++) await Fallar(codigo: "000000");
        _reloj.Avanzar(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        var u = await Ingresar();

        u.Should().BeSameAs(_laura);
        u.BloqueadoHasta.Should().BeNull();
    }

    [Fact]
    public async Task Cuenta_desactivada_no_ingresa()
    {
        _laura.Activo = false;

        var accion = () => Ingresar();

        await accion.Should().ThrowAsync<AutenticacionException>().WithMessage("Esta cuenta está desactivada.");
    }

    [Fact]
    public async Task Un_ingreso_correcto_reinicia_el_contador()
    {
        await Fallar(codigo: "000000");
        await Fallar(contrasena: "Otra2026!");
        _laura.IntentosFallidos.Should().Be(2);

        await Ingresar();

        _laura.IntentosFallidos.Should().Be(0);
    }

    [Fact]
    public void La_contrasena_temporal_es_segura()
    {
        var generadas = Enumerable.Range(0, 100).Select(_ => ContrasenaTemporal.Generar()).ToList();

        generadas.Should().OnlyContain(c => c.Length == 12 && ContrasenaTemporal.EsSegura(c));
        generadas.Should().OnlyContain(c => c.Any(char.IsLower) && c.Any(ch => "!@#$%&*?".Contains(ch)));
        generadas.Should().OnlyHaveUniqueItems();
    }

    [Theory, InlineData("corta1!"), InlineData("sinmayuscula1!"), InlineData("SinNumero!!"), InlineData("SinEspecial12")]
    public void EsSegura_rechaza_contrasenas_debiles(string c) => ContrasenaTemporal.EsSegura(c).Should().BeFalse();
}
