const { ReglaNegocioError, StockInsuficienteError } = require('../errores');

const CODIGO = /^FICHA-[0-9]+[KM]?$/;
const MENSAJES = { REGISTRADA: 'Ficha registrada', RETIRO: 'Retiro exitoso', ENTRADA: 'Entrada registrada' };

/** Inventario de fichas de la bóveda: registro por denominación, entradas y retiros. */
class InventarioFichasService {
  constructor(repo) {
    this.repo = repo;
  }

  async registrar({ codigo, denominacion, categoria, stockInicial }) {
    if (!CODIGO.test(codigo || '')) throw new ReglaNegocioError('El código de ficha no es válido');
    if (!(denominacion > 0)) throw new ReglaNegocioError('La denominación debe ser mayor que cero');
    if (!categoria || !categoria.trim()) throw new ReglaNegocioError('La categoría es obligatoria');
    if (!Number.isInteger(stockInicial) || stockInicial < 0) throw new ReglaNegocioError('El stock inicial no puede ser negativo');
    if (await this.repo.existsByCode(codigo)) throw new ReglaNegocioError('El código de ficha ya está registrado');
    return this.repo.save({ codigo, denominacion, categoria: categoria.trim(), stock: stockInicial });
  }

  async consultar(codigo) {
    const ficha = await this.repo.findByCode(codigo);
    if (!ficha) throw new ReglaNegocioError('Ficha no encontrada');
    return ficha;
  }

  async registrarEntrada(codigo, cantidad) {
    if (!Number.isInteger(cantidad) || cantidad <= 0) throw new ReglaNegocioError('La cantidad debe ser mayor que cero');
    const ficha = await this.consultar(codigo);
    const actualizada = await this.repo.save({ ...ficha, stock: ficha.stock + cantidad });
    await this.repo.addMovement({ codigo, tipo: 'ENTRADA', cantidad, stockFinal: actualizada.stock });
    return actualizada;
  }

  async retirar(codigo, cantidad) {
    if (!Number.isInteger(cantidad) || cantidad <= 0) throw new ReglaNegocioError('La cantidad debe ser mayor que cero');
    const ficha = await this.consultar(codigo);
    if (cantidad > ficha.stock) throw new StockInsuficienteError();
    const actualizada = await this.repo.save({ ...ficha, stock: ficha.stock - cantidad });
    await this.repo.addMovement({ codigo, tipo: 'SALIDA', cantidad, stockFinal: actualizada.stock });
    return actualizada;
  }
}

module.exports = { InventarioFichasService, MENSAJES };
