namespace CasinoErp.Dominio.Compras;

/// <summary>Proveedor de fichas de la bóveda (HU-05).</summary>
public class Proveedor
{
    public int Id { get; set; }
    public string Nit { get; set; } = "";          // ^\d{9}-\d$  p. ej. "901234567-1"
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";       // en minúsculas
    public string Telefono { get; set; } = "";
    public bool Activo { get; set; } = true;
}

public interface IProveedorRepositorio
{
    Task<bool> ExisteNitAsync(string nit);
    Task<Proveedor?> ObtenerPorNitAsync(string nit);
    Task<IReadOnlyList<Proveedor>> ListarAsync(bool soloActivos = false);   // ordenado por Id
    Task AgregarAsync(Proveedor proveedor);
    Task GuardarCambiosAsync();
}
