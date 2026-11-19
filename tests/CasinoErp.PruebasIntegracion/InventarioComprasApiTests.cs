using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CasinoErp.PruebasIntegracion;

/// <summary>HU-05 · Umbral mínimo y alertas de stock crítico contra una base SQLite real (Plan-Camilo C2).</summary>
public class InventarioComprasApiTests(CasinoErpFactory factory) : IClassFixture<CasinoErpFactory>, IAsyncLifetime
{
    private HttpClient _cliente = null!;

    public async Task InitializeAsync() => _cliente = await factory.ClienteComoAsync("laura.gomez@pitboss.co");

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task El_umbral_de_demo_de_500K_es_400()
    {
        var ficha = await _cliente.GetFromJsonAsync<JsonElement>("/api/inventario/fichas/FICHA-500K");

        ficha.GetProperty("stockMinimo").GetInt32().Should().Be(400);
        ficha.GetProperty("enNivelCritico").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Actualiza_el_minimo()
    {
        var r = await _cliente.PutAsJsonAsync("/api/inventario/fichas/FICHA-5K/minimo", new { stockMinimo = 1200 });

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var cuerpo = await r.LeerJsonAsync();
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Stock mínimo actualizado");
        cuerpo.GetProperty("datos").GetProperty("stockMinimo").GetInt32().Should().Be(1200);
        var ficha = await _cliente.GetFromJsonAsync<JsonElement>("/api/inventario/fichas/FICHA-5K");
        ficha.GetProperty("stockMinimo").GetInt32().Should().Be(1200);
    }

    [Fact]
    public async Task Minimo_negativo_responde_400()
    {
        var r = await _cliente.PutAsJsonAsync("/api/inventario/fichas/FICHA-10K/minimo", new { stockMinimo = -1 });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("El stock mínimo no puede ser negativo");
    }

    [Fact]
    public async Task Un_crupier_no_ve_alertas_de_inventario()
    {
        var crupier = await factory.ClienteComoAsync("ana.torres@pitboss.co");

        (await crupier.GetAsync("/api/inventario/alertas")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await crupier.PutAsJsonAsync("/api/inventario/fichas/FICHA-5K/minimo", new { stockMinimo = 1 }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Flujo de la Sprint Review. Tiene su propia base porque retira de FICHA-500K
    /// y las pruebas de arriba asumen que esa ficha sigue en 420.
    /// </summary>
    public class Demo(CasinoErpFactory factory) : IClassFixture<CasinoErpFactory>, IAsyncLifetime
    {
        private HttpClient _cliente = null!;

        public async Task InitializeAsync() => _cliente = await factory.ClienteComoAsync("laura.gomez@pitboss.co");

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task La_compra_que_baja_la_boveda_del_umbral_genera_alerta()
        {
            var r = await _cliente.PostAsJsonAsync("/api/inventario/fichas/FICHA-500K/retiros", new { cantidad = 25 });
            var ficha = (await r.LeerJsonAsync()).GetProperty("datos");
            ficha.GetProperty("stock").GetInt32().Should().Be(395);
            ficha.GetProperty("enNivelCritico").GetBoolean().Should().BeTrue();

            var alertas = await _cliente.GetFromJsonAsync<JsonElement>("/api/inventario/alertas");
            alertas.EnumerateArray().Should().ContainSingle(a => a.GetProperty("codigoFicha").GetString() == "FICHA-500K"
                                                               && a.GetProperty("estado").GetString() == "ABIERTA");
        }
    }

    // ---------------------------------------------------------------- C5 · proveedores y compras

    [Fact]
    public async Task CRUD_de_proveedor()
    {
        var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");

        var alta = await gerente.PostAsJsonAsync("/api/proveedores",
            new { nit = "902000111-3", nombre = "Fichas del Caribe", correo = "Ventas@FichasCaribe.co", telefono = "+57 605 555 0303" });
        alta.StatusCode.Should().Be(HttpStatusCode.Created);
        var cuerpo = await alta.LeerJsonAsync();
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Proveedor registrado");
        cuerpo.GetProperty("datos").GetProperty("correo").GetString().Should().Be("ventas@fichascaribe.co");

        var cambio = await gerente.PutAsJsonAsync("/api/proveedores/902000111-3",
            new { nit = "000000000-0", nombre = "Fichas del Caribe S.A.S.", correo = "ventas@fichascaribe.co", telefono = "+57 605 555 0303" });
        (await cambio.LeerJsonAsync()).GetProperty("datos").GetProperty("nombre").GetString().Should().Be("Fichas del Caribe S.A.S.");

        var baja = await gerente.PostAsync("/api/proveedores/902000111-3/desactivar", null);
        (await baja.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Proveedor desactivado");
        var activos = await gerente.GetFromJsonAsync<JsonElement>("/api/proveedores?soloActivos=true");
        activos.EnumerateArray().Should().NotContain(p => p.GetProperty("nit").GetString() == "902000111-3");

        var alza = await gerente.PostAsync("/api/proveedores/902000111-3/reactivar", null);
        (await alza.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Proveedor reactivado");
        (await gerente.GetFromJsonAsync<JsonElement>("/api/proveedores/902000111-3")).GetProperty("activo").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Los_proveedores_de_demo_existen()
    {
        var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");

        var lista = await gerente.GetFromJsonAsync<JsonElement>("/api/proveedores");

        lista.EnumerateArray().Select(p => p.GetProperty("nit").GetString()).Should().Contain(["901234567-1", "900765432-8"]);
    }

    [Fact]
    public async Task Nit_invalido_responde_400()
    {
        var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");

        var r = await gerente.PostAsJsonAsync("/api/proveedores", new { nit = "123", nombre = "X", correo = "x@x.co", telefono = "1" });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("El NIT no es válido.");
    }

    [Fact]
    public async Task Orden_inexistente_responde_404()
    {
        var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");

        var r = await gerente.PostAsJsonAsync("/api/compras/ordenes/999/enviar", new { nit = (string?)null });

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Orden de compra no encontrada");
    }

    [Fact]
    public async Task Crea_una_orden_manual_y_la_cancela()
    {
        var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");

        var r = await gerente.PostAsJsonAsync("/api/compras/ordenes", new { codigoFicha = "FICHA-25K", cantidad = 1500, nit = "901234567-1" });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        var cuerpo = await r.LeerJsonAsync();
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Orden de compra creada");
        var orden = cuerpo.GetProperty("datos");
        orden.GetProperty("origen").GetString().Should().Be("Manual");
        orden.GetProperty("codigo").GetString().Should().StartWith("OC-");

        var c = await gerente.PostAsync($"/api/compras/ordenes/{orden.GetProperty("id").GetInt32()}/cancelar", null);
        var cancelada = await c.LeerJsonAsync();
        cancelada.GetProperty("mensaje").GetString().Should().Be("Orden cancelada");
        cancelada.GetProperty("datos").GetProperty("estado").GetString().Should().Be("CANCELADA");
    }

    // ---------------------------------------------------------------- C7 · autorización

    [Fact]
    public async Task Un_cajero_ve_las_ordenes_pero_no_administra_proveedores()
    {
        var cajero = await factory.ClienteComoAsync("laura.gomez@pitboss.co");

        (await cajero.GetAsync("/api/compras/ordenes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cajero.PostAsJsonAsync("/api/proveedores", new { nit = "903000111-1", nombre = "X", correo = "x@x.co", telefono = "1" }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Un_crupier_no_ve_compras_ni_proveedores()
    {
        var crupier = await factory.ClienteComoAsync("ana.torres@pitboss.co");

        (await crupier.GetAsync("/api/compras/ordenes")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await crupier.GetAsync("/api/proveedores")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Sin_sesion_responde_401()
    {
        var anonimo = factory.CreateClient();

        (await anonimo.GetAsync("/api/proveedores")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>La compra en caja (cajero) deja la bóveda bajo el umbral y crea el borrador OC-0001.</summary>
    public class OrdenDesdeAlerta(CasinoErpFactory factory) : IClassFixture<CasinoErpFactory>
    {
        [Fact]
        public async Task La_alerta_de_la_compra_crea_una_orden_en_borrador()
        {
            var cajero = await factory.ClienteComoAsync("laura.gomez@pitboss.co");
            await cajero.PostAsJsonAsync("/api/inventario/fichas/FICHA-500K/retiros", new { cantidad = 25 });

            var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");
            var ordenes = await gerente.GetFromJsonAsync<JsonElement>("/api/compras/ordenes");

            var o = ordenes.EnumerateArray().Single(x => x.GetProperty("codigoFicha").GetString() == "FICHA-500K");
            o.GetProperty("estado").GetString().Should().Be("BORRADOR");
            o.GetProperty("origen").GetString().Should().Be("Alerta de stock");
            o.GetProperty("cantidad").GetInt32().Should().Be(165);
            o.GetProperty("proveedorNit").GetString().Should().Be("901234567-1");
            o.GetProperty("codigo").GetString().Should().Be("OC-0001");
        }
    }

    /// <summary>Enviar y recibir la orden repone la bóveda: 395 + 165 = 560 y la alerta se atiende.</summary>
    public class RecepcionDeOrden(CasinoErpFactory factory) : IClassFixture<CasinoErpFactory>
    {
        [Fact]
        public async Task Enviar_y_recibir_la_orden_repone_la_boveda_y_atiende_la_alerta()
        {
            var cajero = await factory.ClienteComoAsync("laura.gomez@pitboss.co");
            await cajero.PostAsJsonAsync("/api/inventario/fichas/FICHA-500K/retiros", new { cantidad = 25 });
            var gerente = await factory.ClienteComoAsync("daniel.ruiz@pitboss.co");
            var id = (await gerente.GetFromJsonAsync<JsonElement>("/api/compras/ordenes")).EnumerateArray()
                .Single(x => x.GetProperty("codigoFicha").GetString() == "FICHA-500K").GetProperty("id").GetInt32();

            var enviada = await (await gerente.PostAsJsonAsync($"/api/compras/ordenes/{id}/enviar", new { nit = (string?)null })).LeerJsonAsync();
            enviada.GetProperty("mensaje").GetString().Should().Be("Orden enviada");
            enviada.GetProperty("datos").GetProperty("estado").GetString().Should().Be("ENVIADA");

            var recibida = await (await gerente.PostAsync($"/api/compras/ordenes/{id}/recibir", null)).LeerJsonAsync();
            recibida.GetProperty("mensaje").GetString().Should().Be("Orden recibida");
            recibida.GetProperty("datos").GetProperty("estado").GetString().Should().Be("RECIBIDA");

            (await gerente.GetFromJsonAsync<JsonElement>("/api/inventario/fichas/FICHA-500K")).GetProperty("stock").GetInt32().Should().Be(560);
            var abiertas = await gerente.GetFromJsonAsync<JsonElement>("/api/inventario/alertas");
            abiertas.EnumerateArray().Should().NotContain(a => a.GetProperty("codigoFicha").GetString() == "FICHA-500K");
        }
    }
}
