namespace PitBoss.Specs.Dominio;

/// <summary>Ficha de casino registrada en la bóveda (Taller 4 / Ejercicio 5).</summary>
public class Ficha
{
    public Ficha(string codigo, long denominacion, string categoria, int stock)
    {
        Codigo = codigo;
        Denominacion = denominacion;
        Categoria = categoria;
        Stock = stock;
    }

    public string Codigo { get; }
    public long Denominacion { get; }
    public string Categoria { get; }
    public int Stock { get; set; }
}

public class InsufficientStockException : Exception
{
    public InsufficientStockException() : base("Stock insuficiente") { }
}

public interface IFichaRepository
{
    bool ExistsByCode(string codigo);
    Ficha? FindByCode(string codigo);
    Ficha Save(Ficha ficha);
}

public class InMemoryFichaRepository : IFichaRepository
{
    private readonly Dictionary<string, Ficha> _datos = new();

    public bool ExistsByCode(string codigo) => _datos.ContainsKey(codigo);
    public Ficha? FindByCode(string codigo) => _datos.TryGetValue(codigo, out var f) ? f : null;
    public Ficha Save(Ficha ficha) { _datos[ficha.Codigo] = ficha; return ficha; }
}

/// <summary>Servicio de inventario de fichas. Misma regla que en Java y Node: no se retira más del stock.</summary>
public class InventarioFichasService
{
    private readonly IFichaRepository _repo;

    public InventarioFichasService(IFichaRepository repo) => _repo = repo;

    public Ficha Registrar(Ficha ficha)
    {
        if (string.IsNullOrWhiteSpace(ficha.Categoria)) throw new ArgumentException("La categoría es obligatoria");
        if (ficha.Stock < 0) throw new ArgumentException("El stock inicial no puede ser negativo");
        if (_repo.ExistsByCode(ficha.Codigo)) throw new ArgumentException("El código de ficha ya está registrado");
        return _repo.Save(ficha);
    }

    public Ficha Consultar(string codigo) =>
        _repo.FindByCode(codigo) ?? throw new KeyNotFoundException("Ficha no encontrada");

    public Ficha Retirar(string codigo, int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero");
        var ficha = Consultar(codigo);
        if (cantidad > ficha.Stock) throw new InsufficientStockException();
        ficha.Stock -= cantidad;
        return _repo.Save(ficha);
    }
}
