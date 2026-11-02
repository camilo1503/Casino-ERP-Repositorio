using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Inventario;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Dominio.Retencion;

namespace CasinoErp.PruebasUnitarias.Dobles;

/// <summary>Reloj fijo para que las reglas de edad y vigencia no dependan de la fecha real.</summary>
public class RelojFijo(DateTimeOffset ahora) : TimeProvider
{
    public RelojFijo() : this(new DateTimeOffset(2026, 10, 26, 15, 0, 0, TimeSpan.Zero)) { }

    public override DateTimeOffset GetUtcNow() => ahora;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

/// <summary>Repositorios en memoria: reemplazan a SQL Server en las pruebas unitarias.</summary>
public class JugadoresEnMemoria : IJugadorRepositorio
{
    public readonly List<Jugador> Datos = [];
    public int Guardados { get; private set; }

    public Task<bool> ExisteDocumentoAsync(string documento) => Task.FromResult(Datos.Any(j => j.Documento == documento));
    public Task<bool> ExisteCorreoAsync(string correo) => Task.FromResult(Datos.Any(j => j.Correo == correo));
    public Task<Jugador?> ObtenerPorDocumentoAsync(string documento) => Task.FromResult(Datos.FirstOrDefault(j => j.Documento == documento));
    public Task<IReadOnlyList<Jugador>> ListarAsync(string? busqueda = null) =>
        Task.FromResult<IReadOnlyList<Jugador>>(Datos.Where(j => busqueda is null || j.Nombre.Contains(busqueda) || j.Documento.Contains(busqueda)).ToList());
    public Task AgregarAsync(Jugador jugador) { jugador.Id = Datos.Count + 1; Datos.Add(jugador); Guardados++; return Task.CompletedTask; }
    public Task GuardarCambiosAsync() { Guardados++; return Task.CompletedTask; }
}

public class MesasEnMemoria : IMesaRepositorio
{
    public readonly List<Mesa> Datos = [];

    public Task<bool> ExisteNumeroAsync(string numero) => Task.FromResult(Datos.Any(m => m.Numero == numero));
    public Task<Mesa?> ObtenerPorNumeroAsync(string numero) => Task.FromResult(Datos.FirstOrDefault(m => m.Numero == numero));
    public Task<IReadOnlyList<Mesa>> ListarAsync() => Task.FromResult<IReadOnlyList<Mesa>>(Datos.ToList());
    public Task AgregarAsync(Mesa mesa) { mesa.Id = Datos.Count + 1; Datos.Add(mesa); return Task.CompletedTask; }
    public Task GuardarCambiosAsync() => Task.CompletedTask;
}

public class TransaccionesEnMemoria : ITransaccionRepositorio
{
    public readonly List<TransaccionCaja> Datos = [];

    public Task<TransaccionCaja?> ObtenerAsync(int id) => Task.FromResult(Datos.FirstOrDefault(t => t.Id == id));
    public Task<IReadOnlyList<TransaccionCaja>> ListarAsync(string? documento = null) =>
        Task.FromResult<IReadOnlyList<TransaccionCaja>>(Datos.Where(t => documento is null || t.Documento == documento).ToList());
    public Task AgregarAsync(TransaccionCaja transaccion) { transaccion.Id = Datos.Count + 1; Datos.Add(transaccion); return Task.CompletedTask; }
    public Task GuardarCambiosAsync() => Task.CompletedTask;
}

public class OfertasEnMemoria : IOfertaRepositorio
{
    public readonly List<OfertaRetencion> Datos = [];
    public ConfiguracionRetencion Configuracion { get; set; } = new();

    public Task<int> ContarOfertasAsync(string documento, DateOnly fecha) =>
        Task.FromResult(Datos.Count(o => o.Documento == documento && o.Fecha == fecha));
    public Task<IReadOnlyList<OfertaRetencion>> ListarAsync() => Task.FromResult<IReadOnlyList<OfertaRetencion>>(Datos.ToList());
    public Task AgregarAsync(OfertaRetencion oferta) { oferta.Id = Datos.Count + 1; Datos.Add(oferta); return Task.CompletedTask; }
    public Task<ConfiguracionRetencion> ObtenerConfiguracionAsync() => Task.FromResult(Configuracion);
    public Task GuardarCambiosAsync() => Task.CompletedTask;
}

public class RellenosEnMemoria(long saldoBoveda = 0) : IRellenoRepositorio
{
    public readonly List<Relleno> Datos = [];
    public readonly Boveda Boveda = new() { Saldo = saldoBoveda };

    public Task<Relleno?> ObtenerAsync(int id) => Task.FromResult(Datos.FirstOrDefault(r => r.Id == id));
    public Task<IReadOnlyList<Relleno>> ListarAsync() => Task.FromResult<IReadOnlyList<Relleno>>(Datos.ToList());
    public Task<bool> ExistePendienteAsync(string mesaNumero) =>
        Task.FromResult(Datos.Any(r => r.MesaNumero == mesaNumero && r.Estado == EstadoRelleno.Pendiente));
    public Task AgregarAsync(Relleno relleno) { if (relleno.Id == 0) relleno.Id = Datos.Count + 1; Datos.Add(relleno); return Task.CompletedTask; }
    public Task<Boveda> ObtenerBovedaAsync() => Task.FromResult(Boveda);
    public Task GuardarCambiosAsync() => Task.CompletedTask;
}

public class FichasEnMemoria : IFichaRepositorio
{
    public readonly List<Ficha> Datos = [];
    public readonly List<MovimientoInventario> Movimientos = [];

    public Task<bool> ExisteCodigoAsync(string codigo) => Task.FromResult(Datos.Any(f => f.Codigo == codigo));
    public Task<Ficha?> ObtenerPorCodigoAsync(string codigo) => Task.FromResult(Datos.FirstOrDefault(f => f.Codigo == codigo));
    public Task<IReadOnlyList<Ficha>> ListarAsync() => Task.FromResult<IReadOnlyList<Ficha>>(Datos.ToList());
    public Task<IReadOnlyList<MovimientoInventario>> ListarMovimientosAsync(string? codigo = null) =>
        Task.FromResult<IReadOnlyList<MovimientoInventario>>(Movimientos.Where(m => codigo is null || m.CodigoFicha == codigo).ToList());
    public Task AgregarAsync(Ficha ficha) { ficha.Id = Datos.Count + 1; Datos.Add(ficha); return Task.CompletedTask; }
    public Task AgregarMovimientoAsync(MovimientoInventario movimiento) { Movimientos.Add(movimiento); return Task.CompletedTask; }
}
