const { ReglaNegocioError } = require('../errores');

/** Operación de mesas: aprobación de rellenos de fichas contra el saldo de la bóveda. */
class RellenoService {
  constructor(repo) {
    this.repo = repo;
  }

  async aprobar(id, aprobadoPor = 'Jefe de piso') {
    const relleno = await this.repo.findById(id);
    if (!relleno) throw new ReglaNegocioError('Relleno no encontrado');
    if (relleno.estado === 'APROBADO') throw new ReglaNegocioError('El relleno ya está aprobado');
    if (relleno.estado !== 'PENDIENTE') throw new ReglaNegocioError('Solo se aprueban rellenos pendientes');
    const saldo = await this.repo.getVaultBalance();
    if (relleno.monto > saldo) throw new ReglaNegocioError('Saldo insuficiente en la bóveda');
    await this.repo.updateVaultBalance(saldo - relleno.monto);
    return this.repo.save({ ...relleno, estado: 'APROBADO', aprobadoPor });
  }
}

module.exports = { RellenoService };
