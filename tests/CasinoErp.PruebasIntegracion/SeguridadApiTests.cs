using CasinoErp.Dominio.Seguridad;
using CasinoErp.Infraestructura;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.PruebasIntegracion;

/// <summary>HU-06 · Seguridad contra una base SQLite real con las cuentas de demostración (Plan-Ivan I4 e I5).</summary>
public class SeguridadApiTests : IClassFixture<CasinoErpFactory>
{
    private readonly CasinoErpFactory _factory;

    public SeguridadApiTests(CasinoErpFactory factory)
    {
        _factory = factory;
        _factory.CreateClient().Dispose();   // arranca el host para que corra PrepararBaseDatos
    }

    [Fact]
    public void La_semilla_crea_las_11_cuentas_de_demo()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasinoErpDbContext>();
        var usuarios = db.Set<Usuario>().ToList();
        usuarios.Should().HaveCount(11);
        usuarios.Single(u => u.Correo == "carolina.rios@pitboss.co").Activo.Should().BeFalse();
        usuarios.Single(u => u.Correo == "ana.torres@pitboss.co").EmpleadoDocumento.Should().Be("1010200300");
        var hasher = scope.ServiceProvider.GetRequiredService<IHasherContrasena>();
        hasher.Verificar(usuarios[0].HashContrasena, "Casino2026").Should().BeTrue();
    }

    [Fact]
    public void El_verificador_acepta_un_codigo_TOTP_real_y_rechaza_uno_errado()
    {
        using var scope = _factory.Services.CreateScope();
        var totp = scope.ServiceProvider.GetRequiredService<IVerificadorTotp>();
        var secreto = totp.NuevoSecreto();
        var codigo = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(secreto)).ComputeTotp();
        var errado = codigo == "000000" ? "111111" : "000000";

        totp.Verificar(secreto, codigo).Should().BeTrue();
        totp.Verificar(secreto, errado).Should().BeFalse();
        totp.UriConfiguracion(secreto, "marta.gil@pitboss.co")
            .Should().Be($"otpauth://totp/PitBoss:marta.gil%40pitboss.co?secret={secreto}&issuer=PitBoss");
    }
}
