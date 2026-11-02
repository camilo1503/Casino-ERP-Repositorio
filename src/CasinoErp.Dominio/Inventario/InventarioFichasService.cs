using System.Text.RegularExpressions;
using CasinoErp.Dominio.Comun;

namespace CasinoErp.Dominio.Inventario;

/// <summary>Inventario de fichas de la bóveda: registro por denominación, entradas y retiros (Taller 4).</summary>
public class InventarioFichasService
{
    public const string MensajeRegistrada = "Ficha registrada";
    public const string MensajeEntrada = "Entrada registrada";
    public const string MensajeRetiro = "Retiro exitoso";

    private static readonly Regex Codigo = new(@"^FICHA-[0-9]+[KM]?$");

    private readonly IFichaRepositorio _repo;
    private readonly TimeProvider _reloj;

    public InventarioFichasService(IFichaRepositorio repo, TimeProvider reloj)
    {
        _repo = repo;
        _reloj = reloj;
    }

    public async Task<Ficha> RegistrarAsync(string codigo, long denominacion, string categoria, int stockInicial)
    {
        if (!Codigo.IsMatch(codigo ?? "")) throw new ReglaNegocioException("El código de ficha no es válido");
        if (denominacion <= 0) throw new ReglaNegocioException("La denominación debe ser mayor que cero");
        if (string.IsNullOrWhiteSpace(categoria)) throw new ReglaNegocioException("La categoría es obligatoria");
        if (stockInicial < 0) throw new ReglaNegocioException("El stock inicial no puede ser negativo");
        if (await _repo.ExisteCodigoAsync(codigo!)) throw new ReglaNegocioException("El código de ficha ya está registrado");

        var ficha = new Ficha { Codigo = codigo!, Denominacion = denominacion, Categoria = categoria.Trim(), Stock = stockInicial };
        await _repo.AgregarAsync(ficha);
        return ficha;
    }

    public async Task<Ficha> ConsultarAsync(string codigo) =>
        await _repo.ObtenerPorCodigoAsync(codigo) ?? throw new NoEncontradoException("Ficha no encontrada");

    public Task<IReadOnlyList<Ficha>> ListarAsync() => _repo.ListarAsync();

    public Task<IReadOnlyList<MovimientoInventario>> ListarMovimientosAsync(string? codigo = null) =>
        _repo.ListarMovimientosAsync(codigo);

    public async Task<Ficha> RegistrarEntradaAsync(string codigo, int cantidad)
    {
        ValidarCantidad(cantidad);
        var ficha = await ConsultarAsync(codigo);
        ficha.Stock += cantidad;
        await RegistrarMovimientoAsync(ficha, TipoMovimiento.Entrada, cantidad);
        return ficha;
    }

    public async Task<Ficha> RetirarAsync(string codigo, int cantidad)
    {
        ValidarCantidad(cantidad);
        var ficha = await ConsultarAsync(codigo);
        if (cantidad > ficha.Stock) throw new StockInsuficienteException();
        ficha.Stock -= cantidad;
        await RegistrarMovimientoAsync(ficha, TipoMovimiento.Salida, cantidad);
        return ficha;
    }

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor que cero");
    }

    private Task RegistrarMovimientoAsync(Ficha ficha, TipoMovimiento tipo, int cantidad) =>
        _repo.AgregarMovimientoAsync(new MovimientoInventario
        {
            CodigoFicha = ficha.Codigo,
            Tipo = tipo,
            Cantidad = cantidad,
            StockFinal = ficha.Stock,
            Fecha = _reloj.GetUtcNow().UtcDateTime,
        });
}
