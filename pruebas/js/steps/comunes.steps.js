const assert = require('node:assert/strict');
const { Given, Then } = require('@cucumber/cucumber');

Given('que el usuario {string} con rol {string} ha iniciado sesión', function (nombre, rol) {
  this.usuario = { nombre, rol };
});

const verificarMensaje = function (esperado) {
  assert.equal(this.mensaje, esperado);
};
Then('el sistema muestra el mensaje {string}', verificarMensaje);
Then('el sistema confirma {string}', verificarMensaje);
Then('el sistema responde {string}', verificarMensaje);
