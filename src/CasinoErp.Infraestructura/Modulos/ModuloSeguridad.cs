using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.Infraestructura.Modulos;

/// <summary>
/// Registro del módulo Seguridad (contrato C1). Dueño: Iván. Ver desarrollo/planes/Plan-Ivan.md (I4).
/// Aquí se registran sus repositorios, servicios y su ISemilla.
/// </summary>
public static class ModuloSeguridad
{
    public static IServiceCollection AgregarSeguridad(this IServiceCollection services) => services;
}
