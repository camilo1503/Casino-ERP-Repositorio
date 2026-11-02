namespace CasinoErp.Dominio.Mesas;

public enum EstadoMesa { Cerrada, Abierta, Mantenimiento, Retirada }

/// <summary>E-02 Mesa de juego del catálogo del piso.</summary>
public class Mesa
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public string Juego { get; set; } = "";
    public string Zona { get; set; } = "Zona A";
    public int Asientos { get; set; } = 7;
    public long ApuestaMinima { get; set; }
    public long ApuestaMaxima { get; set; }
    public EstadoMesa Estado { get; set; } = EstadoMesa.Cerrada;
    public string? MotivoRetiro { get; set; }
}

public interface IMesaRepositorio
{
    Task<bool> ExisteNumeroAsync(string numero);
    Task<Mesa?> ObtenerPorNumeroAsync(string numero);
    Task<IReadOnlyList<Mesa>> ListarAsync();
    Task AgregarAsync(Mesa mesa);
    Task GuardarCambiosAsync();
}
