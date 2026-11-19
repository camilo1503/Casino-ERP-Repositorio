using CasinoErp.Dominio.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Infraestructura.Repositorios;

/// <summary>Usuarios con Entity Framework Core (SQL Server / Azure SQL o SQLite en local).</summary>
public class UsuarioRepositorio(CasinoErpDbContext db) : IUsuarioRepositorio
{
    private DbSet<Usuario> Usuarios => db.Set<Usuario>();

    public Task<Usuario?> ObtenerAsync(int id) => Usuarios.FirstOrDefaultAsync(u => u.Id == id);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var normalizado = correo.Trim().ToLowerInvariant();
        return Usuarios.FirstOrDefaultAsync(u => u.Correo == normalizado);
    }

    public Task<bool> ExisteCorreoAsync(string correo)
    {
        var normalizado = correo.Trim().ToLowerInvariant();
        return Usuarios.AnyAsync(u => u.Correo == normalizado);
    }

    public async Task<IReadOnlyList<Usuario>> ListarAsync() => await Usuarios.OrderBy(u => u.Id).ToListAsync();

    public async Task AgregarAsync(Usuario usuario)
    {
        Usuarios.Add(usuario);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
