using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CasinoErp.PruebasIntegracion;

/// <summary>HU-04 · API de empleados y nómina contra una base SQLite real con los datos de demostración (Plan-Felipe F4 y F5).</summary>
public class PersonalNominaApiTests : IClassFixture<CasinoErpFactory>
{
    private readonly HttpClient _cliente;

    public PersonalNominaApiTests(CasinoErpFactory factory) => _cliente = factory.CreateClient();

    [Fact]
    public async Task Lista_los_18_empleados_de_demostracion()
    {
        var lista = await _cliente.GetFromJsonAsync<JsonElement>("/api/empleados");

        lista.GetArrayLength().Should().BeGreaterOrEqualTo(18);
        lista.EnumerateArray().Should().Contain(e => e.GetProperty("documento").GetString() == "1010334923"
                                                    && e.GetProperty("turno").GetString() == "NOCHE");   // Bruno Lara
    }

    [Fact]
    public async Task Los_empleados_de_demostracion_coinciden_con_el_mockup()
    {
        var lista = (await _cliente.GetFromJsonAsync<JsonElement>("/api/empleados")).EnumerateArray().ToList();
        JsonElement Por(string documento) => lista.Single(e => e.GetProperty("documento").GetString() == documento);

        var ana = Por("1010200300");
        ana.GetProperty("nombre").GetString().Should().Be("Ana Torres");
        ana.GetProperty("cargo").GetString().Should().Be("CRUPIER");
        ana.GetProperty("juego").GetString().Should().Be("Blackjack");
        ana.GetProperty("area").GetString().Should().Be("M-01");
        ana.GetProperty("salarioBase").GetInt64().Should().Be(2_600_000);
        ana.GetProperty("tipoContrato").GetString().Should().Be("INDEFINIDO");
        ana.GetProperty("fechaIngreso").GetString().Should().Be("2025-01-15");

        var laura = Por("1010216138");
        laura.GetProperty("cargo").GetString().Should().Be("CAJERO");
        laura.GetProperty("juego").GetString().Should().Be("Caja");
        laura.GetProperty("area").GetString().Should().Be("Caja 2");

        var daniela = Por("1010327004");
        daniela.GetProperty("cargo").GetString().Should().Be("JEFE_DE_PISO");
        daniela.GetProperty("juego").GetString().Should().Be("Supervisión");
        daniela.GetProperty("salarioBase").GetInt64().Should().Be(4_200_000);

        Por("1010224057").GetProperty("nombre").GetString().Should().Be("Miguel Á. Soto");
    }

    [Fact]
    public async Task Registra_y_consulta_un_empleado()
    {
        var crear = await _cliente.PostAsJsonAsync("/api/empleados", new { documento = "1099888777", nombre = "Marta Gil", cargo = "CAJERO" });

        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var cuerpo = await crear.LeerJsonAsync();
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Empleado registrado");
        var consultado = await _cliente.GetFromJsonAsync<JsonElement>("/api/empleados/1099888777");
        consultado.GetProperty("juego").GetString().Should().Be("Caja");
        consultado.GetProperty("salarioBase").GetInt64().Should().Be(2_300_000);
    }

    [Fact]
    public async Task Documento_duplicado_responde_400()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/empleados",
            new { documento = "1.010.200.300", nombre = "Otra Persona", cargo = "CRUPIER", juego = "Ruleta" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await respuesta.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Ya existe un empleado con ese documento.");
    }

    [Fact]
    public async Task Empleado_inexistente_responde_404()
    {
        var respuesta = await _cliente.GetAsync("/api/empleados/999999");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await respuesta.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Empleado no encontrado");
    }

    [Fact]
    public async Task Actualiza_un_empleado()
    {
        await _cliente.PostAsJsonAsync("/api/empleados", new { documento = "1099888701", nombre = "Iris Pinto", cargo = "CRUPIER", juego = "Craps" });

        var respuesta = await _cliente.PutAsJsonAsync("/api/empleados/1099888701",
            new { turno = "NOCHE", area = "M-06", tipoContrato = "TERMINO_FIJO", salarioBase = 2_700_000 });

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var datos = (await respuesta.LeerJsonAsync()).GetProperty("datos");
        datos.GetProperty("turno").GetString().Should().Be("NOCHE");
        datos.GetProperty("area").GetString().Should().Be("M-06");
        datos.GetProperty("tipoContrato").GetString().Should().Be("TERMINO_FIJO");
        datos.GetProperty("salarioBase").GetInt64().Should().Be(2_700_000);
    }

    [Fact]
    public async Task Retira_un_empleado()
    {
        await _cliente.PostAsJsonAsync("/api/empleados", new { documento = "1099888702", nombre = "Hugo Mesa", cargo = "CAJERO" });

        var respuesta = await _cliente.PostAsJsonAsync("/api/empleados/1099888702/retirar", new { motivo = "Renuncia" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var cuerpo = await respuesta.LeerJsonAsync();
        cuerpo.GetProperty("mensaje").GetString().Should().Be("Empleado retirado");
        cuerpo.GetProperty("datos").GetProperty("activo").GetBoolean().Should().BeFalse();
        var activos = await _cliente.GetFromJsonAsync<JsonElement>("/api/empleados?soloActivos=true");
        activos.EnumerateArray().Should().NotContain(e => e.GetProperty("documento").GetString() == "1099888702");
    }
}
