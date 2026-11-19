using CasinoErp.Dominio.Personal;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Infraestructura.Repositorios;

/// <summary>Empleados con Entity Framework Core (SQL Server / Azure SQL o SQLite en local).</summary>
public class EmpleadoRepositorio(CasinoErpDbContext db) : IEmpleadoRepositorio
{
    private DbSet<Empleado> Empleados => db.Set<Empleado>();

    public Task<bool> ExisteDocumentoAsync(string documento) => Empleados.AnyAsync(e => e.Documento == documento);

    public Task<Empleado?> ObtenerPorDocumentoAsync(string documento) => Empleados.FirstOrDefaultAsync(e => e.Documento == documento);

    public async Task<IReadOnlyList<Empleado>> ListarAsync(bool soloActivos = false)
    {
        var consulta = Empleados.AsQueryable();
        if (soloActivos) consulta = consulta.Where(e => e.Activo);
        return await consulta.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task AgregarAsync(Empleado empleado)
    {
        Empleados.Add(empleado);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
