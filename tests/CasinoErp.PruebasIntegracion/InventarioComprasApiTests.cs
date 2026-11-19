using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CasinoErp.PruebasIntegracion;

/// <summary>HU-05 · Umbral mínimo y alertas de stock crítico contra una base SQLite real (Plan-Camilo C2).</summary>
public class InventarioComprasApiTests : IClassFixture<CasinoErpFactory>
{
    private readonly HttpClient _cliente;

    public InventarioComprasApiTests(CasinoErpFactory factory) => _cliente = factory.CreateClient();

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

    /// <summary>
    /// Flujo de la Sprint Review. Tiene su propia base porque retira de FICHA-500K
    /// y las pruebas de arriba asumen que esa ficha sigue en 420.
    /// </summary>
    public class Demo : IClassFixture<CasinoErpFactory>
    {
        private readonly HttpClient _cliente;

        public Demo(CasinoErpFactory factory) => _cliente = factory.CreateClient();

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
}
