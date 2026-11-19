using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.Infraestructura.Modulos;

/// <summary>
/// Registro del módulo Nomina (contrato C1). Dueño: Felipe. Ver desarrollo/planes/Plan-Felipe.md (F5).
/// Aquí se registran sus repositorios, servicios y su ISemilla.
/// </summary>
public static class ModuloNomina
{
    public static IServiceCollection AgregarNomina(this IServiceCollection services) => services;
}
