namespace CasinoErp.Dominio.Seguridad;

/// <summary>Roles del ERP (contrato C5). El orden importa: <see cref="MatrizAcceso.RolesCon"/> devuelve los roles en este orden.</summary>
public enum Rol { Gerente, Administrador, Anfitrion, JefePiso, Crupier, Marketing, Cajero, TalentoHumano, Cumplimiento }

/// <summary>Claves de los módulos del ERP, iguales a las vistas del mockup (contrato C5).</summary>
public static class Modulos
{
    public const string Inicio = "inicio", Perfiles = "perfiles", Promociones = "promociones", Retencion = "retencion",
        Catalogo = "catalogo", MiMesa = "mimesa", Mesas = "mesas", Caja = "caja", Inventario = "inventario",
        Competencias = "competencias", Rrhh = "rrhh", Cumplimiento = "cumplimiento", Reportes = "reportes",
        Usuarios = "usuarios", Proveedores = "proveedores";
}

/// <summary>
/// Matriz rol × módulo (contrato C5): la constante <c>ROLES</c> del mockup, sin <c>campana</c>
/// (vive dentro de promociones), más <c>proveedores</c> para Gerente y JefePiso.
/// </summary>
public static class MatrizAcceso
{
    private static readonly Dictionary<Rol, HashSet<string>> Matriz = new()
    {
        [Rol.Gerente] = Conjunto(Modulos.Inicio, Modulos.Perfiles, Modulos.Promociones, Modulos.Retencion, Modulos.Catalogo,
            Modulos.Mesas, Modulos.Caja, Modulos.Inventario, Modulos.Competencias, Modulos.Rrhh, Modulos.Cumplimiento,
            Modulos.Reportes, Modulos.Proveedores),
        [Rol.Administrador] = Conjunto(Modulos.Usuarios, Modulos.Reportes),
        [Rol.Anfitrion] = Conjunto(Modulos.Inicio, Modulos.Perfiles, Modulos.Retencion, Modulos.Catalogo),
        [Rol.JefePiso] = Conjunto(Modulos.Inicio, Modulos.Catalogo, Modulos.Mesas, Modulos.Inventario, Modulos.Competencias,
            Modulos.Proveedores),
        [Rol.Crupier] = Conjunto(Modulos.MiMesa),
        [Rol.Marketing] = Conjunto(Modulos.Inicio, Modulos.Perfiles, Modulos.Promociones, Modulos.Retencion, Modulos.Catalogo),
        [Rol.Cajero] = Conjunto(Modulos.Caja, Modulos.Inventario),
        [Rol.TalentoHumano] = Conjunto(Modulos.Inicio, Modulos.Competencias, Modulos.Rrhh),
        [Rol.Cumplimiento] = Conjunto(Modulos.Inicio, Modulos.Cumplimiento, Modulos.Reportes, Modulos.Competencias, Modulos.Caja),
    };

    private static HashSet<string> Conjunto(params string[] modulos) => new(modulos, StringComparer.Ordinal);

    /// <summary>Módulos que puede ver el rol.</summary>
    public static IReadOnlySet<string> ModulosDe(Rol rol) => Matriz[rol];

    /// <summary>Indica si el rol tiene el módulo.</summary>
    public static bool PuedeAcceder(Rol rol, string modulo) => Matriz[rol].Contains(modulo);

    /// <summary>Roles que tienen al menos uno de los módulos, en el orden del enum.</summary>
    public static IReadOnlyList<Rol> RolesCon(params string[] modulos) =>
        Enum.GetValues<Rol>().Where(r => modulos.Any(m => Matriz[r].Contains(m))).ToList();
}
