using CasinoErp.Dominio.Seguridad;

namespace CasinoErp.Infraestructura.Semillas;

/// <summary>
/// Las 11 cuentas de demostración (contrato C6): nombres de seed().users del mockup, contraseña Casino2026
/// y un secreto TOTP nuevo por cuenta. En Development y pruebas también sirve el código demo 482913.
/// </summary>
public class SemillaSeguridad(IHasherContrasena hasher, IVerificadorTotp totp) : ISemilla
{
    public const string ContrasenaDemo = "Casino2026";

    public int Orden => 40;

    private static readonly (string Nombre, string Correo, Rol Rol, bool Activo, string? Empleado)[] Cuentas =
    [
        ("Daniel Ruiz", "daniel.ruiz@pitboss.co", Rol.Gerente, true, null),
        ("Iván León", "ivan.leon@pitboss.co", Rol.Administrador, true, null),
        ("Sebastián Prada", "sebastian.prada@pitboss.co", Rol.Anfitrion, true, null),
        ("Daniela Vargas", "daniela.vargas@pitboss.co", Rol.JefePiso, true, "1010327004"),
        ("Ana Torres", "ana.torres@pitboss.co", Rol.Crupier, true, "1010200300"),
        ("Jorge Díaz", "jorge.diaz@pitboss.co", Rol.Marketing, true, null),
        ("Laura Gómez", "laura.gomez@pitboss.co", Rol.Cajero, true, "1010216138"),
        ("Felipe Cortés", "felipe.cortes@pitboss.co", Rol.TalentoHumano, true, null),
        ("Andrea Méndez", "andrea.mendez@pitboss.co", Rol.Cumplimiento, true, null),
        ("Miguel Á. Soto", "miguel.soto@pitboss.co", Rol.Crupier, true, "1010224057"),
        ("Carolina Ríos", "carolina.rios@pitboss.co", Rol.Cajero, false, null),
    ];

    public void Cargar(CasinoErpDbContext db)
    {
        if (db.Set<Usuario>().Any()) return;

        db.Set<Usuario>().AddRange(Cuentas.Select(c => new Usuario
        {
            Nombre = c.Nombre,
            Correo = c.Correo,
            Rol = c.Rol,
            Activo = c.Activo,
            HashContrasena = hasher.Hash(ContrasenaDemo),
            SecretoTotp = totp.NuevoSecreto(),
            DebeCambiarContrasena = false,
            EmpleadoDocumento = c.Empleado,
        }));
        db.SaveChanges();
    }
}
