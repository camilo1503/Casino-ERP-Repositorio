const { ReglaNegocioError } = require('../errores');

const NUMERO = /^M-\d{2}$/;
const JUEGOS = ['Blackjack', 'Ruleta', 'Póker', 'Baccarat', 'Craps'];
const MENSAJES = {
  REGISTRADA: 'Mesa registrada exitosamente',
  ACTUALIZADA: 'Mesa actualizada',
  RETIRADA: 'Mesa retirada del catálogo',
};

/** E-02 Catálogo de mesas: registrar, consultar, actualizar y retirar (borrado lógico). */
class MesaService {
  constructor(repo) {
    this.repo = repo;
  }

  validarLimites(apuestaMinima, apuestaMaxima) {
    if (!(apuestaMinima > 0)) throw new ReglaNegocioError('La apuesta mínima debe ser mayor que cero');
    if (apuestaMinima >= apuestaMaxima) throw new ReglaNegocioError('La apuesta mínima debe ser menor que la máxima');
  }

  async registrar({ numero, juego, apuestaMinima, apuestaMaxima }) {
    if (!NUMERO.test(numero || '')) throw new ReglaNegocioError('El número de mesa debe tener el formato M-00');
    if (!JUEGOS.includes(juego)) throw new ReglaNegocioError('El juego es obligatorio');
    this.validarLimites(apuestaMinima, apuestaMaxima);
    if (await this.repo.existsByNumber(numero)) throw new ReglaNegocioError('El número de mesa ya está registrado');
    return this.repo.save({ numero, juego, apuestaMinima, apuestaMaxima, estado: 'CERRADA' });
  }

  async consultar(numero) {
    const mesa = await this.repo.findByNumber(numero);
    if (!mesa) throw new ReglaNegocioError('Mesa no encontrada');
    return mesa;
  }

  async actualizarLimites(numero, apuestaMinima, apuestaMaxima) {
    const mesa = await this.consultar(numero);
    if (mesa.estado === 'RETIRADA') throw new ReglaNegocioError('La mesa está retirada');
    this.validarLimites(apuestaMinima, apuestaMaxima);
    return this.repo.save({ ...mesa, apuestaMinima, apuestaMaxima });
  }

  async retirar(numero, motivo) {
    const mesa = await this.consultar(numero);
    if (mesa.estado === 'ABIERTA') throw new ReglaNegocioError('No se puede retirar una mesa abierta');
    if (!motivo || !motivo.trim()) throw new ReglaNegocioError('El motivo es obligatorio');
    return this.repo.save({ ...mesa, estado: 'RETIRADA', motivoRetiro: motivo.trim() });
  }
}

module.exports = { MesaService, MENSAJES, JUEGOS };
