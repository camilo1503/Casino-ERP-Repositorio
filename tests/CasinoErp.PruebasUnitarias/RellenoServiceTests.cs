using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>pruebas/features/rellenos.feature</summary>
public class RellenoServiceTests
{
    private readonly MesasEnMemoria _mesas = new();

    private (RellenoService servicio, RellenosEnMemoria repo) Crear(long saldoBoveda)
    {
        var repo = new RellenosEnMemoria(saldoBoveda);
        _mesas.Datos.Add(new Mesa { Numero = "M-04", Juego = "Blackjack", ApuestaMinima = 1, ApuestaMaxima = 2, Estado = EstadoMesa.Abierta });
        _mesas.Datos.Add(new Mesa { Numero = "M-05", Juego = "Póker", ApuestaMinima = 1, ApuestaMaxima = 2, Estado = EstadoMesa.Cerrada });
        return (new RellenoService(repo, new MesaService(_mesas), new RelojFijo()), repo);
    }

    private static Relleno Existe(RellenosEnMemoria repo, int id, long monto, EstadoRelleno estado)
    {
        var relleno = new Relleno { Id = id, MesaNumero = "M-04", Monto = monto, Estado = estado };
        repo.Datos.Add(relleno);
        return relleno;
    }

    [Fact]
    public async Task Aprueba_relleno_dentro_del_saldo_y_descuenta_la_boveda()
    {
        var (servicio, repo) = Crear(10_000_000);
        Existe(repo, 100, 5_000_000, EstadoRelleno.Pendiente);

        var relleno = await servicio.AprobarAsync(100, "Daniela Vargas");

        relleno.Estado.Should().Be(EstadoRelleno.Aprobado);
        relleno.ResueltoPor.Should().Be("Daniela Vargas");
        repo.Boveda.Saldo.Should().Be(5_000_000);
    }

    [Fact]
    public async Task Rechaza_relleno_que_excede_el_saldo()
    {
        var (servicio, repo) = Crear(3_000_000);
        var relleno = Existe(repo, 101, 5_000_000, EstadoRelleno.Pendiente);

        var accion = () => servicio.AprobarAsync(101, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Saldo insuficiente en la bóveda");
        repo.Boveda.Saldo.Should().Be(3_000_000);
        relleno.Estado.Should().Be(EstadoRelleno.Pendiente);
    }

    [Fact]
    public async Task No_vuelve_a_aprobar_un_relleno_aprobado()
    {
        var (servicio, repo) = Crear(10_000_000);
        Existe(repo, 102, 2_000_000, EstadoRelleno.Aprobado);

        var accion = () => servicio.AprobarAsync(102, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El relleno ya está aprobado");
        repo.Boveda.Saldo.Should().Be(10_000_000);
    }

    [Fact]
    public async Task Aprobar_sin_nombre_usa_jefe_de_piso()
    {
        var (servicio, repo) = Crear(10_000_000);
        Existe(repo, 1, 1_000, EstadoRelleno.Pendiente);

        (await servicio.AprobarAsync(1, " ")).ResueltoPor.Should().Be("Jefe de piso");
    }

    [Fact]
    public async Task Solicita_relleno_para_mesa_abierta()
    {
        var (servicio, repo) = Crear(0);

        var relleno = await servicio.SolicitarAsync("M-04", 2_000_000);

        relleno.Estado.Should().Be(EstadoRelleno.Pendiente);
        repo.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Mesa_cerrada_no_pide_relleno()
    {
        var (servicio, _) = Crear(0);

        var accion = () => servicio.SolicitarAsync("M-05", 2_000_000);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Solo las mesas abiertas piden relleno");
    }

    [Fact]
    public async Task Solicitud_con_monto_cero_se_rechaza()
    {
        var (servicio, _) = Crear(0);

        var accion = () => servicio.SolicitarAsync("M-04", 0);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El monto debe ser mayor que cero");
    }

    [Fact]
    public async Task Rechazar_relleno_pendiente_exige_motivo()
    {
        var (servicio, repo) = Crear(0);
        Existe(repo, 7, 1_000, EstadoRelleno.Pendiente);

        await ((Func<Task>)(() => servicio.RechazarAsync(7, "", null)))
            .Should().ThrowAsync<ReglaNegocioException>().WithMessage("El motivo es obligatorio");

        var relleno = await servicio.RechazarAsync(7, "Mesa sin actividad", "Daniela Vargas");
        relleno.Estado.Should().Be(EstadoRelleno.Rechazado);
        relleno.MotivoRechazo.Should().Be("Mesa sin actividad");
    }

    [Fact]
    public async Task Relleno_rechazado_no_se_aprueba()
    {
        var (servicio, repo) = Crear(10_000_000);
        Existe(repo, 8, 1_000, EstadoRelleno.Rechazado);

        var accion = () => servicio.AprobarAsync(8, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Solo se resuelven rellenos pendientes");
    }

    [Fact]
    public async Task Relleno_inexistente_lanza_no_encontrado()
    {
        var (servicio, _) = Crear(0);

        var accion = () => servicio.AprobarAsync(999, null);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task Deposito_aumenta_el_saldo_de_la_boveda()
    {
        var (servicio, repo) = Crear(1_000);

        await servicio.DepositarAsync(4_000);

        repo.Boveda.Saldo.Should().Be(5_000);
        await ((Func<Task>)(() => servicio.DepositarAsync(-1))).Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task Una_mesa_no_tiene_dos_rellenos_pendientes()
    {
        var (servicio, _) = Crear(0);
        var primero = await servicio.SolicitarAsync("M-04", 2_000_000, "Camila Pardo");

        var accion = () => servicio.SolicitarAsync("M-04", 5_000_000);

        primero.SolicitadoPor.Should().Be("Camila Pardo");
        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La mesa ya tiene un relleno pendiente");
    }
}
