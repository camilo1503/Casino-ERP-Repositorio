using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.Infraestructura.Modulos;

/// <summary>
/// Registro del módulo Personal (contrato C1). Dueño: Felipe. Ver desarrollo/planes/Plan-Felipe.md (F4).
/// Aquí se registran sus repositorios, servicios y su ISemilla.
/// </summary>
public static class ModuloPersonal
{
    public static IServiceCollection AgregarPersonal(this IServiceCollection services) => services;
}
