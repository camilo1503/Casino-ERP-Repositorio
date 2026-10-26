const { ReglaNegocioError } = require('../errores');

/** Parámetros de retención aprobados en el Sprint 1 (RF-PRO-006 / RF-PRO-007). */
const CONFIG = {
  porcentajeDescuento: 20,
  topeDescuento: 200000,
  fichasBono: 50000,
  bonoReclamable: 100000,
  diasVigencia: 30,
  minutosMinimos: 30,
  ofertasPorDia: 1,
};
const MENSAJES = { EMITIDA: 'Oferta de retención emitida' };

const nivelPorPuntos = (p) => (p >= 10000 ? 'Diamante' : p >= 5000 ? 'Oro' : p >= 2000 ? 'Plata' : 'Bronce');
const TIPO_POR_NIVEL = { Bronce: 'DESCUENTO_FICHAS', Plata: 'DESCUENTO_FICHAS', Oro: 'FICHAS_BONO', Diamante: 'BONO_RECLAMABLE' };

function sumarDias(fecha, dias) {
  const d = new Date(fecha + 'T00:00:00Z');
  d.setUTCDate(d.getUTCDate() + dias);
  return d.toISOString().slice(0, 10);
}

/** E-03 Retención: propone una oferta cuando el jugador muestra que se quiere ir. */
class RetencionService {
  constructor(repo, config = CONFIG, hoy = () => '2026-10-26') {
    this.repo = repo;
    this.config = config;
    this.hoy = hoy;
    this.secuencia = 1022;
  }

  /** Descuento sobre una compra de fichas: porcentaje con tope. No guarda nada si el monto es inválido. */
  async aplicarDescuento(documento, monto) {
    if (!(monto > 0)) throw new ReglaNegocioError('Monto inválido');
    const { porcentajeDescuento, topeDescuento } = this.config;
    const descuento = Math.min(Math.round((monto * porcentajeDescuento) / 100), topeDescuento);
    await this.repo.save({ tipo: 'DESCUENTO_FICHAS', documento, monto, descuento, fecha: this.hoy() });
    return descuento;
  }

  ofertaSugerida(jugador) {
    return TIPO_POR_NIVEL[nivelPorPuntos(jugador.puntos || 0)];
  }

  async evaluarSenal({ jugador, minutosSesion, senal }) {
    if (!jugador) throw new ReglaNegocioError('Jugador sin perfil registrado');
    if (jugador.estado !== 'ACTIVO') throw new ReglaNegocioError('No se ofrece retención: juego responsable');
    if (!(minutosSesion >= this.config.minutosMinimos)) {
      throw new ReglaNegocioError(`La sesión debe durar al menos ${this.config.minutosMinimos} minutos`);
    }
    const hoy = this.hoy();
    if ((await this.repo.countOffers(jugador.documento, hoy)) >= this.config.ofertasPorDia) {
      throw new ReglaNegocioError('El jugador ya recibió una oferta hoy');
    }
    const tipo = this.ofertaSugerida(jugador);
    const valor = { DESCUENTO_FICHAS: this.config.porcentajeDescuento, FICHAS_BONO: this.config.fichasBono, BONO_RECLAMABLE: this.config.bonoReclamable }[tipo];
    return this.repo.save({
      codigo: `RET-${this.secuencia++}`,
      documento: jugador.documento,
      senal,
      tipo,
      valor,
      fecha: hoy,
      vence: sumarDias(hoy, this.config.diasVigencia),
      estado: 'EMITIDA',
    });
  }
}

module.exports = { RetencionService, CONFIG, MENSAJES, nivelPorPuntos };
