using CasinoErp.Dominio.Seguridad;
using Microsoft.AspNetCore.Authorization;

namespace CasinoErp.Api.Seguridad;

/// <summary>
/// Exige un token válido de un rol que tenga al menos uno de los módulos (contrato C5).
/// Se traduce a <c>Roles</c> con <see cref="MatrizAcceso.RolesCon"/>: sin token → 401; rol sin el módulo → 403.
/// </summary>
public sealed class AutorizarModuloAttribute : AuthorizeAttribute
{
    public AutorizarModuloAttribute(params string[] modulos)
    {
        Roles = string.Join(",", MatrizAcceso.RolesCon(modulos));
    }
}
