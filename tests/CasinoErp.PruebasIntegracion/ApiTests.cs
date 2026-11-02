using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CasinoErp.PruebasIntegracion;

/// <summary>Levanta la API con una base SQLite temporal y los datos de demostración.</summary>
public class CasinoErpFactory : WebApplicationFactory<Program>
{
    private readonly string _archivo = Path.Combine(Path.GetTempPath(), $"casino-erp-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("BaseDatos:Proveedor", "Sqlite");
        builder.UseSetting("BaseDatos:CargarDatosDemo", "true");
        builder.UseSetting("ConnectionStrings:CasinoErp", $"Data Source={_archivo};Pooling=False");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_archivo)) File.Delete(_archivo);
    }
}

/// <summary>Pruebas de integración: controlador + servicio + EF Core + base de datos real (SQLite).</summary>
public class ApiTests : IClassFixture<CasinoErpFactory>
{
    private readonly HttpClient _cliente;

    public ApiTests(CasinoErpFactory factory) => _cliente = factory.CreateClient();

    private static async Task<JsonElement> Json(HttpResponseMessage respuesta) =>
        (await respuesta.Content.ReadFromJsonAsync<JsonElement>());

    [Fact]
    public async Task Lista_los_jugadores_de_demostracion()
    {
        var jugadores = await _cliente.GetFromJsonAsync<JsonElement>("/api/jugadores");

        jugadores.GetArrayLength().Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public async Task Registra_y_consulta_un_jugador()
    {
        var crear = await _cliente.PostAsJsonAsync("/api/jugadores",
            new { documento = "1033444555", nombre = "Laura Gómez", correo = "laura@correo.com", fechaNacimiento = "1995-02-11" });

        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var cuerpo = await Json(crear);
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Jugador registrado exitosamente");
        cuerpo.GetProperty("datos").GetProperty("estado").GetString().Should().Be("ACTIVO");

        var consultado = await _cliente.GetFromJsonAsync<JsonElement>("/api/jugadores/1033444555");
        consultado.GetProperty("nombre").GetString().Should().Be("Laura Gómez");
    }

    [Fact]
    public async Task Regla_de_negocio_responde_400_con_el_mensaje()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/jugadores",
            new { documento = "1020456789", nombre = "Otra Persona", correo = "otra@correo.com", fechaNacimiento = "1988-01-10" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Json(respuesta)).GetProperty("mensaje").GetString().Should().Be("El documento ya está registrado");
    }

