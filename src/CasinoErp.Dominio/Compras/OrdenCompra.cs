namespace CasinoErp.Dominio.Compras;

public enum EstadoOrden { Borrador, Enviada, Recibida, Cancelada }

/// <summary>Orden de compra de fichas a un proveedor. Nace en Borrador, manual o por una alerta de stock crítico.</summary>
public class OrdenCompra
{
    public const string OrigenAlerta = "Alerta de stock", OrigenManual = "Manual";

    public int Id { get; set; }
    public string? ProveedorNit { get; set; }
    public string CodigoFicha { get; set; } = "";
    public int Cantidad { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Borrador;
    public string Origen { get; set; } = OrigenManual;
    public DateTime CreadaEn { get; set; }
    public DateTime? ActualizadaEn { get; set; }

    public string Codigo => $"OC-{Id:D4}";
}

public interface IOrdenCompraRepositorio
{
    Task<OrdenCompra?> ObtenerAsync(int id);
    Task<IReadOnlyList<OrdenCompra>> ListarAsync();                 // más recientes primero
    Task<bool> ExisteAbiertaAsync(string codigoFicha);              // Borrador o Enviada
    Task AgregarAsync(OrdenCompra orden);
    Task GuardarCambiosAsync();
}
