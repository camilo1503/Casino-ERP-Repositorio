using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Jugadores;

namespace CasinoErp.Dominio.Caja;

/// <summary>Caja / POS: compras de fichas, pagos a jugadores y revisión de cumplimiento.</summary>
public class CajaService
{
    public const string MensajeAprobadaRevision = "Transacción aprobada";
    public const string MensajeAnulada = "Transacción anulada";

    public static readonly IReadOnlyList<string> EstadosCumplimiento = ["Reportada", "Escalada"];

    /// <summary>Transacciones desde este monto pasan a revisión de cumplimiento (SARLAFT).</summary>
    public const long UmbralCumplimiento = 5_000_000;

    public static readonly IReadOnlyList<long> Denominaciones = [5000, 10000, 25000, 50000, 100000, 500000];

    private readonly ITransaccionRepositorio _transacciones;
    private readonly JugadorService _jugadores;
    private readonly TimeProvider _reloj;

    public CajaService(ITransaccionRepositorio transacciones, JugadorService jugadores, TimeProvider reloj)
    {
        _transacciones = transacciones;
        _jugadores = jugadores;
        _reloj = reloj;
    }

    public static long CalcularTotal(long denominacion, int cantidad)
    {
        if (!Denominaciones.Contains(denominacion)) throw new ReglaNegocioException("Denominación no válida");
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor que cero");
        return denominacion * cantidad;
    }

    public async Task<TransaccionCaja> RegistrarAsync(TipoTransaccion tipo, string? documento, long monto,
        long descuento = 0, string caja = "Caja 2", string? ubicacion = null)
    {
        if (monto <= 0) throw new ReglaNegocioException("El monto debe ser mayor que cero");
        if (descuento < 0 || descuento >= monto) throw new ReglaNegocioException("El descuento no es válido");
        if (descuento > 0 && tipo != TipoTransaccion.Compra) throw new ReglaNegocioException("El descuento solo aplica a compras");

        Jugador? jugador = null;
        if (!string.IsNullOrWhiteSpace(documento))
        {
            jugador = await _jugadores.ConsultarAsync(documento);
            if (tipo == TipoTransaccion.Compra && jugador.Estado == EstadoJugador.Autoexcluido)
                throw new ReglaNegocioException("No se venden fichas a un jugador autoexcluido");
            if (tipo == TipoTransaccion.Compra && jugador.Estado != EstadoJugador.Activo)
                throw new ReglaNegocioException("El jugador no está activo");
        }

        var transaccion = new TransaccionCaja
        {
            Tipo = tipo,
            Documento = jugador?.Documento,
            Monto = monto,
            Descuento = descuento,
            Estado = monto >= UmbralCumplimiento ? EstadoTransaccion.EnRevision : EstadoTransaccion.Aprobada,
            Caja = caja,
            Ubicacion = ubicacion,
            Fecha = _reloj.GetUtcNow().UtcDateTime,
        };
        if (transaccion.Estado == EstadoTransaccion.EnRevision) transaccion.EstadoCumplimiento = "En revisión";
        await _transacciones.AgregarAsync(transaccion);
        if (jugador is not null && tipo == TipoTransaccion.Compra) await _jugadores.SumarPuntosAsync(jugador, monto);
        return transaccion;
    }

    /// <summary>El supervisor aprueba una transacción que estaba en revisión.</summary>
    public async Task<TransaccionCaja> AprobarRevisionAsync(int id)
    {
        var transaccion = await ConsultarAsync(id);
        if (transaccion.Estado != EstadoTransaccion.EnRevision) throw new ReglaNegocioException("La transacción no está en revisión");
        transaccion.Estado = EstadoTransaccion.Aprobada;
        await _transacciones.GuardarCambiosAsync();
        return transaccion;
    }

    public async Task<TransaccionCaja> AnularAsync(int id)
    {
        var transaccion = await ConsultarAsync(id);
        if (transaccion.Estado != EstadoTransaccion.Aprobada) throw new ReglaNegocioException("Solo se anulan transacciones aprobadas");
        transaccion.Estado = EstadoTransaccion.Anulada;
        await _transacciones.GuardarCambiosAsync();
        return transaccion;
    }

    /// <summary>El oficial de cumplimiento reporta a la UIAF o escala a gerencia una transacción sobre el umbral.</summary>
    public async Task<TransaccionCaja> CambiarEstadoCumplimientoAsync(int id, string estado)
    {
        if (!EstadosCumplimiento.Contains(estado)) throw new ReglaNegocioException("El estado de cumplimiento no es válido");
        var transaccion = await ConsultarAsync(id);
        if (transaccion.Monto < UmbralCumplimiento) throw new ReglaNegocioException("La transacción no supera el umbral de cumplimiento");
        if (transaccion.Estado == EstadoTransaccion.Anulada) throw new ReglaNegocioException("La transacción está anulada");
        transaccion.EstadoCumplimiento = estado;
        await _transacciones.GuardarCambiosAsync();
        return transaccion;
    }

    public async Task<TransaccionCaja> ConsultarAsync(int id) =>
        await _transacciones.ObtenerAsync(id) ?? throw new NoEncontradoException("Transacción no encontrada");

    public Task<IReadOnlyList<TransaccionCaja>> ListarAsync(string? documento = null) => _transacciones.ListarAsync(documento);

    /// <summary>Mensajes de pruebas/features/compra_fichas.feature.</summary>
    public static string Mensaje(TransaccionCaja transaccion) => (transaccion.Tipo, transaccion.Estado) switch
    {
        (TipoTransaccion.Compra, EstadoTransaccion.EnRevision) => "Compra enviada a revisión de cumplimiento",
        (TipoTransaccion.Compra, _) => "Compra registrada",
        (_, EstadoTransaccion.EnRevision) => "Pago enviado a revisión de cumplimiento",
        _ => "Pago registrado",
    };
}
