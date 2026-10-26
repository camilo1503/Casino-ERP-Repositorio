/** Formatea pesos colombianos: 200000 -> "$200.000". */
const pesos = (n) => '$' + String(Math.round(Math.abs(n))).replace(/\B(?=(\d{3})+(?!\d))/g, '.');
module.exports = { pesos };
