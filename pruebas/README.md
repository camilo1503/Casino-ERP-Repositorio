# PitBoss ERP · Pruebas para CRUD empresariales

Taller técnico de Gherkin, JUnit 5, Jest, Cucumber, SpecFlow/Reqnroll y Font Awesome aplicado al ERP del casino.
Ingeniería de Software II · Universidad de Cundinamarca, sede Chía.

## Integrantes y responsabilidades

| Integrante | Parte del taller | Módulo del ERP |
| --- | --- | --- |
| Daniel Camilo Ruiz Rodríguez | Gherkin (plantilla CRUD), Ejercicio 1, Taller 1 | Perfiles de jugador y catálogo de mesas |
| Jorge Alcides Díaz Lozano | Jest (Ejercicio 3), Taller 2 | Retención de jugadores |
| Iván Felipe León Pinilla | JUnit 5 (Ejercicio 2), Cucumber (Ejercicio 4), Taller 3 | Caja y rellenos de fichas |
| Felipe Antonio Cortés Hernández | SpecFlow/Reqnroll (Ejercicio 5), Font Awesome (Ejercicio 6), Taller 4 | Inventario de fichas |

## Equivalencia entre el taller y el ERP

| Taller (guía) | En PitBoss ERP | Regla equivalente |
| --- | --- | --- |
| CRUD de proveedores | Perfiles de jugador / catálogo de mesas | Documento o número de mesa único; no se retira una mesa abierta |
| Solicitud de cotizaciones | Ofertas de retención | Monto > 0; solo jugadores activos con sesión ≥ 30 min; una oferta por día |
| Aprobación de órdenes de compra | Aprobación de rellenos de fichas | El relleno no supera el saldo de la bóveda; no se re-aprueba; aprobar descuenta la bóveda |
| Inventario de productos | Inventario de fichas de la bóveda | Código único, stock inicial ≥ 0, categoría obligatoria, no se retira más del stock |

## Estructura

```
features/      6 archivos .feature en español (los usan Cucumber-JS, Cucumber-JVM y Reqnroll)
js/            servicios Node.js + pruebas Jest + step definitions de Cucumber-JS
java/          proyecto Maven: dominio Java + JUnit 5 + step definitions de Cucumber-JVM
dotnet/        proyecto C# con Reqnroll (sucesor de SpecFlow) + FluentAssertions
reporte/       generador del reporte HTML con Font Awesome (Ejercicio 6)
evidencias/    resultados de las ejecuciones (JSON, XML, salidas de consola y capturas)
```

## Cómo ejecutar

```bash
# Jest y Cucumber-JS (Node 18 o superior)
cd js
npm install
npm run test:coverage        # 63 pruebas Jest + cobertura
npm run test:bdd:detalle     # 37 escenarios Gherkin

# JUnit 5 y Cucumber-JVM (JDK 17 o superior y Maven)
cd java
mvn test                     # JUnit 5 + RunCucumberTest (mismos .feature)

# Reqnroll en .NET 8
cd dotnet/PitBoss.Specs
dotnet test

# Reporte con Font Awesome (desde la carpeta raíz)
python reporte/generar_reporte.py
```

## Resultados

- Jest: 63 pruebas aprobadas; cobertura de líneas 97,9 % y de ramas 88,2 %.
- Cucumber-JS: 37 escenarios y 166 pasos aprobados.
- JUnit 5 (5.10, consola de JUnit Platform): 46 pruebas aprobadas.
- `mvn test` (Maven 3.9.9, Java 25, Windows): 83 pruebas, 0 fallos, BUILD SUCCESS. Son las 46 de JUnit 5 más los 37 escenarios de Cucumber-JVM (`RunCucumberTest`). JaCoCo: líneas 95,4 %, ramas 76,9 %.
- `dotnet test` (Reqnroll 2.4 + NUnit, .NET SDK 9.0.200 sobre net8.0): 2 de 2 escenarios aprobados.
- Reporte Font Awesome: 185 pruebas, 185 aprobadas, 0 fallidas, 0 pendientes.
- Para repetir todo en Windows: `ejecutar_pruebas.bat` (descarga Maven si no está instalado y deja las salidas en `evidencias/`).
