using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Inventario;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Dominio.Retencion;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Infraestructura;

/// <summary>Repositorios con Entity Framework Core (SQL Server / Azure SQL o SQLite en local).</summary>
public class JugadorRepositorio(CasinoErpDbContext db) : IJugadorRepositorio
{
    public Task<bool> ExisteDocumentoAsync(string documento) => db.Jugadores.AnyAsync(j => j.Documento == documento);

    public Task<bool> ExisteCorreoAsync(string correo) => db.Jugadores.AnyAsync(j => j.Correo == correo);

    public Task<Jugador?> ObtenerPorDocumentoAsync(string documento) =>
        db.Jugadores.FirstOrDefaultAsync(j => j.Documento == documento);

    public async Task<IReadOnlyList<Jugador>> ListarAsync(string? busqueda = null)
    {
        var consulta = db.Jugadores.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busqueda))
            consulta = consulta.Where(j => j.Documento.Contains(busqueda) || j.Nombre.Contains(busqueda));
        return await consulta.OrderBy(j => j.Id).ToListAsync();
    }

    public async Task AgregarAsync(Jugador jugador)
    {
        db.Jugadores.Add(jugador);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

public class MesaRepositorio(CasinoErpDbContext db) : IMesaRepositorio
{
    public Task<bool> ExisteNumeroAsync(string numero) => db.Mesas.AnyAsync(m => m.Numero == numero);

    public Task<Mesa?> ObtenerPorNumeroAsync(string numero) => db.Mesas.FirstOrDefaultAsync(m => m.Numero == numero);

    public async Task<IReadOnlyList<Mesa>> ListarAsync() => await db.Mesas.OrderBy(m => m.Numero).ToListAsync();

    public async Task AgregarAsync(Mesa mesa)
    {
        db.Mesas.Add(mesa);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

public class TransaccionRepositorio(CasinoErpDbContext db) : ITransaccionRepositorio
{
    public Task<TransaccionCaja?> ObtenerAsync(int id) => db.Transacciones.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IReadOnlyList<TransaccionCaja>> ListarAsync(string? documento = null)
    {
        var consulta = db.Transacciones.AsQueryable();
        if (!string.IsNullOrWhiteSpace(documento)) consulta = consulta.Where(t => t.Documento == documento);
        return await consulta.OrderByDescending(t => t.Id).ToListAsync();
    }

    public async Task AgregarAsync(TransaccionCaja transaccion)
    {
        db.Transacciones.Add(transaccion);
        await db.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

public class OfertaRepositorio(CasinoErpDbContext db) : IOfertaRepositorio
{
    public Task<int> ContarOfertasAsync(string documento, DateOnly fecha) =>
        db.Ofertas.CountAsync(o => o.Documento == documento && o.Fecha == fecha);

    public async Task<IReadOnlyList<OfertaRetencion>> ListarAsync() =>
        await db.Ofertas.OrderByDescending(o => o.Id).ToListAsync();

    public async Task AgregarAsync(OfertaRetencion oferta)
    {
        db.Ofertas.Add(oferta);
        await db.SaveChangesAsync();
    }

    public async Task<ConfiguracionRetencion> ObtenerConfiguracionAsync()
    {
        var config = await db.ConfiguracionRetencion.FirstOrDefaultAsync(c => c.Id == 1);
        if (config is null)
        {
            config = new ConfiguracionRetencion();
            db.ConfiguracionRetencion.Add(config);
            await db.SaveChangesAsync();
        }
        return config;
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

public class RellenoRepositorio(CasinoErpDbContext db) : IRellenoRepositorio
{
    public Task<Relleno?> ObtenerAsync(int id) => db.Rellenos.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IReadOnlyList<Relleno>> ListarAsync() => await db.Rellenos.OrderByDescending(r => r.Id).ToListAsync();

    public Task<bool> ExistePendienteAsync(string mesaNumero) =>
        db.Rellenos.AnyAsync(r => r.MesaNumero == mesaNumero && r.Estado == EstadoRelleno.Pendiente);

    public async Task AgregarAsync(Relleno relleno)
    {
        db.Rellenos.Add(relleno);
        await db.SaveChangesAsync();
    }

    public async Task<Boveda> ObtenerBovedaAsync()
    {
        var boveda = await db.Bovedas.FirstOrDefaultAsync(b => b.Id == 1);
        if (boveda is null)
        {
            boveda = new Boveda { Id = 1, Saldo = 0 };
            db.Bovedas.Add(boveda);
        }
        return boveda;
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}

public class FichaRepositorio(CasinoErpDbContext db) : IFichaRepositorio
{
    public Task<bool> ExisteCodigoAsync(string codigo) => db.Fichas.AnyAsync(f => f.Codigo == codigo);

    public Task<Ficha?> ObtenerPorCodigoAsync(string codigo) => db.Fichas.FirstOrDefaultAsync(f => f.Codigo == codigo);

    public async Task<IReadOnlyList<Ficha>> ListarAsync() => await db.Fichas.OrderBy(f => f.Denominacion).ToListAsync();

    public async Task<IReadOnlyList<MovimientoInventario>> ListarMovimientosAsync(string? codigo = null)
    {
        var consulta = db.Movimientos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(codigo)) consulta = consulta.Where(m => m.CodigoFicha == codigo);
        return await consulta.OrderByDescending(m => m.Id).ToListAsync();
    }

    public async Task AgregarAsync(Ficha ficha)
    {
        db.Fichas.Add(ficha);
        await db.SaveChangesAsync();
    }

    public async Task AgregarMovimientoAsync(MovimientoInventario movimiento)
    {
        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync();
    }
}
