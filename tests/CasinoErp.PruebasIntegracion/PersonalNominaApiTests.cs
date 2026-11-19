using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CasinoErp.PruebasIntegracion;

/// <summary>HU-04 · API de empleados y nómina contra una base SQLite real con los datos de demostración (Plan-Felipe F4 y F5).</summary>
public class PersonalNominaApiTests : IClassFixture<CasinoErpFactory>, IAsyncLifetime
{
    private readonly CasinoErpFactory _factory;
    private HttpClient _cliente = null!;

    public PersonalNominaApiTests(CasinoErpFactory factory) => _factory = factory;

    public async Task InitializeAsync() => _cliente = await _factory.ClienteComoAsync("felipe.cortes@pitboss.co");
    public Task DisposeAsync() => Task.CompletedTask;

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

    // Plan-Felipe F7: autorización por módulo (contrato C5). La cajera no tiene Rrhh; el jefe de piso tiene
    // Mesas y Competencias (ve empleados) pero no Rrhh (no ve nómina).

    [Fact]
    public async Task Un_cajero_no_puede_calcular_la_nomina()
    {
        var cajero = await _factory.ClienteComoAsync("laura.gomez@pitboss.co");

        var respuesta = await cajero.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 10 });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task El_jefe_de_piso_lista_empleados_pero_no_ve_nomina()
    {
        var jefe = await _factory.ClienteComoAsync("daniela.vargas@pitboss.co");

        (await jefe.GetAsync("/api/empleados")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await jefe.GetAsync("/api/nomina")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Sin_sesion_no_se_puede_listar_empleados()
    {
        var anonimo = _factory.CreateClient();

        (await anonimo.GetAsync("/api/empleados")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

/// <summary>
/// HU-04 · API de nómina (Plan-Felipe F5). Va en su propia clase, con su propia base, para que los empleados
/// que registran las pruebas de PersonalNominaApiTests no cambien el número de filas de la nómina de octubre.
/// </summary>
public class NominaApiTests : IClassFixture<CasinoErpFactory>, IAsyncLifetime
{
    private readonly CasinoErpFactory _factory;
    private HttpClient _cliente = null!;

    public NominaApiTests(CasinoErpFactory factory) => _factory = factory;

    public async Task InitializeAsync() => _cliente = await _factory.ClienteComoAsync("felipe.cortes@pitboss.co");
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Calcula_la_nomina_de_octubre_con_los_empleados_de_demostracion()
    {
        var r = await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 10 });

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var j = await r.LeerJsonAsync();
        j.GetProperty("mensaje").GetString().Should().Be("Nómina calculada");
        var datos = j.GetProperty("datos");
        datos.GetProperty("estado").GetString().Should().Be("BORRADOR");
        datos.GetProperty("detalle").GetArrayLength().Should().Be(18);
        datos.GetProperty("totalNeto").GetInt64().Should().BeGreaterThan(0);
        datos.GetProperty("detalle").EnumerateArray()
             .Single(d => d.GetProperty("empleadoDocumento").GetString() == "1010334923")
             .GetProperty("neto").GetInt64().Should().Be(3_229_200);       // Bruno Lara, turno noche
        datos.GetProperty("detalle").EnumerateArray()
             .Single(d => d.GetProperty("empleadoDocumento").GetString() == "1010200300")
             .GetProperty("auxilioTransporte").GetInt64().Should().Be(249_095); // Ana Torres, con auxilio
    }

    [Fact]
    public async Task Los_totales_calculados_aparecen_en_el_json()
    {
        var datos = (await (await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 9 })).LeerJsonAsync())
            .GetProperty("datos");
        var detalle = datos.GetProperty("detalle").EnumerateArray().ToList();

        datos.GetProperty("totalNeto").GetInt64().Should().Be(detalle.Sum(d => d.GetProperty("neto").GetInt64()));
        datos.GetProperty("totalRecargos").GetInt64().Should().Be(910_000);
        datos.GetProperty("totalDeducciones").GetInt64().Should().Be(detalle.Sum(d => d.GetProperty("totalDeducciones").GetInt64()));
        datos.GetProperty("totalAportesEmpleador").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Aprueba_y_luego_no_permite_recalcular()
    {
        var calculada = await (await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 11 })).LeerJsonAsync();
        var id = calculada.GetProperty("datos").GetProperty("id").GetInt32();

        var aprobar = await _cliente.PostAsync($"/api/nomina/{id}/aprobar", null);
        aprobar.StatusCode.Should().Be(HttpStatusCode.OK);
        var aprobada = await aprobar.LeerJsonAsync();
        aprobada.GetProperty("mensaje").GetString().Should().Be("Nómina aprobada");
        aprobada.GetProperty("datos").GetProperty("estado").GetString().Should().Be("APROBADA");

        var recalcular = await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 11 });
        recalcular.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await recalcular.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("La nómina de 11/2026 ya está aprobada.");
    }

    [Fact]
    public async Task Recalcular_un_borrador_lo_reemplaza()
    {
        await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 8 });
        var segunda = await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 8 });

        segunda.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await _cliente.GetFromJsonAsync<JsonElement>("/api/nomina");
        lista.EnumerateArray().Count(n => n.GetProperty("anio").GetInt32() == 2026 && n.GetProperty("mes").GetInt32() == 8).Should().Be(1);
        var consultada = await _cliente.GetFromJsonAsync<JsonElement>("/api/nomina/2026/8");
        consultada.GetProperty("detalle").GetArrayLength().Should().Be(18);
    }

    [Fact]
    public async Task Mes_invalido_responde_400()
    {
        var r = await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 13 });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("El mes debe estar entre 1 y 12.");
    }

    [Fact]
    public async Task Nomina_inexistente_responde_404()
    {
        var r = await _cliente.GetAsync("/api/nomina/2026/1");

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Nómina no encontrada");
    }

    [Fact]
    public async Task Consulta_los_parametros_2026()
    {
        var p = await _cliente.GetFromJsonAsync<JsonElement>("/api/nomina/parametros/2026");

        p.GetProperty("salarioMinimo").GetInt64().Should().Be(1_750_905);
        p.GetProperty("auxilioTransporte").GetInt64().Should().Be(249_095);
        p.GetProperty("arl").GetDecimal().Should().Be(0.00522m);
    }

    [Fact]
    public async Task Guarda_los_parametros_de_otro_anio()
    {
        var r = await _cliente.PutAsJsonAsync("/api/nomina/parametros/2027", new
        {
            anio = 1999, salarioMinimo = 1_850_000, auxilioTransporte = 260_000,
            saludEmpleado = 0.04m, pensionEmpleado = 0.04m, fondoSolidaridad = 0.01m, recargoNocturno = 0.35m,
            retencionServicios = 0.10m, saludEmpleador = 0.085m, pensionEmpleador = 0.12m, arl = 0.00522m,
            cajaCompensacion = 0.04m, icbf = 0.03m, sena = 0.02m,
        });

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("Parámetros de nómina guardados");
        var p = await _cliente.GetFromJsonAsync<JsonElement>("/api/nomina/parametros/2027");
        p.GetProperty("anio").GetInt32().Should().Be(2027);   // el año de la ruta manda sobre el del cuerpo
        p.GetProperty("salarioMinimo").GetInt64().Should().Be(1_850_000);
    }

    [Fact]
    public async Task Parametros_invalidos_responden_400()
    {
        var r = await _cliente.PutAsJsonAsync("/api/nomina/parametros/2028", new { salarioMinimo = 0, auxilioTransporte = 0 });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.LeerJsonAsync()).GetProperty("mensaje").GetString().Should().Be("El salario mínimo debe ser mayor que cero.");
    }
}
