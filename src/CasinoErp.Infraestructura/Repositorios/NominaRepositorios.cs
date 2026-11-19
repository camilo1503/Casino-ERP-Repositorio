using CasinoErp.Dominio.Nominas;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Infraestructura.Repositorios;

/// <summary>Nóminas con su detalle (Entity Framework Core).</summary>
public class NominaRepositorio(CasinoErpDbContext db) : INominaRepositorio
{
    /// <summary>Incluye el detalle en el orden en que lo generó el motor (por nombre del empleado).</summary>
    private IQueryable<Nomina> ConDetalle => db.Set<Nomina>().Include(n => n.Detalle.OrderBy(d => d.Id));

    public Task<Nomina?> ObtenerAsync(int id) => ConDetalle.FirstOrDefaultAsync(n => n.Id == id);

    public Task<Nomina?> ObtenerPorPeriodoAsync(int anio, int mes) => ConDetalle.FirstOrDefaultAsync(n => n.Anio == anio && n.Mes == mes);

    public async Task<IReadOnlyList<Nomina>> ListarAsync() =>
        await ConDetalle.OrderByDescending(n => n.Anio).ThenByDescending(n => n.Mes).ToListAsync();

    public async Task AgregarAsync(Nomina nomina)
    {
        db.Set<Nomina>().Add(nomina);
        await db.SaveChangesAsync();
    }

    public async Task EliminarAsync(Nomina nomina)
    {
        db.Set<Nomina>().Remove(nomina);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

/// <summary>Parámetros de nómina por año; guardar es un upsert por año.</summary>
public class ParametrosNominaRepositorio(CasinoErpDbContext db) : IParametrosNominaRepositorio
{
    public Task<ParametrosNomina?> ObtenerAsync(int anio) => db.Set<ParametrosNomina>().FirstOrDefaultAsync(p => p.Anio == anio);

    public async Task GuardarAsync(ParametrosNomina p)
    {
        var existente = await ObtenerAsync(p.Anio);
        if (existente is null)
        {
            db.Set<ParametrosNomina>().Add(p);
        }
        else
        {
            p.Id = existente.Id;
            db.Entry(existente).CurrentValues.SetValues(p);
        }
        await db.SaveChangesAsync();
    }
}
