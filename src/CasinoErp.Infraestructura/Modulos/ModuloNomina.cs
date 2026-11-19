using CasinoErp.Dominio.Nominas;
using CasinoErp.Infraestructura.Repositorios;
using CasinoErp.Infraestructura.Semillas;
using Microsoft.Extensions.DependencyInjection;

namespace CasinoErp.Infraestructura.Modulos;

/// <summary>
/// Registro del módulo Nomina (contrato C1). Dueño: Felipe. Ver desarrollo/planes/Plan-Felipe.md (F5).
/// Aquí se registran sus repositorios, servicios y su ISemilla.
/// </summary>
public static class ModuloNomina
{
    public static IServiceCollection AgregarNomina(this IServiceCollection services)
    {
        // DIP: el motor recibe todas las IReglaLiquidacion registradas; un contrato nuevo es una línea más aquí.
        services.AddSingleton<IReglaLiquidacion, ReglaContratoLaboral>();
        services.AddSingleton<IReglaLiquidacion, ReglaPrestacionServicios>();
        services.AddSingleton<PayrollEngine>();
        services.AddScoped<INominaRepositorio, NominaRepositorio>();
        services.AddScoped<IParametrosNominaRepositorio, ParametrosNominaRepositorio>();
        services.AddScoped<NominaService>();
        services.AddScoped<ISemilla, SemillaNomina>();
        return services;
    }
}
