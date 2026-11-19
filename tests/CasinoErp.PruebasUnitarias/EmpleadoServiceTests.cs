using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Personal;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-04 · CRUD de empleados (Plan-Felipe F1, mockup HU-PER-01).</summary>
public class EmpleadoServiceTests
{
    private readonly EmpleadosEnMemoria _repo = new();
    private readonly EmpleadoService _servicio;

    public EmpleadoServiceTests() => _servicio = new EmpleadoService(_repo, new RelojFijo());

    private Task<Empleado> RegistrarAna() => _servicio.RegistrarAsync("1010200300", "Ana Torres", Cargo.Crupier, "Blackjack");

    [Fact]
    public async Task Registra_un_crupier_con_el_salario_de_su_cargo()
    {
        var e = await _servicio.RegistrarAsync("1.010.200.300", "Ana Torres", Cargo.Crupier, "Blackjack");

        e.Documento.Should().Be("1010200300");
        e.SalarioBase.Should().Be(2_600_000);
        e.Juego.Should().Be("Blackjack");
        e.Turno.Should().Be(Turno.Tarde);
        e.Area.Should().Be("Sin asignar");
        e.TipoContrato.Should().Be(TipoContrato.Indefinido);
        e.FechaIngreso.Should().Be(new DateOnly(2026, 10, 26));
        e.Activo.Should().BeTrue();
        _repo.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task El_cajero_queda_con_juego_Caja_y_su_salario()
    {
        var e = await _servicio.RegistrarAsync("1010216138", "Laura Gómez", Cargo.Cajero, null);

        e.Juego.Should().Be("Caja");
        e.SalarioBase.Should().Be(2_300_000);
    }

    [Fact]
    public async Task El_jefe_de_piso_queda_en_Supervision()
    {
        var e = await _servicio.RegistrarAsync("1010327004", "Daniela Vargas", Cargo.JefeDePiso, null);

        e.Juego.Should().Be("Supervisión");
        e.SalarioBase.Should().Be(4_200_000);
    }

    [Fact]
    public async Task Respeta_el_salario_y_contrato_recibidos()
    {
        var e = await _servicio.RegistrarAsync("1010200300", "Ana Torres", Cargo.Crupier, "Ruleta",
            TipoContrato.PrestacionServicios, 3_000_000);

        e.TipoContrato.Should().Be(TipoContrato.PrestacionServicios);
        e.SalarioBase.Should().Be(3_000_000);
    }

    [Fact]
    public async Task Normaliza_el_nombre()
    {
        var e = await _servicio.RegistrarAsync("1010200300", "  Ana Torres ", Cargo.Crupier, "Blackjack");

        e.Nombre.Should().Be("Ana Torres");
    }

    [Theory]
    [InlineData("", "Ana Torres")]
    [InlineData("1010200300", "  ")]
    [InlineData("...", "Ana Torres")]
    public async Task Exige_nombre_y_documento(string documento, string nombre)
    {
        var accion = () => _servicio.RegistrarAsync(documento, nombre, Cargo.Crupier, "Blackjack");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Nombre y documento son obligatorios.");
        _repo.Datos.Should().BeEmpty();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901")]
    public async Task Rechaza_documento_de_longitud_invalida(string documento)
    {
        var accion = () => _servicio.RegistrarAsync(documento, "Ana Torres", Cargo.Crupier, "Blackjack");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El documento no es válido.");
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("1234567890")]
    public async Task Acepta_documentos_de_6_a_10_digitos(string documento)
    {
        var e = await _servicio.RegistrarAsync(documento, "Ana Torres", Cargo.Crupier, "Blackjack");

        e.Documento.Should().Be(documento);
    }

    [Fact]
    public async Task Rechaza_documento_duplicado_aunque_venga_con_puntos()
    {
        await RegistrarAna();

        var accion = () => _servicio.RegistrarAsync("1.010.200.300", "Otra Persona", Cargo.Cajero, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Ya existe un empleado con ese documento.");
        _repo.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Rechaza_crupier_con_juego_invalido()
    {
        var accion = () => _servicio.RegistrarAsync("1010200300", "Ana Torres", Cargo.Crupier, "Dominó");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El juego del crupier no es válido.");
    }

    [Fact]
    public async Task Rechaza_crupier_sin_juego()
    {
        var accion = () => _servicio.RegistrarAsync("1010200300", "Ana Torres", Cargo.Crupier, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El juego del crupier no es válido.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Rechaza_salario_no_positivo(long salario)
    {
        var accion = () => _servicio.RegistrarAsync("1010200300", "Ana Torres", Cargo.Crupier, "Blackjack",
            TipoContrato.Indefinido, salario);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El salario base debe ser mayor que cero.");
    }

    [Fact]
    public async Task Actualiza_turno_area_contrato_y_salario()
    {
        await RegistrarAna();
        var guardadosAntes = _repo.Guardados;

        var e = await _servicio.ActualizarAsync("1010200300", Turno.Noche, "M-01", TipoContrato.TerminoFijo, 2_700_000);

        e.Turno.Should().Be(Turno.Noche);
        e.Area.Should().Be("M-01");
        e.TipoContrato.Should().Be(TipoContrato.TerminoFijo);
        e.SalarioBase.Should().Be(2_700_000);
        _repo.Guardados.Should().BeGreaterThan(guardadosAntes);
    }

    [Fact]
    public async Task Actualizar_exige_area()
    {
        await RegistrarAna();

        var accion = () => _servicio.ActualizarAsync("1010200300", Turno.Noche, " ", TipoContrato.Indefinido, 2_600_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El área es obligatoria.");
    }

    [Fact]
    public async Task Actualizar_exige_salario_positivo()
    {
        await RegistrarAna();

        var accion = () => _servicio.ActualizarAsync("1010200300", Turno.Noche, "M-01", TipoContrato.Indefinido, 0);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El salario base debe ser mayor que cero.");
    }

    [Fact]
    public async Task No_actualiza_un_retirado()
    {
        await RegistrarAna();
        await _servicio.RetirarAsync("1010200300", "Renuncia");

        var accion = () => _servicio.ActualizarAsync("1010200300", Turno.Noche, "M-01", TipoContrato.Indefinido, 2_600_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El empleado está retirado.");
    }

    [Fact]
    public async Task Retira_con_motivo_y_conserva_el_registro()
    {
        await RegistrarAna();

        var e = await _servicio.RetirarAsync("1.010.200.300", " Renuncia ");

        e.Activo.Should().BeFalse();
        e.MotivoRetiro.Should().Be("Renuncia");
        (await _servicio.ListarAsync()).Should().ContainSingle(x => x.Documento == "1010200300");
    }

    [Fact]
    public async Task Retirar_exige_motivo()
    {
        await RegistrarAna();

        var accion = () => _servicio.RetirarAsync("1010200300", "");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El motivo es obligatorio.");
        _repo.Datos[0].Activo.Should().BeTrue();
    }

    [Fact]
    public async Task No_retira_dos_veces()
    {
        await RegistrarAna();
        await _servicio.RetirarAsync("1010200300", "Renuncia");

        var accion = () => _servicio.RetirarAsync("1010200300", "Otra vez");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El empleado ya está retirado.");
    }

    [Fact]
    public async Task Consulta_con_el_documento_formateado()
    {
        await RegistrarAna();

        var e = await _servicio.ConsultarAsync("1.010.200.300");

        e.Nombre.Should().Be("Ana Torres");
    }

    [Fact]
    public async Task Consultar_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.ConsultarAsync("999999");

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Empleado no encontrado");
    }

    [Fact]
    public async Task Listar_solo_activos_excluye_retirados()
    {
        await RegistrarAna();
        await _servicio.RegistrarAsync("1010216138", "Laura Gómez", Cargo.Cajero, null);
        await _servicio.RetirarAsync("1010200300", "Renuncia");

        var activos = await _servicio.ListarAsync(soloActivos: true);

        activos.Should().ContainSingle().Which.Nombre.Should().Be("Laura Gómez");
        (await _servicio.ListarAsync()).Should().HaveCount(2);
    }

    [Fact]
    public void Salario_por_cargo_y_juegos_de_crupier()
    {
        EmpleadoService.SalarioPorCargo.Should().Equal(new Dictionary<Cargo, long>
        {
            [Cargo.Crupier] = 2_600_000,
            [Cargo.Cajero] = 2_300_000,
            [Cargo.JefeDePiso] = 4_200_000,
        });
        EmpleadoService.JuegosCrupier.Should().Equal("Blackjack", "Ruleta", "Póker", "Baccarat", "Craps");
    }
}
