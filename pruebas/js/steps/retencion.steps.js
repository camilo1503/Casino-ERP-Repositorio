const assert = require('node:assert/strict');
const { Given, When, Then } = require('@cucumber/cucumber');
const { MENSAJES } = require('../src/retencion/retencionService');
const { pesos } = require('../src/dinero');

// Puntos de lealtad de los jugadores de ejemplo del prototipo
const PUNTOS = { 1020456789: 8450, 1018334560: 3200, 80554219: 2700 };

async function asegurarJugador(mundo, documento, estado = 'ACTIVO') {
  const jugador = { documento, nombre: `Jugador ${documento}`, puntos: PUNTOS[documento] || 1000, estado };
  await mundo.repoJugadores.save(jugador);
  mundo.documento = documento;
}

Given('que el jugador {string} tiene una sesión activa de {int} minutos', async function (documento, minutos) {
  await asegurarJugador(this, documento);
  this.minutosSesion = minutos;
});

Given('que el jugador {string} está en estado {string}', async function (documento, estado) {
  await asegurarJugador(this, documento, estado);
});

Given('tiene una sesión activa de {int} minutos', function (minutos) {
  this.minutosSesion = minutos;
});

Given('ya recibió una oferta de retención hoy', async function () {
  await this.repoOfertas.save({ codigo: 'RET-1021', documento: this.documento, tipo: 'FICHAS_BONO', fecha: this.retencion.hoy(), estado: 'EMITIDA' });
});

When('aplico el descuento de retención a una compra de {int} pesos', async function (monto) {
  await this.intentar(() => this.retencion.aplicarDescuento(this.documento, monto), (d) => `Descuento de ${pesos(d)}`);
});

When('el crupier registra la señal {string}', async function (senal) {
  const jugador = await this.repoJugadores.findByDocument(this.documento);
  this.oferta = await this.intentar(() => this.retencion.evaluarSenal({ jugador, minutosSesion: this.minutosSesion, senal }), MENSAJES.EMITIDA);
});

Then('se emite una oferta de tipo {string}', function (tipo) {
  assert.equal(this.oferta.tipo, tipo);
});

Then('no se emite ninguna oferta', function () {
  assert.equal(this.oferta, undefined);
  assert.equal(this.repoOfertas.datos.filter((o) => o.documento === this.documento).length, 0);
});
