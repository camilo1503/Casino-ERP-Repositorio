const assert = require('node:assert/strict');
const { Given, When, Then } = require('@cucumber/cucumber');
const { MENSAJES } = require('../src/mesas/mesaService');

Given('que no existe una mesa con número {string}', async function (numero) {
  assert.equal(await this.repoMesas.existsByNumber(numero), false);
});

Given('que existe la mesa {string} en estado {string}', async function (numero, estado) {
  await this.repoMesas.save({ numero, juego: 'Blackjack', apuestaMinima: 25000, apuestaMaxima: 500000, estado });
});

When('registro la mesa {string} de {string} con apuesta mínima {int} y máxima {int}', async function (numero, juego, apuestaMinima, apuestaMaxima) {
  await this.intentar(() => this.mesas.registrar({ numero, juego, apuestaMinima, apuestaMaxima }), MENSAJES.REGISTRADA);
});

When('actualizo los límites de la mesa {string} a mínima {int} y máxima {int}', async function (numero, min, max) {
  await this.intentar(() => this.mesas.actualizarLimites(numero, min, max), MENSAJES.ACTUALIZADA);
});

When('solicito retirar la mesa {string} por el motivo {string}', async function (numero, motivo) {
  await this.intentar(() => this.mesas.retirar(numero, motivo), MENSAJES.RETIRADA);
});

Then('la mesa {string} queda en estado {string}', async function (numero, estado) {
  assert.equal((await this.repoMesas.findByNumber(numero)).estado, estado);
});
