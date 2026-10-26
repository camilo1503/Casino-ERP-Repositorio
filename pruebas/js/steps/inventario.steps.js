const assert = require('node:assert/strict');
const { Given, When, Then } = require('@cucumber/cucumber');
const { MENSAJES } = require('../src/inventario/inventarioFichasService');

Given('que no existe una ficha con código {string}', async function (codigo) {
  assert.equal(await this.repoFichas.existsByCode(codigo), false);
});

Given('que la ficha {string} tiene stock {int}', async function (codigo, stock) {
  await this.repoFichas.save({ codigo, denominacion: 25000, categoria: 'Verde', stock });
  this.codigo = codigo;
});

When('registro la ficha {string} de {int} pesos, categoría {string} y stock inicial {int}', async function (codigo, denominacion, categoria, stockInicial) {
  this.codigo = codigo;
  await this.intentar(() => this.inventario.registrar({ codigo, denominacion, categoria, stockInicial }), MENSAJES.REGISTRADA);
});

When('se registra una entrada de {int} fichas', async function (cantidad) {
  await this.intentar(() => this.inventario.registrarEntrada(this.codigo, cantidad), MENSAJES.ENTRADA);
});

When('se retiran {int} fichas', async function (cantidad) {
  await this.intentar(() => this.inventario.retirar(this.codigo, cantidad), MENSAJES.RETIRO);
});

Then('el stock final es {int}', async function (stock) {
  assert.equal((await this.repoFichas.findByCode(this.codigo)).stock, stock);
});

Then('el movimiento queda registrado en el historial', function () {
  const ultimo = this.repoFichas.movimientos.at(-1);
  assert.ok(ultimo, 'no hay movimientos');
  assert.equal(ultimo.codigo, this.codigo);
});
