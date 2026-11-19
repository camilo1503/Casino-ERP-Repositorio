using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Personal;
using CasinoErp.Dominio.Seguridad;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-06 · Gestión de cuentas y asignación de roles (Plan-Ivan I3; mockup FORM.userNew, FORM.userRole, ACT.resetPass, ACT.userOff/On).</summary>
public class UsuarioServiceTests
{
    private const string DocAna = "1010200300";   // crupier
    private const string DocLaura = "1010216138"; // cajera

    private readonly UsuariosEnMemoria _usuarios = new();
    private readonly EmpleadosEnMemoria _empleados = new();
    private readonly UsuarioService _servicio;
    private readonly Usuario _ivan;
    private readonly Usuario _sebastian;

    public UsuarioServiceTests()
    {
        _empleados.AgregarAsync(new Empleado { Documento = DocAna, Nombre = "Ana Torres", Cargo = Cargo.Crupier, Juego = "Blackjack" }).Wait();
        _empleados.AgregarAsync(new Empleado { Documento = DocLaura, Nombre = "Laura Gómez", Cargo = Cargo.Cajero, Juego = "Caja" }).Wait();
        _ivan = Agregar("Iván León", "ivan.leon@pitboss.co", Rol.Administrador);
        _sebastian = Agregar("Sebastián Prada", "sebastian.prada@pitboss.co", Rol.Anfitrion);
        _servicio = new UsuarioService(_usuarios, _empleados, new HasherFalso(), new TotpFalso());
    }

    private Usuario Agregar(string nombre, string correo, Rol rol)
    {
        var u = new Usuario { Nombre = nombre, Correo = correo, Rol = rol, HashContrasena = "H:Casino2026", SecretoTotp = "SECRETO-" + nombre };
        _usuarios.AgregarAsync(u).Wait();
        return u;
    }

    [Fact]
    public async Task Crea_usuario_con_contrasena_temporal_y_secreto_totp()
    {
        var r = await _servicio.CrearAsync("Marta Gil", "  Marta.Gil@PitBoss.co ", Rol.Cajero, null);
        r.Usuario.Correo.Should().Be("marta.gil@pitboss.co");
        r.Usuario.DebeCambiarContrasena.Should().BeTrue();
        ContrasenaTemporal.EsSegura(r.ContrasenaTemporal).Should().BeTrue();
        r.Usuario.HashContrasena.Should().Be("H:" + r.ContrasenaTemporal);
        r.UriTotp.Should().StartWith("otpauth://totp/PitBoss:marta.gil@pitboss.co?secret=");

        r.Usuario.Nombre.Should().Be("Marta Gil");
        r.Usuario.Rol.Should().Be(Rol.Cajero);
        r.Usuario.Activo.Should().BeTrue();
        r.Usuario.SecretoTotp.Should().NotBeNullOrEmpty();
        _usuarios.Datos.Should().Contain(r.Usuario);
    }

