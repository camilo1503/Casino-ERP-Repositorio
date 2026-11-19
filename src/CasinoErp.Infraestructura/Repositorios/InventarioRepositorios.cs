using CasinoErp.Dominio.Inventario;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Infraestructura.Repositorios;

/// <summary>Alertas de stock crítico con Entity Framework Core (HU-05).</summary>
public class AlertaStockRepositorio(CasinoErpDbContext db) : IAlertaStockRepositorio
{
    private DbSet<AlertaStock> Alertas => db.Set<AlertaStock>();

    public Task<AlertaStock?> ObtenerAbiertaAsync(string codigoFicha) =>
        Alertas.FirstOrDefaultAsync(a => a.CodigoFicha == codigoFicha && a.Estado == EstadoAlerta.Abierta);

    public async Task<IReadOnlyList<AlertaStock>> ListarAsync(bool soloAbiertas)
    {
        var consulta = Alertas.AsQueryable();
        if (soloAbiertas) consulta = consulta.Where(a => a.Estado == EstadoAlerta.Abierta);
        return await consulta.OrderByDescending(a => a.CreadaEn).ToListAsync();
    }

    public async Task AgregarAsync(AlertaStock alerta)
    {
        Alertas.Add(alerta);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
