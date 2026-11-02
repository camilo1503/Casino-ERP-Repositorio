using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoErp.Api.Controllers;

public record Resumen(
    int JugadoresActivos,
    int MesasAbiertas,
    int MesasTotales,
    long SaldoBoveda,
    long ValorInventarioFichas,
    int RellenosPendientes,
    long VentasCaja,
    int OfertasEmitidas);

/// <summary>Indicadores para el tablero principal del ERP.</summary>
[ApiController]
[Route("api/resumen")]
public class ResumenController(CasinoErpDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<Resumen> Obtener()
    {
        var fichas = await db.Fichas.Select(f => new { f.Denominacion, f.Stock }).ToListAsync();
        var compras = await db.Transacciones
            .Where(t => t.Tipo == TipoTransaccion.Compra && t.Estado != EstadoTransaccion.Anulada)
            .Select(t => t.Monto).ToListAsync();
        return new Resumen(
            await db.Jugadores.CountAsync(j => j.Estado == EstadoJugador.Activo),
            await db.Mesas.CountAsync(m => m.Estado == EstadoMesa.Abierta),
            await db.Mesas.CountAsync(m => m.Estado != EstadoMesa.Retirada),
            (await db.Bovedas.FirstOrDefaultAsync())?.Saldo ?? 0,
            fichas.Sum(f => f.Denominacion * f.Stock),
            await db.Rellenos.CountAsync(r => r.Estado == EstadoRelleno.Pendiente),
            compras.Sum(),
            await db.Ofertas.CountAsync());
    }
}
