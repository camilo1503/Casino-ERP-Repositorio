# Guion del video de arc42 (Tarea 2 de la guía, máximo 60 s)

| Tiempo | Qué se muestra | Qué se dice |
|---|---|---|
| 0–15 s | Sección 5: `05-bloques/nomina.md`, el bloque Nómina y su frontera | El módulo de nómina está aislado: expone sus endpoints y solo depende de `IEmpleadoRepositorio` y de su persistencia. |
| 15–40 s | Sección 10: la matriz de roles y `[AutorizarModulo(Modulos.Rrhh)]` | Solo el rol con acceso a RR. HH. puede llamar a `NominaController`; el resto recibe 403. |
| 40–60 s | Las dos secciones lado a lado | Cada control de la sección 10 tiene su lugar en la sección 5: el atributo está en el controlador del bloque Nómina. Las dos secciones son coherentes. |

**Quién graba:** _por definir._ **Enlace del video:** _pendiente._

## Resultado de la revisión cruzada (J7.4)

### Bloque Nómina (Felipe), revisado el 2026-11-18

Revisado `05-bloques/nomina.md` y `ADR-005` contra el código y las pruebas de `develop` (`04e37c9`):

- Cumple el contenido de F8.1: frontera, diagrama de clases, SOLID, diagrama de secuencia con las ramas de error y tabla de reglas con ejemplos.
- Coinciden con el código: los registros de DI de `ModuloNomina`, las rutas y los verbos del controlador, los constructores de `PayrollEngine` y `NominaService`, y las cantidades de la tabla de pruebas (28, 18, 14, 18 y 7 + 9, ejecutadas con `dotnet test`).
- Cálculos verificados a mano: Ana Torres, Bruno Lara, jefe de piso, cajero, salario alto, prestación de servicios, los límites de 2 y 4 SMMLV y el redondeo.
- El control de acceso (`[AutorizarModulo(Modulos.Rrhh)]` en `NominaController`) figura en el texto y en el diagrama de secuencia como pendiente de F7. Coincide con la tabla de C5 de `contratos.md`, pero **el atributo aún no existe en el código**.
- Las citas legales del ADR-005 (Decretos 1469 y 1470 de 2025 y 0159 de 2026) vienen de D-003 y no se verificaron contra fuentes oficiales.
- Detalle menor: el encabezado dice `5.x`; conviene numerarlo cuando estén los demás bloques.

### Sección 10 (Iván) frente al bloque Nómina, revisado el 2026-11-18

Con la seguridad de Iván ya en `develop` (`cce0aa1`) y su arc42 (`2932d93`):

- `10-conceptos-seguridad.md` lista `NominaController` → módulo `rrhh` → roles Gerente y Talento humano, y `EmpleadosController` → `competencias`, `rrhh`, `mesas`. El código coincide: `NominaController` lleva `[AutorizarModulo(Modulos.Rrhh)]` y `EmpleadosController` lleva `[AutorizarModulo(Modulos.Competencias, Modulos.Rrhh, Modulos.Mesas)]`.
- El bloque Nómina (Felipe) dice que solo Gerente y Talento Humano calculan o aprueban la nómina. Es coherente con la sección 10.
- Detalle para Felipe: `05-bloques/nomina.md` todavía dice que los atributos «se aplican cuando la seguridad (I5) está en develop». Ya están, así que esa frase puede pasar a presente.

### Pendiente

Falta el bloque de Camilo (`05-bloques/inventario-compras.md`, PR #8 todavía abierto) para comparar el control de `InventarioController` (`[AutorizarModulo(Modulos.Inventario, Modulos.Caja)]`, ya en el código) con su bloque. Las incoherencias se anotarán como comentarios en el PR de su dueño.
