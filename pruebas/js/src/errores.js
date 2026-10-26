/** Error de regla de negocio: el mensaje es el que ve el usuario en el ERP. */
class ReglaNegocioError extends Error {
  constructor(mensaje) {
    super(mensaje);
    this.name = 'ReglaNegocioError';
  }
}

/** Se lanza cuando un retiro supera el stock disponible (Taller 4 / Ejercicio 5). */
class StockInsuficienteError extends ReglaNegocioError {
  constructor() {
    super('Stock insuficiente');
    this.name = 'StockInsuficienteError';
  }
}

module.exports = { ReglaNegocioError, StockInsuficienteError };
