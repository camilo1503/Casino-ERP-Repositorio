const assert = require('node:assert/strict');
const { Given, When, Then } = require('@cucumber/cucumber');
const { MENSAJES } = require('../src/perfiles/jugadorService');

Given('que no existe un jugador con documento {string}', async function (documento) {
  assert.equal(await this.repoJugadores.existsByDocument(documento), false);
});

Given('que ya existe un jugador con documento {string}', async function (documento) {
  await this.repoJugadores.save({ documento, nombre: 'María Fernanda López', correo: 'maria.lopez@correo.com', fechaNacimiento: '1990-05-14', puntos: 8450, estado: 'ACTIVO' });
});

When('registro al jugador con documento {string}, nombre {string}, correo {string} y fecha de nacimiento {string}', async function (documento, nombre, correo, fechaNacimiento) {
  this.jugador = await this.intentar(() => this.jugadores.registrar({ documento, nombre, correo, fechaNacimiento }), MENSAJES.REGISTRADO);
});

When('registro un jugador con correo {string}', async function (correo) {
  await this.intentar(() => this.jugadores.registrar({ documento: '1030000001', nombre: 'Jugador de Prueba', correo, fechaNacimiento: '1995-01-01' }), 'Correo válido');
});

When('desactivo al jugador con documento {string} por el motivo {string}', async function (documento, motivo) {
  await this.intentar(() => this.jugadores.desactivar(documento, motivo), MENSAJES.DESACTIVADO);
});

Then('el jugador se crea con estado {string}', function (estado) {
  assert.equal(this.jugador.estado, estado);
});

Then('el jugador con documento {string} queda en estado {string}', async function (documento, estado) {
  assert.equal((await this.repoJugadores.findByDocument(documento)).estado, estado);
});

Then('el jugador con documento {string} conserva el nombre {string}', async function (documento, nombre) {
  assert.equal((await this.repoJugadores.findByDocument(documento)).nombre, nombre);
});
