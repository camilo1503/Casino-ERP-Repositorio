using CasinoErp.Dominio.Inventario;

namespace CasinoErp.PruebasUnitarias.Dobles;

/// <summary>Repositorio de alertas de stock en memoria (HU-05).</summary>
public class AlertasEnMemoria : IAlertaStockRepositorio
{
    public readonly List<AlertaStock> Datos = [];
    public int Guardados { get; private set; }

    public Task<AlertaStock?> ObtenerAbiertaAsync(string codigoFicha) =>
        Task.FromResult(Datos.FirstOrDefault(a => a.CodigoFicha == codigoFicha && a.Estado == EstadoAlerta.Abierta));

    public Task<IReadOnlyList<AlertaStock>> ListarAsync(bool soloAbiertas) =>
        Task.FromResult<IReadOnlyList<AlertaStock>>(Datos
            .Where(a => !soloAbiertas || a.Estado == EstadoAlerta.Abierta)
            .OrderByDescending(a => a.CreadaEn)
            .ToList());

    public Task AgregarAsync(AlertaStock alerta) { alerta.Id = Datos.Count + 1; Datos.Add(alerta); Guardados++; return Task.CompletedTask; }

    public Task GuardarCambiosAsync() { Guardados++; return Task.CompletedTask; }
}

/// <summary>Observador que solo guarda los avisos recibidos.</summary>
public class ObservadorEspia : IObservadorStockCritico
{
    public readonly List<AlertaStock> Avisos = [];

    public Task StockCriticoAsync(AlertaStock alerta, Ficha ficha) { Avisos.Add(alerta); return Task.CompletedTask; }
}
