# Bitácora · Felipe · Plan-Felipe

> Solo la edita Felipe (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| F0 | Arranque | Hecha (falta mover Jira) | — |
| F1 | Dominio de empleados (contrato C4) | Hecha (fusionada en `develop`, `f083b68`) | Fusión del integrador (D-009) |
| F2 | Parámetros y reglas de liquidación | Hecha (fusionada en `develop`, `5ce30c7`) | Fusión del integrador (D-009) |
| F3 | PayrollEngine y NominaService | Hecha (fusionada en `develop`, `5ce30c7`) | Fusión del integrador (D-009) |
| F4 | Persistencia y API de empleados | En revisión | [comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-empleados-api) |
| F5 | Persistencia y API de nómina | En revisión | [comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-nomina-api) |
| F6 | Front personal.js | Pendiente | — |
| F7 | Autorización de mis controladores | Pendiente | — |
| F8 | arc42 bloque Nómina y cierre | Pendiente | — |

## Pendientes abiertos
- F0.3: mover HU-04 y la subtarea F1 a "En curso" en Jira (lo hace Felipe; el agente no tiene acceso a Jira).
- ~~F1.5: abrir el PR de F1~~ (resuelto: el integrador la fusionó en `develop`, D-009).
- ~~Comando de cobertura de AGENT.md §10 sin `coverlet.msbuild`~~ (resuelto: la Fase 0 agregó el paquete).
- La rama `feature/HU-04-reglas-liquidacion` quedó obsoleta (F2 ya entró por `feature/HU-04-nomina-motor`). No se fusiona; Felipe o Jorge pueden borrarla.
- Jira: mover CER-22, CER-23 (F2, F3) a "Listo" y CER-24 y CER-25 (F4, F5) a "En revisión".
- `feature/HU-04-nomina-api` (F5) se tomó de `feature/HU-04-empleados-api` (F4) porque F4 aún no está fusionada: conviene fusionar F4 antes que F5.

## Bloqueos abiertos
- ~~F4, F5 y F6 esperan la Fase 0 de Jorge~~ (resuelto: Fase 0 en `develop` desde `8fff1ce`, D-007).
- F7 espera la I5 de Iván ("seguridad activa"). Aún no está en `develop`.

## Entradas

## 2026-11-18 20:22 · Tarea F5 · Terminada (en revisión)
**Hecho:**
- Rama `feature/HU-04-nomina-api`, tomada de la rama de F4 (F4 aún no está fusionada en `develop`; F5 necesita los empleados de su semilla).
- `src/CasinoErp.Infraestructura/Configuraciones/NominaConfiguracion.cs`: `NominaConfiguracion` (tabla `Nominas`, índice único `Anio`+`Mes`, detalle en cascada, totales ignorados), `LiquidacionEmpleadoConfiguracion` (tabla `LiquidacionesNomina`, enums como texto) y `ParametrosNominaConfiguracion` (índice único por año, tasas con precisión 7,5).
- `src/CasinoErp.Infraestructura/Repositorios/NominaRepositorios.cs`: `NominaRepositorio` (incluye el detalle; lista por año y mes descendentes) y `ParametrosNominaRepositorio` (guardar es *upsert* por año).
- `src/CasinoErp.Infraestructura/Semillas/SemillaNomina.cs` (`Orden = 20`): `ParametrosNomina.Decreto2026()` si la tabla está vacía.
- `src/CasinoErp.Infraestructura/Modulos/ModuloNomina.cs`: reglas y `PayrollEngine` como singleton, repositorios, `NominaService` y la semilla.
- `src/CasinoErp.Api/Controllers/NominaController.cs`: `POST /api/nomina/calculate`, `GET /api/nomina`, `GET /api/nomina/{anio}/{mes}`, `POST /api/nomina/{id}/aprobar`, `GET` y `PUT /api/nomina/parametros/{anio}` (el año de la ruta manda).
- `tests/CasinoErp.PruebasIntegracion/PersonalNominaApiTests.cs`: clase nueva `NominaApiTests` con 9 pruebas (las 4 del plan más totales en el JSON, recalcular un borrador, 404, guardar parámetros de 2027 y parámetros inválidos).
- TDD: primero las pruebas (fallaban las 9 por rutas inexistentes).
**Evidencia:**
- `dotnet test tests/CasinoErp.PruebasIntegracion --filter "FullyQualifiedName~NominaApiTests"` → 16 aprobadas (9 de nómina + 7 de empleados), 0 fallos.
- `dotnet test CasinoErp.sln` → 244 aprobadas (210 unitarias + 34 de integración), 0 fallos. `dotnet build` → 0 advertencias. Cobertura oficial del dominio: 97,35 % de líneas.
- Prueba manual con `BaseDatos:RecrearAlIniciar=true`: `POST /api/nomina/calculate` (2026/10) → 200, `BORRADOR`, 18 filas; Bruno Lara recargo 910.000, auxilio 0, neto 3.229.200; Ana Torres auxilio 249.095, neto 2.641.095; total neto 49.074.720, aportes del empleador 14.713.782. Aprobar → 200 `APROBADA`; recalcular → 400 "La nómina de 10/2026 ya está aprobada."; `GET /api/nomina/parametros/2026` → 1.750.905 y 249.095.
**Decisiones:**
- Las pruebas de nómina van en una segunda clase (`NominaApiTests`) del mismo archivo, con su propia base, para que la nómina de octubre tenga exactamente las 18 filas del plan sin depender del orden de las pruebas de empleados (que registran empleados nuevos en la base compartida de su clase).
- El repositorio carga el detalle ordenado por `Id`, que es el orden en que el motor lo generó (por nombre).
**Pendientes:** fusionar F4 antes que F5 (integrador); mover CER-25 en Jira.
**Bloqueos / necesito de otros:** F7 espera la I5 de Iván.
**Siguiente paso:** F6, front `js/api/personal.js`.
**Commits/PR:** ver el commit `HU-04: Persistencia de nomina y POST /api/nomina/calculate`.

## 2026-11-18 20:11 · Tarea F4 · Terminada (en revisión)
**Hecho:**
- Rama `feature/HU-04-empleados-api` tomada de `develop` (con la Fase 0, F1, F2 y F3 ya fusionadas).
- `src/CasinoErp.Infraestructura/Configuraciones/EmpleadoConfiguracion.cs`: tabla `Empleados`, índice único de `Documento`, longitudes del plan y enums como texto.
- `src/CasinoErp.Infraestructura/Repositorios/PersonalRepositorios.cs`: `EmpleadoRepositorio` con `db.Set<Empleado>()`, orden por `Id`.
- `src/CasinoErp.Infraestructura/Semillas/SemillaPersonal.cs` (`Orden = 10`, idempotente): los 18 empleados de `seed().staff` en el mismo orden, con `Documento = 1010200300 + i * 7919`, cargo, juego, turno y área del mockup, salario por cargo, `Indefinido` y ingreso `2025-01-15`.
- `src/CasinoErp.Infraestructura/Modulos/ModuloPersonal.cs`: registra el repositorio, `EmpleadoService` y la semilla.
- `src/CasinoErp.Api/Controllers/EmpleadosController.cs`: `GET /api/empleados?soloActivos=`, `GET /api/empleados/{documento}`, `POST /api/empleados` (201), `PUT /api/empleados/{documento}` y `POST /api/empleados/{documento}/retirar`.
- `tests/CasinoErp.PruebasIntegracion/PersonalNominaApiTests.cs`: 7 pruebas (las 5 del plan, la comparación de la semilla con el mockup y `PUT`). Cliente con `factory.CreateClient()` porque la I5 de Iván aún no está en `develop`.
- TDD: primero las pruebas (fallaban con 404 en las rutas).
**Evidencia:**
- `dotnet test tests/CasinoErp.PruebasIntegracion --filter PersonalNominaApiTests` → 7 aprobadas, 0 fallos.
- `dotnet test CasinoErp.sln` → 235 aprobadas (210 unitarias + 25 de integración), 0 fallos. `dotnet build` → 0 advertencias.
- Cobertura oficial: `CasinoErp.Dominio` 97,35 % de líneas.
- Prueba manual (F4.5): API con `BaseDatos:RecrearAlIniciar=true`; en Swagger, `GET /api/empleados` → 200 con 18 empleados (Ana Torres `1010200300` en M-01, Bruno Lara en `NOCHE`, Daniela Vargas en `Supervisión`). El front cargó en línea (8 jugadores, 19 mesas) sin errores en la consola.
**Decisiones:** ninguna fuera del plan.
**Pendientes:** abrir el PR de F4 hacia `develop` (o dejar la rama para el integrador, D-009); mover CER-24 en Jira.
**Bloqueos / necesito de otros:** F7 espera la I5 de Iván. F5 ya se puede hacer (F3 y F4 listos).
**Siguiente paso:** F5, persistencia de nómina y `POST /api/nomina/calculate`, en `feature/HU-04-nomina-api`.
**Commits/PR:** ver el commit `HU-04: Persistencia, semilla y API REST de empleados`.

## 2026-11-18 20:02 · Tarea F3 · Terminada (PR F2 + F3 pendiente)
**Hecho:**
- `src/CasinoErp.Dominio/Nominas/PayrollEngine.cs`: cálculo puro; valida el mes, toma solo activos, rechaza salarios laborales bajo el mínimo (la prestación de servicios no tiene esa restricción), elige la regla con `Aplica` y lanza `InvalidOperationException` si falta una; devuelve el borrador ordenado por nombre, sin fechas.
- `src/CasinoErp.Dominio/Nominas/NominaService.cs`: `CalcularAsync` (no recalcula meses aprobados, exige parámetros del año, reemplaza el borrador anterior), `AprobarAsync`, `ConsultarAsync`, `ListarAsync`, `ConsultarParametrosAsync` y `GuardarParametrosAsync` (tasas en [0, 1], salario mínimo > 0, auxilio ≥ 0, año ≥ 2020).
- `tests/CasinoErp.PruebasUnitarias/Dobles/NominaEnMemoria.cs` (`ParametrosEnMemoria`, `NominasEnMemoria`), `PayrollEngineTests.cs` (14 pruebas) y `NominaServiceTests.cs` (18 pruebas): las del plan más casos de borde (lista vacía, salario igual al mínimo, término fijo bajo el mínimo, retirado bajo el mínimo, mes con dos dígitos en el mensaje, error del motor que no borra el borrador anterior, tasas negativas, auxilio negativo, año inválido, consultas inexistentes) y una prueba de OCP con una regla nueva inyectada.
- TDD: primero las pruebas (no compilaban: `PayrollEngine` y `NominaService` no existían).
**Evidencia:**
- `dotnet test tests/CasinoErp.PruebasUnitarias --filter "PayrollEngineTests|NominaServiceTests"` → 32 aprobadas, 0 fallos.
- `dotnet test CasinoErp.sln` → 228 aprobadas (210 unitarias + 18 de integración), 0 fallos. `dotnet build` → 0 advertencias.
- Cobertura oficial (AGENT.md §10): `CasinoErp.Dominio` 97,35 % de líneas, 90,66 % de ramas. `PayrollEngine.cs`, `NominaService.cs`, `ReglaContratoLaboral.cs`: 100 % de líneas y de ramas.
**Decisiones:**
- El motor valida el salario mínimo de todos los activos antes de liquidar (el primero que incumpla da el mensaje) y ordena por nombre con `StringComparer.CurrentCulture`.
- `CalcularAsync` ejecuta el motor antes de borrar el borrador anterior: si el cálculo falla, el borrador previo se conserva (hay prueba).
- `GuardarParametrosAsync` valida en el orden del plan: tasas, salario mínimo, auxilio y año.
- Git: al hacer `pull --rebase origin develop` (llegó `321814b` de Jorge) se reescribieron los 3 commits de F2 ya empujados, con el mismo contenido (`git range-diff` sin diferencias). Para no forzar ningún push, F2 + F3 se suben a la rama nueva `feature/HU-04-nomina-motor`. La rama `feature/HU-04-reglas-liquidacion` queda obsoleta (solo tiene F2, ya incluida en la nueva): no se debe fusionar.
**Pendientes:** abrir el PR F2 + F3 desde `feature/HU-04-nomina-motor` hacia `develop` (Felipe, con la descripción que preparó el agente); mover CER-22 y CER-23 en Jira; avisarle a Jorge que `feature/HU-04-reglas-liquidacion` está obsoleta.
**Bloqueos / necesito de otros:** ninguno para F4 (la Fase 0 ya está en `develop`).
**Siguiente paso:** F4, persistencia y API de empleados, en `feature/HU-04-empleados-api` tomada de `develop`.
**Commits/PR:** ver el commit `HU-04: PayrollEngine (calculo puro, SRP/DIP) y NominaService`.

## 2026-11-18 19:56 · Fin de sesión · F1 fusionada, F2 terminada
**Hecho:** al hacer `pull --rebase` de F2 llegaron a `develop` la fusión de F1 (`f083b68`), la Fase 0 (`8fff1ce`, D-007) y las decisiones D-008 (ajustes de C1, C2 y C5) y D-009 (el integrador fusiona los PR). Se leyeron las tres decisiones.
**Evidencia:** en `feature/HU-04-reglas-liquidacion` rebasada sobre `develop`: `dotnet test CasinoErp.sln` → 196 aprobadas (178 unitarias + 18 de integración), 0 fallos. Comando oficial de cobertura (AGENT.md §10) → `CasinoErp.Dominio` 96,2 % de líneas, 89,4 % de ramas.
**Decisiones:** ninguna nueva. Según D-009, F2 no lleva PR propio: queda la rama empujada y su PR se abre junto con F3 (F3.6).
**Pendientes:** F0.3 en Jira (Felipe).
**Bloqueos / necesito de otros:** F7 espera la I5 de Iván. F4, F5 y F6 ya no están bloqueadas.
**Siguiente paso:** F3, `PayrollEngine` y `NominaService`; luego F4 (API de empleados), que ya se puede hacer.
**Commits/PR:** `80427b4`, `b828fbd` en `feature/HU-04-reglas-liquidacion` ([comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-reglas-liquidacion)).

## 2026-11-18 19:55 · Tarea F2 · Terminada
**Hecho:**
- Rama `feature/HU-04-reglas-liquidacion`, tomada de la rama de F1 (F1 aún no está fusionada en `develop`), con `develop` integrado por `pull --rebase`.
- En `src/CasinoErp.Dominio/Nominas/`: `ParametrosNomina.cs` (con `Decreto2026()` e `IParametrosNominaRepositorio`), `Nomina.cs` (`EstadoNomina`, `LiquidacionEmpleado`, `Nomina` con sus totales e `INominaRepositorio`), `IReglaLiquidacion.cs`, `ReglaContratoLaboral.cs` y `ReglaPrestacionServicios.cs`, con el algoritmo del plan y `MidpointRounding.AwayFromZero`.
- `tests/CasinoErp.PruebasUnitarias/ReglasLiquidacionTests.cs`: 18 pruebas (las 14 del plan más los parámetros del decreto, el turno de mañana sin recargo, `TotalDeducciones` y los totales de `Nomina`). TDD: primero las pruebas (no compilaban: `Nominas` no existía).
**Evidencia:**
- `dotnet test tests/CasinoErp.PruebasUnitarias --filter ReglasLiquidacionTests` → 18 aprobadas, 0 fallos.
- `dotnet test CasinoErp.sln` → 193 aprobadas (178 unitarias + 15 de integración), 0 fallos. `dotnet build` → 0 advertencias.
- Cobertura: `CasinoErp.Dominio` 96,5 % de líneas y 89,5 % de ramas. Reglas e `IReglaLiquidacion.cs` al 100 %; en `Nomina.cs` faltan propiedades que se usan en F3 (`CalculadaEn`, `AprobadaEn`, `Id`).
- Los valores del plan coinciden con el mockup: Bruno Lara (crupier de noche, 2.600.000) → neto 3.229.200; jefe de piso (4.200.000) → neto 3.864.000.
**Decisiones:**
- Clase `internal static Liquidacion` en `IReglaLiquidacion.cs` con `R()` (redondeo) y `Para(empleado)` (copia los datos del empleado), para no repetir código en las dos reglas. Es interna al dominio; no cambia ninguna firma del plan.
**Pendientes:** el PR de F2 se abre junto con F3 (F3.6).
**Bloqueos / necesito de otros:** ninguno para F3.
**Siguiente paso:** F3, `PayrollEngine` y `NominaService`.
**Commits/PR:** ver el commit `HU-04: Parametros de nomina 2026 y reglas de liquidacion por tipo de contrato`.

## 2026-11-18 19:36 · Tarea F1 · Terminada (PR pendiente)
**Hecho:**
- `src/CasinoErp.Dominio/Personal/Empleado.cs`: contrato C4 literal (`Cargo`, `Turno`, `TipoContrato`, `Empleado`, `IEmpleadoRepositorio`).
- `src/CasinoErp.Dominio/Personal/EmpleadoService.cs`: `RegistrarAsync`, `ActualizarAsync`, `RetirarAsync`, `ConsultarAsync`, `ListarAsync`, `SalarioPorCargo` y `JuegosCrupier`, con los mensajes literales del plan.
- `tests/CasinoErp.PruebasUnitarias/Dobles/PersonalEnMemoria.cs` (`EmpleadosEnMemoria`) y `EmpleadoServiceTests.cs`: 28 pruebas (las 18 del plan más casos de borde: documentos de 6 y 10 dígitos, documento solo con puntos, salario negativo, actualizar con salario 0, consulta con documento formateado, nombre con espacios, tabla de salarios).
- TDD: primero las pruebas (no compilaban: `Empleado` no existía), luego el código.
**Evidencia:**
- `dotnet test tests/CasinoErp.PruebasUnitarias --filter EmpleadoServiceTests` → 28 aprobadas, 0 fallos.
- `dotnet test CasinoErp.sln` → 175 aprobadas (160 unitarias + 15 de integración), 0 fallos. `dotnet build` → 0 advertencias.
- Cobertura (`--collect:"XPlat Code Coverage"`): `CasinoErp.Dominio` 97,3 % de líneas y 89,2 % de ramas; `Personal/` 100 % de líneas.
**Decisiones:**
- Se agrega el método público `EmpleadoService.NormalizarDocumento` (quita todo lo que no sea dígito), usado por `RegistrarAsync` y `ConsultarAsync`. Es una adición dentro de mi módulo; no cambia el contrato C4.
- `ActualizarAsync` primero consulta el empleado (404 si no existe) y luego valida en el orden del plan: área, salario, retirado. `RetirarAsync` valida el motivo antes de consultar, como en el plan.
- Al cajero y al jefe de piso se les ignora el juego recibido y quedan con "Caja" y "Supervisión" (regla 7 del plan).
**Pendientes:** abrir el PR (F1.5) y mover Jira (F0.3).
**Bloqueos / necesito de otros:** ninguno para F2 y F3.
**Siguiente paso:** F2, parámetros de nómina 2026 y reglas de liquidación, en `feature/HU-04-reglas-liquidacion` tomada de esta rama.
**Commits/PR:** ver el commit `HU-04: Dominio de empleados (contrato C4)`.

## 2026-11-18 19:00 · Tarea F0 · Terminada
**Hecho:** `develop` sincronizado; rama `feature/HU-04-empleados-dominio`; tabla del plan ya estaba en la bitácora.
**Evidencia:** `dotnet test CasinoErp.sln` en `develop` → 147 aprobadas (132 unitarias + 15 de integración), 0 fallos.
**Decisiones:** ninguna.
**Pendientes:** F0.3, mover HU-04 y F1 a "En curso" en Jira (Felipe).
**Bloqueos / necesito de otros:** no hay Fase 0 (D-007), contrato C5 ni seguridad (I5) en `develop`; F4–F7 esperan.
**Siguiente paso:** F1.
**Commits/PR:** —

## 2026-11-18 · Creación de la bitácora · Terminada
**Hecho:** se crea la bitácora con el plan del Sprint 2.
**Siguiente paso:** primera tarea del plan.


<!-- Copia este bloque para cada entrada nueva -->
## AAAA-MM-DD HH:MM · Tarea X.Y · En curso | Terminada | Bloqueada
**Hecho:**
**Evidencia:**
**Decisiones:**
**Pendientes:**
**Bloqueos / necesito de otros:**
**Siguiente paso:**
**Commits/PR:**
