using System.Text.RegularExpressions;
using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Personal;

namespace CasinoErp.Dominio.Seguridad;

public record UsuarioCreado(Usuario Usuario, string ContrasenaTemporal, string UriTotp);
public record ContrasenaRestablecida(Usuario Usuario, string ContrasenaTemporal);

/// <summary>Gestión de cuentas y asignación de roles (HU-06; mockup FORM.userNew, FORM.userRole, ACT.resetPass, ACT.userOff/On).</summary>
public partial class UsuarioService(IUsuarioRepositorio usuarios, IEmpleadoRepositorio empleados, IHasherContrasena hasher, IVerificadorTotp totp)
{
    public const string MensajeCreado = "Usuario creado", MensajeRol = "Rol actualizado", MensajeRestablecida = "Contraseña restablecida",
                        MensajeDesactivado = "Usuario desactivado", MensajeReactivado = "Usuario reactivado",
                        MensajeContrasena = "Contraseña actualizada";

    private const string MensajeVincular = "Vincula el usuario de crupier a un empleado para asignarle su mesa.";
    private const string MensajeNoCrupier = "El empleado vinculado no es crupier.";

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex CorreoValido();

    public async Task<UsuarioCreado> CrearAsync(string nombre, string correo, Rol rol, string? empleadoDocumento)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("Escribe el nombre.");
        correo = (correo ?? "").Trim().ToLowerInvariant();
        if (!CorreoValido().IsMatch(correo)) throw new ReglaNegocioException("El correo no es válido.");
        if (await usuarios.ExisteCorreoAsync(correo)) throw new ReglaNegocioException("Ya existe una cuenta con ese correo.");
        if (rol == Rol.Crupier && string.IsNullOrWhiteSpace(empleadoDocumento)) throw new ReglaNegocioException(MensajeVincular);
        if (!string.IsNullOrWhiteSpace(empleadoDocumento))
        {
            var empleado = await empleados.ObtenerPorDocumentoAsync(empleadoDocumento)
                ?? throw new NoEncontradoException("Empleado no encontrado");
            if (rol == Rol.Crupier && empleado.Cargo != Cargo.Crupier) throw new ReglaNegocioException(MensajeNoCrupier);
        }

        var temporal = ContrasenaTemporal.Generar();
        var secreto = totp.NuevoSecreto();
        var usuario = new Usuario
        {
            Nombre = nombre.Trim(),
            Correo = correo,
            Rol = rol,
            HashContrasena = hasher.Hash(temporal),
            SecretoTotp = secreto,
            DebeCambiarContrasena = true,
            EmpleadoDocumento = string.IsNullOrWhiteSpace(empleadoDocumento) ? null : empleadoDocumento,
        };
        await usuarios.AgregarAsync(usuario);
        return new UsuarioCreado(usuario, temporal, totp.UriConfiguracion(secreto, correo));
    }

    public async Task<Usuario> CambiarRolAsync(int id, Rol rol)
    {
        var u = await ConsultarAsync(id);
        if (rol != Rol.Administrador && await EsUnicoAdministradorActivoAsync(u))
            throw new ReglaNegocioException("No puedes quitar el rol al último administrador.");
        if (rol == Rol.Crupier)
        {
            if (string.IsNullOrWhiteSpace(u.EmpleadoDocumento)) throw new ReglaNegocioException(MensajeVincular);
            var empleado = await empleados.ObtenerPorDocumentoAsync(u.EmpleadoDocumento)
                ?? throw new NoEncontradoException("Empleado no encontrado");
            if (empleado.Cargo != Cargo.Crupier) throw new ReglaNegocioException(MensajeNoCrupier);
        }

        u.Rol = rol;
        await usuarios.GuardarCambiosAsync();
        return u;
    }

    public async Task<ContrasenaRestablecida> RestablecerContrasenaAsync(int id)
    {
        var u = await ConsultarAsync(id);
        var temporal = ContrasenaTemporal.Generar();
        u.HashContrasena = hasher.Hash(temporal);
        u.DebeCambiarContrasena = true;
        u.IntentosFallidos = 0;
        u.BloqueadoHasta = null;
        await usuarios.GuardarCambiosAsync();
        return new ContrasenaRestablecida(u, temporal);
    }

    public async Task<Usuario> DesactivarAsync(int id)
    {
        var u = await ConsultarAsync(id);
        if (!u.Activo) throw new ReglaNegocioException("El usuario ya está inactivo.");
        if (await EsUnicoAdministradorActivoAsync(u)) throw new ReglaNegocioException("No puedes desactivar al último administrador.");
        u.Activo = false;
        await usuarios.GuardarCambiosAsync();
        return u;
    }

    public async Task<Usuario> ReactivarAsync(int id)
    {
        var u = await ConsultarAsync(id);
        if (u.Activo) throw new ReglaNegocioException("El usuario ya está activo.");
        u.Activo = true;
        await usuarios.GuardarCambiosAsync();
        return u;
    }

    public async Task<Usuario> ConsultarAsync(int id) =>
        await usuarios.ObtenerAsync(id) ?? throw new NoEncontradoException("Usuario no encontrado");

    public Task<IReadOnlyList<Usuario>> ListarAsync() => usuarios.ListarAsync();

    public async Task CambiarContrasenaAsync(int id, string actual, string nueva)
    {
        var u = await ConsultarAsync(id);
        if (!hasher.Verificar(u.HashContrasena, actual)) throw new AutenticacionException("La contraseña actual no es correcta.");
        if (!ContrasenaTemporal.EsSegura(nueva))
            throw new ReglaNegocioException("La contraseña necesita 8 caracteres, una mayúscula, un número y un carácter especial.");
        u.HashContrasena = hasher.Hash(nueva);
        u.DebeCambiarContrasena = false;
        await usuarios.GuardarCambiosAsync();
    }

    private async Task<bool> EsUnicoAdministradorActivoAsync(Usuario u)
    {
        if (u.Rol != Rol.Administrador || !u.Activo) return false;
        var todos = await usuarios.ListarAsync();
        return !todos.Any(o => o.Id != u.Id && o.Rol == Rol.Administrador && o.Activo);
    }
}
