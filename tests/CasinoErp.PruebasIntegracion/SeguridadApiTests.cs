using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CasinoErp.Dominio.Seguridad;
using CasinoErp.Infraestructura;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.PruebasIntegracion;

/// <summary>
/// HU-06 · Seguridad contra una base SQLite real con las cuentas de demostración (Plan-Ivan I4 e I5).
/// Las pruebas que desactivan cuentas, cambian roles o contraseñas usan cuentas que ninguna otra prueba de la clase necesita
/// (sebastian.prada, andrea.mendez o usuarios creados en la propia prueba), así que el orden no importa.
/// </summary>
public class SeguridadApiTests : IClassFixture<CasinoErpFactory>
{
    private static readonly string[] CuentasDemo =
    [
        "daniel.ruiz@pitboss.co", "ivan.leon@pitboss.co", "sebastian.prada@pitboss.co", "daniela.vargas@pitboss.co",
        "ana.torres@pitboss.co", "jorge.diaz@pitboss.co", "laura.gomez@pitboss.co", "felipe.cortes@pitboss.co",
        "andrea.mendez@pitboss.co", "miguel.soto@pitboss.co", "carolina.rios@pitboss.co",
    ];

    private readonly CasinoErpFactory _factory;

    public SeguridadApiTests(CasinoErpFactory factory)
    {
        _factory = factory;
        _factory.CreateClient().Dispose();   // arranca el host para que corra PrepararBaseDatos
    }

