namespace CasinoErp.Dominio.Inventario;

public enum TipoMovimiento { Entrada, Salida }

/// <summary>Denominación de ficha guardada en la bóveda.</summary>
public class Ficha
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public long Denominacion { get; set; }
    public string Categoria { get; set; } = "";
    public int Stock { get; set; }

    public long ValorTotal => Denominacion * Stock;
}

/// <summary>Historial de entradas y salidas de fichas.</summary>
public class MovimientoInventario
{
    public int Id { get; set; }
    public string CodigoFicha { get; set; } = "";
    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public int StockFinal { get; set; }
    public DateTime Fecha { get; set; }
}

public interface IFichaRepositorio
{
    Task<bool> ExisteCodigoAsync(string codigo);
    Task<Ficha?> ObtenerPorCodigoAsync(string codigo);
    Task<IReadOnlyList<Ficha>> ListarAsync();
    Task<IReadOnlyList<MovimientoInventario>> ListarMovimientosAsync(string? codigo = null);
    Task AgregarAsync(Ficha ficha);
    /// <summary>Registra el movimiento y guarda los cambios pendientes de la ficha.</summary>
    Task AgregarMovimientoAsync(MovimientoInventario movimiento);
}
