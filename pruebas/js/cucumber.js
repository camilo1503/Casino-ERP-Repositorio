// Configuración de Cucumber-JS: usa los mismos archivos .feature que Cucumber-JVM y Reqnroll.
const base = { paths: ['../features/*.feature'], require: ['steps/**/*.js'] };

module.exports = {
  // npx cucumber-js            -> barra de progreso + reportes JSON y HTML
  default: { ...base, format: ['progress-bar', 'json:reports/cucumber.json', 'html:reports/cucumber.html'] },
  // npx cucumber-js -p detalle -> cada escenario y paso en la consola
  detalle: { ...base, format: ['@cucumber/pretty-formatter'] },
};