    [Fact]
    public async Task Exige_nombre()
    {
        var accion = () => _servicio.CrearAsync("  ", "marta.gil@pitboss.co", Rol.Cajero, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Escribe el nombre.");
    }

    [Fact]
    public async Task Rechaza_correo_invalido()
    {
        var accion = () => _servicio.CrearAsync("Marta Gil", "marta@", Rol.Cajero, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El correo no es válido.");
    }

    [Fact]
    public async Task Rechaza_correo_duplicado_sin_importar_mayusculas()
    {
        var accion = () => _servicio.CrearAsync("Otro Iván", "IVAN.LEON@pitboss.co", Rol.Cajero, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Ya existe una cuenta con ese correo.");
    }

    [Fact]
    public async Task Crupier_debe_vincular_un_empleado()
    {
        var accion = () => _servicio.CrearAsync("Ana Torres", "ana.torres@pitboss.co", Rol.Crupier, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>()
            .WithMessage("Vincula el usuario de crupier a un empleado para asignarle su mesa.");
    }

    [Fact]
    public async Task Crupier_vinculado_a_un_cajero_se_rechaza()
    {
        var accion = () => _servicio.CrearAsync("Laura Gómez", "laura.gomez@pitboss.co", Rol.Crupier, DocLaura);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El empleado vinculado no es crupier.");
    }

    [Fact]
    public async Task Empleado_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.CrearAsync("Nadie", "nadie@pitboss.co", Rol.Crupier, "999999");

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Empleado no encontrado");
    }

    [Fact]
    public async Task Crupier_vinculado_correctamente()
    {
        var r = await _servicio.CrearAsync("Ana Torres", "ana.torres@pitboss.co", Rol.Crupier, DocAna);

        r.Usuario.EmpleadoDocumento.Should().Be(DocAna);
        r.Usuario.Rol.Should().Be(Rol.Crupier);
    }

    [Fact]
    public async Task Cambia_el_rol()
    {
        var u = await _servicio.CambiarRolAsync(_sebastian.Id, Rol.Cajero);

        u.Rol.Should().Be(Rol.Cajero);
        _sebastian.Rol.Should().Be(Rol.Cajero);
    }

    [Fact]
    public async Task No_quita_el_rol_al_ultimo_administrador()
    {
        var accion = () => _servicio.CambiarRolAsync(_ivan.Id, Rol.Gerente);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No puedes quitar el rol al último administrador.");
        _ivan.Rol.Should().Be(Rol.Administrador);
    }

    [Fact]
    public async Task Si_hay_dos_administradores_se_puede_cambiar_uno()
    {
        Agregar("Otra Admin", "otra.admin@pitboss.co", Rol.Administrador);

        var u = await _servicio.CambiarRolAsync(_ivan.Id, Rol.Gerente);

        u.Rol.Should().Be(Rol.Gerente);
    }

    [Fact]
    public async Task Un_administrador_inactivo_no_cuenta_como_otro_administrador()
    {
        var inactiva = Agregar("Admin Inactiva", "admin.inactiva@pitboss.co", Rol.Administrador);
        inactiva.Activo = false;

        var accion = () => _servicio.CambiarRolAsync(_ivan.Id, Rol.Gerente);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No puedes quitar el rol al último administrador.");
    }

    [Fact]
    public async Task Cambiar_a_crupier_exige_un_empleado_crupier()
    {
        var sinEmpleado = () => _servicio.CambiarRolAsync(_sebastian.Id, Rol.Crupier);
        await sinEmpleado.Should().ThrowAsync<ReglaNegocioException>()
            .WithMessage("Vincula el usuario de crupier a un empleado para asignarle su mesa.");

        _sebastian.EmpleadoDocumento = DocLaura;
        var conCajero = () => _servicio.CambiarRolAsync(_sebastian.Id, Rol.Crupier);
        await conCajero.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El empleado vinculado no es crupier.");

        _sebastian.EmpleadoDocumento = DocAna;
        (await _servicio.CambiarRolAsync(_sebastian.Id, Rol.Crupier)).Rol.Should().Be(Rol.Crupier);
    }

    [Fact]
    public async Task No_desactiva_al_ultimo_administrador()
    {
        var accion = () => _servicio.DesactivarAsync(_ivan.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("No puedes desactivar al último administrador.");
        _ivan.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Desactiva_y_reactiva()
    {
        (await _servicio.DesactivarAsync(_sebastian.Id)).Activo.Should().BeFalse();
        var otraVez = () => _servicio.DesactivarAsync(_sebastian.Id);
        await otraVez.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El usuario ya está inactivo.");

        (await _servicio.ReactivarAsync(_sebastian.Id)).Activo.Should().BeTrue();
        var yaActivo = () => _servicio.ReactivarAsync(_sebastian.Id);
        await yaActivo.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El usuario ya está activo.");
    }

    [Fact]
    public async Task Restablecer_levanta_el_bloqueo_y_conserva_el_totp()
    {
        _sebastian.IntentosFallidos = 3;
        _sebastian.BloqueadoHasta = new DateTime(2026, 10, 26, 15, 15, 0, DateTimeKind.Utc);
        var secreto = _sebastian.SecretoTotp;

        var r = await _servicio.RestablecerContrasenaAsync(_sebastian.Id);

        r.Usuario.Should().BeSameAs(_sebastian);
        ContrasenaTemporal.EsSegura(r.ContrasenaTemporal).Should().BeTrue();
        _sebastian.HashContrasena.Should().Be("H:" + r.ContrasenaTemporal);
        _sebastian.DebeCambiarContrasena.Should().BeTrue();
        _sebastian.IntentosFallidos.Should().Be(0);
        _sebastian.BloqueadoHasta.Should().BeNull();
        _sebastian.SecretoTotp.Should().Be(secreto);
    }

    [Fact]
    public async Task Cambiar_contrasena_exige_la_actual()
    {
        var accion = () => _servicio.CambiarContrasenaAsync(_sebastian.Id, "Otra2026!", "Nueva#2026x");

        await accion.Should().ThrowAsync<AutenticacionException>().WithMessage("La contraseña actual no es correcta.");
    }

    [Fact]
    public async Task Cambiar_contrasena_valida_la_fortaleza()
    {
        var accion = () => _servicio.CambiarContrasenaAsync(_sebastian.Id, "Casino2026", "debil");

        await accion.Should().ThrowAsync<ReglaNegocioException>()
            .WithMessage("La contraseña necesita 8 caracteres, una mayúscula, un número y un carácter especial.");
        _sebastian.HashContrasena.Should().Be("H:Casino2026");
    }

    [Fact]
    public async Task Cambiar_contrasena_quita_la_marca_de_temporal()
    {
        _sebastian.DebeCambiarContrasena = true;

        await _servicio.CambiarContrasenaAsync(_sebastian.Id, "Casino2026", "Nueva#2026x");

        _sebastian.HashContrasena.Should().Be("H:Nueva#2026x");
        _sebastian.DebeCambiarContrasena.Should().BeFalse();
    }

    [Fact]
    public async Task Usuario_inexistente_lanza_no_encontrado()
    {
        var acciones = new Func<Task>[]
        {
            () => _servicio.ConsultarAsync(99),
            () => _servicio.CambiarRolAsync(99, Rol.Cajero),
            () => _servicio.RestablecerContrasenaAsync(99),
            () => _servicio.DesactivarAsync(99),
            () => _servicio.ReactivarAsync(99),
            () => _servicio.CambiarContrasenaAsync(99, "Casino2026", "Nueva#2026x"),
        };

        foreach (var accion in acciones)
            await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Usuario no encontrado");
    }

    [Fact]
    public async Task Consulta_y_lista_por_id()
    {
        (await _servicio.ConsultarAsync(_sebastian.Id)).Should().BeSameAs(_sebastian);
        (await _servicio.ListarAsync()).Select(u => u.Id).Should().Equal(_ivan.Id, _sebastian.Id);
    }
}
