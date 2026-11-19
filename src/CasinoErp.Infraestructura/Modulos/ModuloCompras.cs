using CasinoErp.Dominio.Inventario;
using CasinoErp.Infraestructura.Repositorios;
using CasinoErp.Infraestructura.Semillas;
using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.Infraestructura.Modulos;

/// <summary>
/// Registro del módulo Compras (contrato C1). Dueño: Camilo. Ver desarrollo/planes/Plan-Camilo.md (C2 y C5).
/// Aquí se registran sus repositorios, servicios y su ISemilla.
/// </summary>
public static class ModuloCompras
{
    public static IServiceCollection AgregarCompras(this IServiceCollection services)
    {
        // HU-05: alertas del inventario
        services.AddScoped<IAlertaStockRepositorio, AlertaStockRepositorio>();
        services.AddScoped<ISemilla, SemillaUmbralesInventario>();
        return services;
    }
}
