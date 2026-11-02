namespace CasinoErp.Dominio.Caja;

public enum TipoTransaccion { Compra, Pago }

public enum EstadoTransaccion { Aprobada, EnRevision, Anulada }

/// <summary>Compra de fichas o pago a un jugador registrado en caja (POS).</summary>
public class TransaccionCaja
{
    public int Id { get; set; }
    public TipoTransaccion Tipo { get; set; }
    /// <summary>Documento del jugador; vacío si no está registrado.</summary>
    public string? Documento { get; set; }
    public long Monto { get; set; }
    /// <summary>Descuento de retención aplicado sobre la compra.</summary>
    public long Descuento { get; set; }
    public EstadoTransaccion Estado { get; set; }
    public string Caja { get; set; } = "Caja 2";
    /// <summary>Juego y mesa donde se usaron las fichas (ej. "Blackjack · M-01").</summary>
    public string? Ubicacion { get; set; }
    public DateTime Fecha { get; set; }
    /// <summary>Seguimiento de cumplimiento para montos sobre el umbral: En revisión, Reportada o Escalada.</summary>
    public string? EstadoCumplimiento { get; set; }

    public string Codigo => $"TX-{Id:D5}";
}

public interface ITransaccionRepositorio
{
    Task<TransaccionCaja?> ObtenerAsync(int id);
    Task<IReadOnlyList<TransaccionCaja>> ListarAsync(string? documento = null);
    Task AgregarAsync(TransaccionCaja transaccion);
    Task GuardarCambiosAsync();
}
