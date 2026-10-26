const { ReglaNegocioError } = require('../errores');

const DENOMINACIONES = [5000, 10000, 25000, 50000, 100000, 500000];
const UMBRAL_CUMPLIMIENTO = 5000000;
const MENSAJES = {
  APROBADA: 'Compra registrada',
  EN_REVISION: 'Compra enviada a revisión de cumplimiento',
};

/** Caja / POS: total por denominación y registro de compras de fichas. */
class CompraFichasService {
  constructor(repo) {
    this.repo = repo;
    this.secuencia = 10483;
  }

  calcularTotal(denominacion, cantidad) {
    if (!DENOMINACIONES.includes(denominacion)) throw new ReglaNegocioError('Denominación no válida');
    if (!Number.isInteger(cantidad) || cantidad <= 0) throw new ReglaNegocioError('La cantidad debe ser mayor que cero');
    return denominacion * cantidad;
  }

  async registrarCompra(documento, monto) {
    if (!(monto > 0)) throw new ReglaNegocioError('El monto debe ser mayor que cero');
    const estado = monto >= UMBRAL_CUMPLIMIENTO ? 'EN_REVISION' : 'APROBADA';
    return this.repo.save({ id: `TX-${this.secuencia++}`, documento, monto, estado });
  }
}

module.exports = { CompraFichasService, MENSAJES, DENOMINACIONES, UMBRAL_CUMPLIMIENTO };
