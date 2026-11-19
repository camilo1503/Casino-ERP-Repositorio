using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CasinoErp.PruebasIntegracion;

/// <summary>Preparación de la base de datos al arrancar (D-006: BaseDatos:RecrearAlIniciar).</summary>
public class BaseDatosTests
{
    [Fact]
    public async Task RecrearAlIniciar_borra_los_cambios_y_vuelve_a_cargar_los_datos_demo()
    {
        using var original = new CasinoErpFactory();
        var cliente = original.CreateClient();
        var crear = await cliente.PostAsJsonAsync("/api/jugadores",
            new { documento = "1077888999", nombre = "Persona Temporal", correo = "temporal@correo.com", fechaNacimiento = "1990-03-03" });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);

        using var recreada = original.WithWebHostBuilder(b => b.UseSetting("BaseDatos:RecrearAlIniciar", "true"));
        var clienteRecreado = recreada.CreateClient();

        (await clienteRecreado.GetAsync("/api/jugadores/1077888999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var jugadores = await clienteRecreado.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/jugadores");
        jugadores.GetArrayLength().Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public async Task Salud_responde_ok()
    {
        using var factory = new CasinoErpFactory();
        var salud = await factory.CreateClient().GetFromJsonAsync<System.Text.Json.JsonElement>("/api/salud");
        salud.GetProperty("estado").GetString().Should().Be("ok");
    }

    [Fact]
    public async Task Sin_RecrearAlIniciar_los_cambios_se_conservan()
    {
        using var original = new CasinoErpFactory();
        var cliente = original.CreateClient();
        await cliente.PostAsJsonAsync("/api/jugadores",
            new { documento = "1077888998", nombre = "Persona Persistente", correo = "persiste@correo.com", fechaNacimiento = "1990-03-03" });

        using var reiniciada = original.WithWebHostBuilder(_ => { });
        (await reiniciada.CreateClient().GetAsync("/api/jugadores/1077888998")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