    private Task<HttpResponseMessage> Login(string correo, string contrasena = Autenticacion.ContrasenaDemo, string codigo = Autenticacion.CodigoDemo) =>
        _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { correo, contrasena, codigo });

    private static async Task<int> IdDe(HttpClient admin, string correo)
    {
        var lista = await admin.GetFromJsonAsync<JsonElement>("/api/usuarios");
        return lista.EnumerateArray().Single(u => u.GetProperty("correo").GetString() == correo).GetProperty("id").GetInt32();
    }

    private static async Task<string[]> Modulos(HttpResponseMessage login) =>
        (await login.LeerJsonAsync()).GetProperty("modulos").EnumerateArray().Select(x => x.GetString()!).ToArray();

    // ---------- I4: infraestructura y cuentas de demo ----------

    [Fact]
    public void La_semilla_crea_las_11_cuentas_de_demo()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasinoErpDbContext>();
        // Las demás pruebas de la clase crean usuarios: se cuentan solo las 11 cuentas de C6 (Id 1 a 11).
        var usuarios = db.Set<Usuario>().Where(u => u.Id <= 11).OrderBy(u => u.Id).ToList();
        usuarios.Should().HaveCount(11);
        usuarios.Select(u => u.Correo).Should().BeEquivalentTo(CuentasDemo);
        usuarios.Single(u => u.Correo == "carolina.rios@pitboss.co").Activo.Should().BeFalse();
        usuarios.Single(u => u.Correo == "ana.torres@pitboss.co").EmpleadoDocumento.Should().Be("1010200300");
        var hasher = scope.ServiceProvider.GetRequiredService<IHasherContrasena>();
        hasher.Verificar(usuarios.Single(u => u.Correo == "daniel.ruiz@pitboss.co").HashContrasena, "Casino2026").Should().BeTrue();
    }

    [Fact]
    public void El_verificador_acepta_un_codigo_TOTP_real_y_rechaza_uno_errado()
    {
        using var scope = _factory.Services.CreateScope();
        var totp = scope.ServiceProvider.GetRequiredService<IVerificadorTotp>();
        var secreto = totp.NuevoSecreto();
        var codigo = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(secreto)).ComputeTotp();
        var errado = codigo == "000000" ? "111111" : "000000";

        totp.Verificar(secreto, codigo).Should().BeTrue();
        totp.Verificar(secreto, errado).Should().BeFalse();
        totp.UriConfiguracion(secreto, "marta.gil@pitboss.co")
            .Should().Be($"otpauth://totp/PitBoss:marta.gil%40pitboss.co?secret={secreto}&issuer=PitBoss");
    }

    // ---------- I5: JWT, login y autorización por módulo ----------

    [Fact]
    public async Task Sin_token_la_api_responde_401() =>
        (await _factory.CreateClient().GetAsync("/api/jugadores")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    [Fact]
    public async Task Salud_y_front_son_anonimos()
    {
        var cliente = _factory.CreateClient();
        (await cliente.GetAsync("/api/salud")).StatusCode.Should().Be(HttpStatusCode.OK);
        var front = await cliente.GetAsync("/");
        front.StatusCode.Should().Be(HttpStatusCode.OK);
        (await front.Content.ReadAsStringAsync()).Should().Contain("<html");
    }

    [Fact]
    public async Task El_login_devuelve_token_y_los_modulos_del_rol()
    {
        var r = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { correo = "laura.gomez@pitboss.co", contrasena = "Casino2026", codigo = "482913" });
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var j = await r.LeerJsonAsync();
        j.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        j.GetProperty("usuario").GetProperty("rol").GetString().Should().Be("CAJERO");
        j.GetProperty("modulos").EnumerateArray().Select(x => x.GetString()).Should().BeEquivalentTo(["caja", "inventario"]);
    }

    [Fact]
    public async Task Codigo_errado_responde_401_con_mensaje()
    {
        var r = await Login("daniela.vargas@pitboss.co", codigo: "000000");

        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Código incorrecto. Intento 1 de 5.");
    }

    [Fact]
    public async Task Cuenta_inactiva_responde_401()
    {
        var r = await Login("carolina.rios@pitboss.co");

        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Esta cuenta está desactivada.");
    }

    [Fact]
    public async Task El_cajero_entra_a_caja_pero_no_a_usuarios_ni_al_resumen()
    {
        var laura = await _factory.ClienteComoAsync("laura.gomez@pitboss.co");

        (await laura.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await laura.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await laura.GetAsync("/api/resumen")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task El_crupier_solo_entra_a_mesas_y_retencion()
    {
        var ana = await _factory.ClienteComoAsync("ana.torres@pitboss.co");

        (await ana.GetAsync("/api/mesas")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ana.GetAsync("/api/retencion/ofertas")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ana.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task El_admin_crea_usuario_y_la_respuesta_no_expone_secretos()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var r = await admin.PostAsJsonAsync("/api/usuarios", new { nombre = "Marta Gil", correo = "marta.gil@pitboss.co", rol = "CAJERO" });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        var texto = await r.Content.ReadAsStringAsync();
        texto.Should().Contain("contrasenaTemporal").And.Contain("otpauth://")
             .And.NotContain("hashContrasena").And.NotContain("secretoTotp");
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Usuario creado");
    }

    [Fact]
    public async Task El_listado_de_usuarios_no_expone_secretos()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var r = await admin.GetAsync("/api/usuarios");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var texto = await r.Content.ReadAsStringAsync();
        texto.Should().Contain("ivan.leon@pitboss.co").And.NotContain("hashContrasena").And.NotContain("secretoTotp");
    }

    [Fact]
    public async Task El_gerente_no_administra_usuarios()
    {
        var gerente = await _factory.ClienteComoAsync("daniel.ruiz@pitboss.co");

        (await gerente.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Un_token_de_usuario_desactivado_deja_de_servir()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var creado = await admin.PostAsJsonAsync("/api/usuarios", new { nombre = "Cajero Temporal", correo = "cajero.temporal@pitboss.co", rol = "CAJERO" });
        var datos = (await creado.LeerJsonAsync()).GetProperty("datos");
        var id = datos.GetProperty("usuario").GetProperty("id").GetInt32();
        var cajero = await _factory.ClienteComoAsync("cajero.temporal@pitboss.co", datos.GetProperty("contrasenaTemporal").GetString()!);
        (await cajero.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.OK);

        (await admin.PostAsync($"/api/usuarios/{id}/desactivar", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await cajero.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cambiar_el_rol_invalida_el_token_anterior()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var sebastian = await _factory.ClienteComoAsync("sebastian.prada@pitboss.co");
        var id = await IdDe(admin, "sebastian.prada@pitboss.co");

        var r = await admin.PutAsJsonAsync($"/api/usuarios/{id}/rol", new { rol = "CAJERO" });
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Rol actualizado");

        (await sebastian.GetAsync("/api/jugadores")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var nuevo = await Login("sebastian.prada@pitboss.co");
        (await Modulos(nuevo)).Should().BeEquivalentTo(["caja", "inventario"]);
    }

    [Fact]
    public async Task Asignar_un_rol_cambia_los_modulos_accesibles()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var antes = await _factory.ClienteComoAsync("andrea.mendez@pitboss.co");
        (await antes.GetAsync("/api/resumen")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await antes.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.OK);

        var id = await IdDe(admin, "andrea.mendez@pitboss.co");
        (await admin.PutAsJsonAsync($"/api/usuarios/{id}/rol", new { rol = "ANFITRION" })).StatusCode.Should().Be(HttpStatusCode.OK);

        var despues = await _factory.ClienteComoAsync("andrea.mendez@pitboss.co");
        (await despues.GetAsync("/api/resumen")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await despues.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task No_se_puede_quitar_el_rol_al_ultimo_admin()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var id = await IdDe(admin, "ivan.leon@pitboss.co");

        var r = await admin.PutAsJsonAsync($"/api/usuarios/{id}/rol", new { rol = "GERENTE" });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("No puedes quitar el rol al último administrador.");
    }

    [Fact]
    public async Task Cambiar_contrasena()
    {
        var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
        var creado = await admin.PostAsJsonAsync("/api/usuarios", new { nombre = "Pedro Cambio", correo = "pedro.cambio@pitboss.co", rol = "MARKETING" });
        var temporal = (await creado.LeerJsonAsync()).GetProperty("datos").GetProperty("contrasenaTemporal").GetString()!;
        var pedro = await _factory.ClienteComoAsync("pedro.cambio@pitboss.co", temporal);

        var r = await pedro.PostAsJsonAsync("/api/auth/cambiar-contrasena", new { actual = temporal, nueva = "Nueva#2026x" });

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Contraseña actualizada");
        var login = await Login("pedro.cambio@pitboss.co", "Nueva#2026x");
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        (await login.LeerJsonAsync()).GetProperty("usuario").GetProperty("debeCambiarContrasena").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Cambiar_contrasena_con_la_actual_errada_responde_400_y_no_cierra_la_sesion()
    {
        var jorge = await _factory.ClienteComoAsync("jorge.diaz@pitboss.co");

        var r = await jorge.PostAsJsonAsync("/api/auth/cambiar-contrasena", new { actual = "Otra2026!", nueva = "Nueva#2026x" });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("La contraseña actual no es correcta.");
    }

    [Fact]
    public async Task Yo_devuelve_el_usuario_y_sus_modulos()
    {
        var felipe = await _factory.ClienteComoAsync("felipe.cortes@pitboss.co");

        var j = await felipe.GetFromJsonAsync<JsonElement>("/api/auth/yo");

        j.GetProperty("usuario").GetProperty("correo").GetString().Should().Be("felipe.cortes@pitboss.co");
        j.GetProperty("usuario").GetProperty("rol").GetString().Should().Be("TALENTO_HUMANO");
        j.GetProperty("modulos").EnumerateArray().Select(x => x.GetString()).Should().BeEquivalentTo(["inicio", "competencias", "rrhh"]);
    }

    [Fact]
    public async Task Todos_los_controladores_exigen_token()
    {
        var anonimo = _factory.CreateClient();
        string[] rutas = ["/api/jugadores", "/api/mesas", "/api/caja/transacciones", "/api/retencion/ofertas", "/api/rellenos",
                          "/api/inventario/fichas", "/api/resumen", "/api/empleados", "/api/nomina", "/api/usuarios", "/api/auth/yo"];

        foreach (var ruta in rutas)
            (await anonimo.GetAsync(ruta)).StatusCode.Should().Be(HttpStatusCode.Unauthorized, ruta);
    }
}
