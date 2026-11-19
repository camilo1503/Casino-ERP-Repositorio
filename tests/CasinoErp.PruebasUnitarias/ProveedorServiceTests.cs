using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Compras;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-05 · CRUD de proveedores de fichas (Plan-Camilo C3).</summary>
public class ProveedorServiceTests
{
    private const string Nit = "901234567-1";
    private readonly ProveedoresEnMemoria _repo = new();
    private readonly ProveedorService _servicio;

    public ProveedorServiceTests() => _servicio = new ProveedorService(_repo);

    private Task<Proveedor> RegistrarAndinas() =>
        _servicio.RegistrarAsync(Nit, "Fichas Andinas S.A.S.", "VENTAS@FICHASANDINAS.CO", "+57 601 555 0101");

    [Fact]
    public async Task Registra_un_proveedor()
    {
        var p = await RegistrarAndinas();

        p.Nit.Should().Be(Nit);
        p.Nombre.Should().Be("Fichas Andinas S.A.S.");
        p.Correo.Should().Be("ventas@fichasandinas.co");
        p.Telefono.Should().Be("+57 601 555 0101");
        p.Activo.Should().BeTrue();
        _repo.Datos.Should().ContainSingle();
    }

    [Theory, InlineData("90123456-1"), InlineData("901234567"), InlineData("901234567-12"), InlineData("ABC")]
    public async Task Rechaza_nit_invalido(string nit)
    {
        var accion = () => _servicio.RegistrarAsync(nit, "X", "x@x.co", "1");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El NIT no es válido.");
    }

    [Fact]
    public async Task Rechaza_nit_duplicado()
    {
        await RegistrarAndinas();

        var accion = () => RegistrarAndinas();

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Ya existe un proveedor con ese NIT.");
    }

    [Fact]
    public async Task Rechaza_nombre_vacio()
    {
        var accion = () => _servicio.RegistrarAsync(Nit, "  ", "x@x.co", "1");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El nombre es obligatorio.");
    }

    [Fact]
    public async Task Rechaza_correo_invalido()
    {
        var accion = () => _servicio.RegistrarAsync(Nit, "X", "ventas@", "1");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El correo no es válido.");
    }

    [Fact]
    public async Task Rechaza_telefono_vacio()
    {
        var accion = () => _servicio.RegistrarAsync(Nit, "X", "x@x.co", "");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El teléfono es obligatorio.");
    }

    [Fact]
    public async Task Actualiza_nombre_correo_y_telefono()
    {
        await RegistrarAndinas();

        var p = await _servicio.ActualizarAsync(Nit, "Fichas Andinas", "Compras@FichasAndinas.co", "+57 601 555 0199");

        p.Nombre.Should().Be("Fichas Andinas");
        p.Correo.Should().Be("compras@fichasandinas.co");
        p.Telefono.Should().Be("+57 601 555 0199");
    }

    [Fact]
    public async Task Actualizar_valida_los_datos()
    {
        await RegistrarAndinas();

        var accion = () => _servicio.ActualizarAsync(Nit, "Fichas Andinas", "sin-arroba", "1");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El correo no es válido.");
    }

    [Fact]
    public async Task Desactiva_sin_borrar()
    {
        await RegistrarAndinas();

        var p = await _servicio.DesactivarAsync(Nit);

        p.Activo.Should().BeFalse();
        (await _servicio.ListarAsync()).Should().ContainSingle();
        (await _servicio.ListarAsync(soloActivos: true)).Should().BeEmpty();
    }

    [Fact]
    public async Task No_desactiva_dos_veces()
    {
        await RegistrarAndinas();
        await _servicio.DesactivarAsync(Nit);

        var accion = () => _servicio.DesactivarAsync(Nit);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El proveedor ya está inactivo.");
    }

    [Fact]
    public async Task Reactiva()
    {
        await RegistrarAndinas();
        await _servicio.DesactivarAsync(Nit);

        var p = await _servicio.ReactivarAsync(Nit);

        p.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task No_reactiva_uno_activo()
    {
        await RegistrarAndinas();

        var accion = () => _servicio.ReactivarAsync(Nit);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El proveedor ya está activo.");
    }

    [Fact]
    public async Task Consultar_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.ConsultarAsync("999999999-9");

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Proveedor no encontrado");
    }

    [Fact]
    public async Task El_nit_se_recorta_antes_de_validar()
    {
        var p = await _servicio.RegistrarAsync("  901234567-1 ", "X", "x@x.co", "1");

        p.Nit.Should().Be(Nit);
        (await _servicio.ConsultarAsync(" 901234567-1")).Nombre.Should().Be("X");
    }
}
