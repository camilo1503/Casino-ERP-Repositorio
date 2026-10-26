const { ReglaNegocioError } = require('../errores');

const CORREO = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
const DOCUMENTO = /^\d{6,12}$/;
const MENSAJES = {
  REGISTRADO: 'Jugador registrado exitosamente',
  ACTUALIZADO: 'Perfil actualizado',
  DESACTIVADO: 'Jugador desactivado',
  AUTOEXCLUIDO: 'Jugador autoexcluido',
};

/** Calcula la edad cumplida en una fecha de referencia (formato AAAA-MM-DD). */
function edad(fechaNacimiento, hoy) {
  const [a, m, d] = fechaNacimiento.split('-').map(Number);
  const [ah, mh, dh] = hoy.split('-').map(Number);
  let anios = ah - a;
  if (mh < m || (mh === m && dh < d)) anios -= 1;
  return anios;
}

/** E-01 Gestión de Perfiles de Jugador: CRUD con borrado lógico. */
class JugadorService {
  constructor(repo, hoy = () => '2026-10-26') {
    this.repo = repo;
    this.hoy = hoy;
  }

  validarCorreo(correo) {
    if (!CORREO.test(correo || '')) throw new ReglaNegocioError('Formato de correo inválido');
  }

  async registrar({ documento, nombre, correo, fechaNacimiento }) {
    if (!nombre || !nombre.trim()) throw new ReglaNegocioError('El nombre es obligatorio');
    if (!DOCUMENTO.test(documento || '')) throw new ReglaNegocioError('El documento no es válido');
    this.validarCorreo(correo);
    if (!/^\d{4}-\d{2}-\d{2}$/.test(fechaNacimiento || '')) throw new ReglaNegocioError('La fecha de nacimiento es obligatoria');
    if (await this.repo.existsByDocument(documento)) throw new ReglaNegocioError('El documento ya está registrado');
    if (edad(fechaNacimiento, this.hoy()) < 18) throw new ReglaNegocioError('El jugador debe ser mayor de 18 años');
    return this.repo.save({
      documento,
      nombre: nombre.trim(),
      correo: correo.trim().toLowerCase(),
      fechaNacimiento,
      puntos: 0,
      estado: 'ACTIVO',
    });
  }

  async consultar(documento) {
    const jugador = await this.repo.findByDocument(documento);
    if (!jugador) throw new ReglaNegocioError('Jugador no encontrado');
    return jugador;
  }

  async actualizar(documento, cambios) {
    const jugador = await this.consultar(documento);
    if (cambios.documento && cambios.documento !== documento) {
      throw new ReglaNegocioError('El documento no se puede modificar');
    }
    if (cambios.correo !== undefined) this.validarCorreo(cambios.correo);
    return this.repo.save({ ...jugador, ...cambios, documento });
  }

  async desactivar(documento, motivo) {
    return this.cambiarEstado(documento, motivo, 'INACTIVO');
  }

  async autoexcluir(documento, motivo) {
    return this.cambiarEstado(documento, motivo, 'AUTOEXCLUIDO');
  }

  async cambiarEstado(documento, motivo, estado) {
    if (!motivo || !motivo.trim()) throw new ReglaNegocioError('El motivo es obligatorio');
    const jugador = await this.consultar(documento);
    return this.repo.save({ ...jugador, estado, motivoEstado: motivo.trim() });
  }
}

module.exports = { JugadorService, MENSAJES, edad };
