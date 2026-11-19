using CasinoErp.Dominio.Seguridad;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-06 · Matriz de acceso rol × módulo (Plan-Ivan I1, contrato C5, constante ROLES del mockup).</summary>
public class MatrizAccesoTests
{
    [Fact]
    public void El_crupier_solo_ve_Mi_mesa() =>
        MatrizAcceso.ModulosDe(Rol.Crupier).Should().BeEquivalentTo(["mimesa"]);

    [Fact]
    public void El_cajero_ve_caja_e_inventario() =>
        MatrizAcceso.ModulosDe(Rol.Cajero).Should().BeEquivalentTo(["caja", "inventario"]);

    [Fact]
    public void El_administrador_ve_usuarios_y_reportes() =>
        MatrizAcceso.ModulosDe(Rol.Administrador).Should().BeEquivalentTo(["usuarios", "reportes"]);

    [Fact]
    public void El_gerente_ve_13_modulos_y_no_administra_usuarios()
    {
        MatrizAcceso.ModulosDe(Rol.Gerente).Should().HaveCount(13).And.Contain("proveedores").And.NotContain("usuarios").And.NotContain("mimesa");
    }

    [Fact]
    public void Talento_humano_ve_inicio_competencias_y_rrhh() =>
        MatrizAcceso.ModulosDe(Rol.TalentoHumano).Should().BeEquivalentTo(["inicio", "competencias", "rrhh"]);

    [Theory]
    [InlineData(Rol.Anfitrion, new[] { "inicio", "perfiles", "retencion", "catalogo" })]
    [InlineData(Rol.JefePiso, new[] { "inicio", "catalogo", "mesas", "inventario", "competencias", "proveedores" })]
    [InlineData(Rol.Marketing, new[] { "inicio", "perfiles", "promociones", "retencion", "catalogo" })]
    [InlineData(Rol.Cumplimiento, new[] { "inicio", "cumplimiento", "reportes", "competencias", "caja" })]
    public void La_matriz_es_la_del_mockup(Rol rol, string[] modulos) => MatrizAcceso.ModulosDe(rol).Should().BeEquivalentTo(modulos);

    [Fact]
    public void PuedeAcceder_responde_segun_la_matriz()
    {
        MatrizAcceso.PuedeAcceder(Rol.Cajero, Modulos.Caja).Should().BeTrue();
        MatrizAcceso.PuedeAcceder(Rol.Cajero, Modulos.Rrhh).Should().BeFalse();
    }

    [Fact]
    public void RolesCon_devuelve_quien_tiene_algun_modulo() =>
        MatrizAcceso.RolesCon("rrhh").Should().Equal(Rol.Gerente, Rol.TalentoHumano);

    [Fact]
    public void RolesCon_varios_modulos_une_los_roles() =>
        MatrizAcceso.RolesCon("usuarios", "mimesa").Should().Equal(Rol.Administrador, Rol.Crupier);

    [Fact]
    public void Todos_los_roles_tienen_al_menos_un_modulo()
    {
        foreach (var rol in Enum.GetValues<Rol>())
            MatrizAcceso.ModulosDe(rol).Should().NotBeEmpty($"el rol {rol} debe ver algún módulo");
    }
}
