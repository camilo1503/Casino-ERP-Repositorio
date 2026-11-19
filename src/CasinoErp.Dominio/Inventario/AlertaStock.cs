namespace CasinoErp.Dominio.Inventario;

public enum EstadoAlerta { Abierta, Atendida }

/// <summary>Aviso de que una denominación de la bóveda quedó por debajo de su stock mínimo (HU-05).</summary>
public class AlertaStock
{
    public int Id { get; set; }
    public string CodigoFicha { get; set; } = "";
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public EstadoAlerta Estado { get; set; } = EstadoAlerta.Abierta;
    public DateTime CreadaEn { get; set; }
    public DateTime? AtendidaEn { get; set; }
}

public interface IAlertaStockRepositorio
{
    Task<AlertaStock?> ObtenerAbiertaAsync(string codigoFicha);
    Task<IReadOnlyList<AlertaStock>> ListarAsync(bool soloAbiertas);
    Task AgregarAsync(AlertaStock alerta);
    Task GuardarCambiosAsync();
}

/// <summary>
/// Lo implementa quien quiera reaccionar a una alerta nueva (por ejemplo, Compras).
/// El inventario solo conoce esta abstracción (DIP).
/// </summary>
public interface IObservadorStockCritico
{
    Task StockCriticoAsync(AlertaStock alerta, Ficha ficha);
}