    [Fact]
    public async Task Registro_inexistente_responde_404()
    {
        var respuesta = await _cliente.GetAsync("/api/mesas/M-99");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Json(respuesta)).GetProperty("mensaje").GetString().Should().Be("Mesa no encontrada");
    }

    [Fact]
    public async Task No_retira_una_mesa_abierta()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/mesas/M-01/retirar", new { motivo = "Cambio de distribución" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Json(respuesta)).GetProperty("mensaje").GetString().Should().Be("No se puede retirar una mesa abierta");
    }

    [Fact]
    public async Task Compra_grande_queda_en_revision()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/caja/transacciones", new { tipo = "COMPRA", documento = "79456112", monto = 8_750_000 });

        var cuerpo = await Json(respuesta);
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Compra enviada a revisión de cumplimiento");
        cuerpo.GetProperty("datos").GetProperty("estado").GetString().Should().Be("EN_REVISION");
    }

    [Fact]
    public async Task Descuento_de_retencion_formateado_en_pesos()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/retencion/descuento", new { monto = 500_000 });

        (await Json(respuesta)).GetProperty("mensaje").GetString().Should().Be("Descuento de $100.000");
    }

    [Fact]
    public async Task Flujo_de_relleno_descuenta_la_boveda()
    {
        var saldoInicial = (await _cliente.GetFromJsonAsync<JsonElement>("/api/boveda")).GetProperty("saldo").GetInt64();

        var solicitud = await Json(await _cliente.PostAsJsonAsync("/api/rellenos", new { mesaNumero = "M-01", monto = 5_000_000, solicitadoPor = "Ana Torres" }));
        var id = solicitud.GetProperty("datos").GetProperty("id").GetInt32();
        var aprobado = await Json(await _cliente.PostAsJsonAsync($"/api/rellenos/{id}/aprobar", new { aprobadoPor = "Daniela Vargas" }));

        aprobado.GetProperty("datos").GetProperty("estado").GetString().Should().Be("APROBADO");
        var saldoFinal = (await _cliente.GetFromJsonAsync<JsonElement>("/api/boveda")).GetProperty("saldo").GetInt64();
        saldoFinal.Should().Be(saldoInicial - 5_000_000);
    }

    [Fact]
    public async Task Retiro_de_inventario_con_stock_insuficiente()
    {
        await _cliente.PostAsJsonAsync("/api/inventario/fichas", new { codigo = "FICHA-1M", denominacion = 1_000_000, categoria = "Morado", stockInicial = 2 });

        var respuesta = await _cliente.PostAsJsonAsync("/api/inventario/fichas/FICHA-1M/retiros", new { cantidad = 5 });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Json(respuesta)).GetProperty("mensaje").GetString().Should().Be("Stock insuficiente");
        var ficha = await _cliente.GetFromJsonAsync<JsonElement>("/api/inventario/fichas/FICHA-1M");
        ficha.GetProperty("stock").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task Resumen_del_tablero()
    {
        var resumen = await _cliente.GetFromJsonAsync<JsonElement>("/api/resumen");

        resumen.GetProperty("mesasTotales").GetInt32().Should().BeGreaterOrEqualTo(4);
        resumen.GetProperty("valorInventarioFichas").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Pago_de_jugador_no_registrado_y_anulacion()
    {
        var pago = await Json(await _cliente.PostAsJsonAsync("/api/caja/transacciones", new { tipo = "PAGO", monto = 650_000 }));
        var id = pago.GetProperty("datos").GetProperty("id").GetInt32();

        pago.GetProperty("mensaje").GetString().Should().Be("Pago registrado");
        var anulada = await Json(await _cliente.PostAsync($"/api/caja/transacciones/{id}/anular", null));
        anulada.GetProperty("datos").GetProperty("estado").GetString().Should().Be("ANULADA");
    }

    [Fact]
    public async Task Oferta_de_retencion_elegida_por_el_crupier()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/retencion/ofertas",
            new { documento = "80112774", minutosSesion = 40, senal = "Quiere retirarse", origen = "Mesa M-01", aceptada = true, tipo = "DESCUENTO_FICHAS" });

        var cuerpo = await Json(respuesta);
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Oferta de retención emitida");
        cuerpo.GetProperty("datos").GetProperty("tipo").GetString().Should().Be("DESCUENTO_FICHAS");
        cuerpo.GetProperty("datos").GetProperty("valor").GetInt64().Should().Be(20);
    }

    [Fact]
    public async Task Autoexcluido_no_recibe_oferta()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/retencion/ofertas",
            new { documento = "80554219", minutosSesion = 60, senal = "Quiere retirarse", aceptada = true });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Json(respuesta)).GetProperty("mensaje").GetString().Should().Be("No se ofrece retención: juego responsable");
    }

    [Fact]
    public async Task Actualiza_el_perfil_del_jugador()
    {
        var respuesta = await _cliente.PutAsJsonAsync("/api/jugadores/1032778410/perfil",
            new { telefono = "+57 312 000 0000", ciudad = "Cajicá", franja = "Noche", apuestaPromedio = 80_000, preferencias = new[] { "Tragamonedas", "Ruleta" } });

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var jugador = await _cliente.GetFromJsonAsync<JsonElement>("/api/jugadores/1032778410");
        jugador.GetProperty("preferencias").GetArrayLength().Should().Be(2);
        jugador.GetProperty("franja").GetString().Should().Be("Noche");
    }

    [Fact]
    public async Task Sirve_el_front_del_erp()
    {
        var html = await _cliente.GetStringAsync("/");

        html.Should().Contain("PitBoss ERP");
    }
}
