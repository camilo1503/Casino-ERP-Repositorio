// Contexto compartido entre pasos: un "mundo" nuevo por escenario, con repositorios en memoria.
const { setWorldConstructor } = require('@cucumber/cucumber');
const { ReglaNegocioError } = require('../../src/errores');
const repos = require('../../src/repos/memoria');
const { JugadorService } = require('../../src/perfiles/jugadorService');
const { MesaService } = require('../../src/mesas/mesaService');
const { CompraFichasService } = require('../../src/caja/compraFichasService');
const { RetencionService } = require('../../src/retencion/retencionService');
const { RellenoService } = require('../../src/rellenos/rellenoService');
const { InventarioFichasService } = require('../../src/inventario/inventarioFichasService');

class Mundo {
  constructor() {
    this.repoJugadores = new repos.RepoJugadores();
    this.repoMesas = new repos.RepoMesas();
    this.repoCompras = new repos.RepoCompras();
    this.repoOfertas = new repos.RepoOfertas();
    this.repoRellenos = new repos.RepoRellenos();
    this.repoFichas = new repos.RepoFichas();
    this.jugadores = new JugadorService(this.repoJugadores);
    this.mesas = new MesaService(this.repoMesas);
    this.caja = new CompraFichasService(this.repoCompras);
    this.retencion = new RetencionService(this.repoOfertas);
    this.rellenos = new RellenoService(this.repoRellenos);
    this.inventario = new InventarioFichasService(this.repoFichas);
    this.mensaje = undefined;
  }

  /** Ejecuta una acción del ERP y guarda el mensaje que vería el usuario (éxito o regla incumplida). */
  async intentar(accion, mensajeExito) {
    try {
      const resultado = await accion();
      this.mensaje = typeof mensajeExito === 'function' ? mensajeExito(resultado) : mensajeExito;
      return resultado;
    } catch (e) {
      if (!(e instanceof ReglaNegocioError)) throw e;
      this.mensaje = e.message;
      return undefined;
    }
  }
}

setWorldConstructor(Mundo);
