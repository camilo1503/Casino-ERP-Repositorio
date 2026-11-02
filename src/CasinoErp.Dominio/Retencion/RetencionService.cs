using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Jugadores;

namespace CasinoErp.Dominio.Retencion;

/// <summary>E-03 Retención: registra la oferta cuando el jugador muestra que se quiere ir.</summary>
public class RetencionService
{
    public const string MensajeEmitida = "Oferta de retención emitida";
    public const string MensajeRechazada = "Oferta rechazada. Quedó registrada";
    public const string MensajeConfiguracion = "Configuración de retención guardada";

    private readonly IOfertaRepositorio _ofertas;
    private readonly JugadorService _jugadores;
    private readonly TimeProvider _reloj;

    public RetencionService(IOfertaRepositorio ofertas, JugadorService jugadores, TimeProvider reloj)
    {
        _ofertas = ofertas;
        _jugadores = jugadores;
        _reloj = reloj;
    }

    /// <summary>Descuento sobre una compra de fichas: porcentaje con tope.</summary>
    public async Task<long> CalcularDescuentoAsync(long monto)
    {
        if (monto <= 0) throw new ReglaNegocioException("Monto inválido");
        var config = await _ofertas.ObtenerConfiguracionAsync();
        return Math.Min((long)Math.Round(monto * config.PorcentajeDescuento / 100m), config.TopeDescuento);
    }

    public static TipoOferta OfertaSugerida(NivelLealtad nivel) => nivel switch
    {
        NivelLealtad.Diamante => TipoOferta.BonoReclamable,
        NivelLealtad.Oro => TipoOferta.FichasBono,
        _ => TipoOferta.DescuentoFichas,
    };

    /// <summary>Emite la oferta sugerida para el nivel del jugador.</summary>
    public Task<OfertaRetencion> EvaluarSenalAsync(string documento, int minutosSesion, string senal) =>
        RegistrarRespuestaAsync(documento, minutosSesion, senal, "Mesa", aceptada: true);

    /// <summary>
    /// Registra la respuesta del jugador a la oferta (aceptada con el tipo elegido, o rechazada).
    /// Las dos cuentan para el límite diario.
    /// </summary>
    public async Task<OfertaRetencion> RegistrarRespuestaAsync(string documento, int minutosSesion, string senal,
        string origen, bool aceptada, TipoOferta? tipo = null)
    {
        if (string.IsNullOrWhiteSpace(senal)) throw new ReglaNegocioException("La señal es obligatoria");
        var config = await _ofertas.ObtenerConfiguracionAsync();
        var jugador = await _jugadores.ConsultarAsync(documento);
        if (jugador.Estado != EstadoJugador.Activo) throw new ReglaNegocioException("No se ofrece retención: juego responsable");
        if (minutosSesion < config.MinutosMinimos)
            throw new ReglaNegocioException($"La sesión debe durar al menos {config.MinutosMinimos} minutos");

        var hoy = DateOnly.FromDateTime(_reloj.GetLocalNow().DateTime);
        if (await _ofertas.ContarOfertasAsync(jugador.Documento, hoy) >= config.OfertasPorDia)
            throw new ReglaNegocioException("El jugador ya recibió una oferta hoy");

        var oferta = new OfertaRetencion
        {
            Documento = jugador.Documento,
            Senal = senal.Trim(),
            Origen = string.IsNullOrWhiteSpace(origen) ? "Mesa" : origen.Trim(),
            Resultado = aceptada ? ResultadoOferta.Aceptada : ResultadoOferta.Rechazada,
            Fecha = hoy,
            RegistradaEn = _reloj.GetUtcNow().UtcDateTime,
        };
        if (aceptada)
        {
            var elegido = tipo ?? OfertaSugerida(jugador.Nivel);
            oferta.Tipo = elegido;
            oferta.Valor = elegido switch
            {
                TipoOferta.FichasBono => config.FichasBono,
                TipoOferta.BonoReclamable => config.BonoReclamable,
                _ => config.PorcentajeDescuento,
            };
            oferta.Vence = hoy.AddDays(elegido == TipoOferta.BonoReclamable ? config.DiasVigencia : 1);
        }
        await _ofertas.AgregarAsync(oferta);
        return oferta;
    }

    public Task<IReadOnlyList<OfertaRetencion>> ListarAsync() => _ofertas.ListarAsync();

    public Task<ConfiguracionRetencion> ConsultarConfiguracionAsync() => _ofertas.ObtenerConfiguracionAsync();

    public async Task<ConfiguracionRetencion> ActualizarConfiguracionAsync(ConfiguracionRetencion nueva)
    {
        if (nueva.PorcentajeDescuento is < 1 or > 50) throw new ReglaNegocioException("El descuento debe estar entre 1% y 50%");
        if (nueva.TopeDescuento < 0 || nueva.FichasBono < 0 || nueva.BonoReclamable < 0 || nueva.MinutosMinimos < 0 || nueva.DiasVigencia < 1)
            throw new ReglaNegocioException("Revisa los valores: deben ser números positivos");
        if (nueva.OfertasPorDia < 1) throw new ReglaNegocioException("Debe permitirse al menos una oferta por día");

        var config = await _ofertas.ObtenerConfiguracionAsync();
        config.PorcentajeDescuento = nueva.PorcentajeDescuento;
        config.TopeDescuento = nueva.TopeDescuento;
        config.FichasBono = nueva.FichasBono;
        config.BonoReclamable = nueva.BonoReclamable;
        config.DiasVigencia = nueva.DiasVigencia;
        config.MinutosMinimos = nueva.MinutosMinimos;
        config.OfertasPorDia = nueva.OfertasPorDia;
        config.SenalCambioFichas = nueva.SenalCambioFichas;
        config.SenalQuiereRetirarse = nueva.SenalQuiereRetirarse;
        config.SenalCobroCaja = nueva.SenalCobroCaja;
        config.SenalAppJugador = nueva.SenalAppJugador;
        await _ofertas.GuardarCambiosAsync();
        return config;
    }
}
