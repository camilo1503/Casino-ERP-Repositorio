using System.Text.RegularExpressions;
using CasinoErp.Dominio.Comun;

namespace CasinoErp.Dominio.Inventario;

/// <summary>
/// Inventario de fichas de la bóveda: registro por denominación, entradas y retiros (Taller 4).
/// Desde la HU-05 evalúa el stock mínimo después de cada cambio y avisa a los observadores
/// cuando se abre una alerta de stock crítico.
/// </summary>
public class InventarioFichasService
{
    public const string MensajeRegistrada = "Ficha registrada";
    public const string MensajeEntrada = "Entrada registrada";
    public const string MensajeRetiro = "Retiro exitoso";
    public const string MensajeMinimo = "Stock mínimo actualizado";

    private static readonly Regex Codigo = new(@"^FICHA-[0-9]+[KM]?$");

    private readonly IFichaRepositorio _repo;
    private readonly IAlertaStockRepositorio _alertas;
    private readonly IReadOnlyList<IObservadorStockCritico> _observadores;
    private readonly TimeProvider _reloj;

    public InventarioFichasService(IFichaRepositorio repo, IAlertaStockRepositorio alertas,
                                   IEnumerable<IObservadorStockCritico> observadores, TimeProvider reloj)
    {
        _repo = repo;
        _alertas = alertas;
        _observadores = observadores.ToList();
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
        await EvaluarStockAsync(ficha);
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
        await EvaluarStockAsync(ficha);
        return ficha;
    }

    public async Task<Ficha> RetirarAsync(string codigo, int cantidad)
    {
        ValidarCantidad(cantidad);
        var ficha = await ConsultarAsync(codigo);
        if (cantidad > ficha.Stock) throw new StockInsuficienteException();
        ficha.Stock -= cantidad;
        await RegistrarMovimientoAsync(ficha, TipoMovimiento.Salida, cantidad);
        await EvaluarStockAsync(ficha);
        return ficha;
    }

    public async Task<Ficha> ActualizarStockMinimoAsync(string codigo, int stockMinimo)
    {
        if (stockMinimo < 0) throw new ReglaNegocioException("El stock mínimo no puede ser negativo");
        var ficha = await ConsultarAsync(codigo);
        ficha.StockMinimo = stockMinimo;
        // IFichaRepositorio no tiene un "guardar" propio: la ficha y las alertas comparten la misma
        // unidad de trabajo (DbContext con alcance por solicitud), así que este guardado persiste el mínimo.
        await _alertas.GuardarCambiosAsync();
        await EvaluarStockAsync(ficha);
        return ficha;
    }

    public Task<IReadOnlyList<AlertaStock>> ListarAlertasAsync(bool soloAbiertas = true) =>
        _alertas.ListarAsync(soloAbiertas);

    private async Task EvaluarStockAsync(Ficha ficha)
    {
        var abierta = await _alertas.ObtenerAbiertaAsync(ficha.Codigo);
        if (ficha.EnNivelCritico)
        {
            if (abierta is null)
            {
                var nueva = new AlertaStock
                {
                    CodigoFicha = ficha.Codigo,
                    StockActual = ficha.Stock,
                    StockMinimo = ficha.StockMinimo,
                    CreadaEn = Ahora(),
                };
                await _alertas.AgregarAsync(nueva);
                foreach (var observador in _observadores)
                    await observador.StockCriticoAsync(nueva, ficha);
            }
            else
            {
                abierta.StockActual = ficha.Stock;
                abierta.StockMinimo = ficha.StockMinimo;
                await _alertas.GuardarCambiosAsync();
            }
        }
        else if (abierta is not null)
        {
            abierta.Estado = EstadoAlerta.Atendida;
            abierta.AtendidaEn = Ahora();
            abierta.StockActual = ficha.Stock;
            await _alertas.GuardarCambiosAsync();
        }
    }

    private DateTime Ahora() => _reloj.GetUtcNow().UtcDateTime;

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
            Fecha = Ahora(),
        });
}
