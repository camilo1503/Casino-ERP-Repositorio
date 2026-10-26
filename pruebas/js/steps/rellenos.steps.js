const assert = require('node:assert/strict');
const { Given, When, Then } = require('@cucumber/cucumber');

Given('que la bóveda tiene un saldo disponible de {int} pesos', async function (saldo) {
  await this.repoRellenos.updateVaultBalance(saldo);
});

Given('existe un relleno {string} para la mesa {string} por {int} pesos en estado {string}', async function (id, mesa, monto, estado) {
  await this.repoRellenos.save({ id, mesa, monto, estado });
});

When('el jefe de piso aprueba el relleno {string}', async function (id) {
  await this.intentar(() => this.rellenos.aprobar(id, this.usuario.nombre), 'Relleno aprobado');
});

Then('el relleno {string} queda en estado {string}', async function (id, estado) {
  assert.equal((await this.repoRellenos.findById(id)).estado, estado);
});

Then('el saldo de la bóveda queda en {int} pesos', async function (saldo) {
  assert.equal(await this.repoRellenos.getVaultBalance(), saldo);
});
