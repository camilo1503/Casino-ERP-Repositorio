const assert = require('node:assert/strict');
const { When, Then } = require('@cucumber/cucumber');
const { MENSAJES } = require('../src/caja/compraFichasService');

When('registro una compra de fichas por {int} pesos para el jugador {string}', async function (monto, documento) {
  this.compra = await this.intentar(() => this.caja.registrarCompra(documento, monto), (c) => MENSAJES[c.estado]);
});

Then('la compra queda en estado {string}', function (estado) {
  assert.equal(this.compra.estado, estado);
});

Then('no se registra ninguna compra', function () {
  assert.equal(this.repoCompras.datos.length, 0);
});
