namespace CasinoErp.Dominio.Retencion;

public enum TipoOferta { DescuentoFichas, FichasBono, BonoReclamable }

public enum ResultadoOferta { Aceptada, Rechazada }

/// <summary>E-03 Oferta que se le hace al jugador cuando muestra que se quiere ir.</summary>
public class OfertaRetencion
{
    public int Id { get; set; }
    public string Documento { get; set; } = "";
    public string Senal { get; set; } = "";
    /// <summary>Dónde se detectó la señal: "Mesa M-01", "Caja 2" o "App del jugador".</summary>
    public string Origen { get; set; } = "";
    public ResultadoOferta Resultado { get; set; }
    /// <summary>Oferta aceptada; vacía si el jugador rechazó las opciones.</summary>
    public TipoOferta? Tipo { get; set; }
    public long Valor { get; set; }
    public DateOnly Fecha { get; set; }
    public DateTime RegistradaEn { get; set; }
    public DateOnly? Vence { get; set; }

    public string Codigo => $"RET-{Id:D4}";
}

/// <summary>Parámetros de retención aprobados en el Sprint 1 (RF-PRO-006 / RF-PRO-007). Se editan desde el ERP.</summary>
public class ConfiguracionRetencion
{
    public int Id { get; set; } = 1;
    public int PorcentajeDescuento { get; set; } = 20;
    public long TopeDescuento { get; set; } = 200_000;
    public long FichasBono { get; set; } = 50_000;
    public long BonoReclamable { get; set; } = 100_000;
    public int DiasVigencia { get; set; } = 30;
    public int MinutosMinimos { get; set; } = 30;
    public int OfertasPorDia { get; set; } = 1;
    public bool SenalCambioFichas { get; set; } = true;
    public bool SenalQuiereRetirarse { get; set; } = true;
    public bool SenalCobroCaja { get; set; } = true;
    public bool SenalAppJugador { get; set; } = true;
}

public interface IOfertaRepositorio
{
    Task<int> ContarOfertasAsync(string documento, DateOnly fecha);
    Task<IReadOnlyList<OfertaRetencion>> ListarAsync();
    Task AgregarAsync(OfertaRetencion oferta);
    Task<ConfiguracionRetencion> ObtenerConfiguracionAsync();
    Task GuardarCambiosAsync();
}
