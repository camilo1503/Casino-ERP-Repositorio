# Bitácora · Felipe · Plan-Felipe

> Solo la edita Felipe (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| F0 | Arranque | Hecha (falta mover Jira) | — |
| F1 | Dominio de empleados (contrato C4) | Hecha (fusionada en `develop`, `f083b68`) | Fusión del integrador (D-009) |
| F2 | Parámetros y reglas de liquidación | En revisión (PR F2 + F3) | [comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-reglas-liquidacion) |
| F3 | PayrollEngine y NominaService | En revisión (PR F2 + F3) | [comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-reglas-liquidacion) |
| F4 | Persistencia y API de empleados | Pendiente | — |
| F5 | Persistencia y API de nómina | Pendiente | — |
| F6 | Front personal.js | Pendiente | — |
| F7 | Autorización de mis controladores | Pendiente | — |
| F8 | arc42 bloque Nómina y cierre | Pendiente | — |

## Pendientes abiertos
- F0.3: mover HU-04 y la subtarea F1 a "En curso" en Jira (lo hace Felipe; el agente no tiene acceso a Jira).
- ~~F1.5: abrir el PR de F1~~ (resuelto: el integrador la fusionó en `develop`, D-009).
- ~~Comando de cobertura de AGENT.md §10 sin `coverlet.msbuild`~~ (resuelto: la Fase 0 agregó el paquete).

## Bloqueos abiertos
- ~~F4, F5 y F6 esperan la Fase 0 de Jorge~~ (resuelto: Fase 0 en `develop` desde `8fff1ce`, D-007).
- F7 espera la I5 de Iván ("seguridad activa"). Aún no está en `develop`.

## Entradas

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
- Git: al hacer `pull --rebase origin develop` (llegó `321814b` de Jorge) se reescribieron los 3 commits de F2 ya empujados, con el mismo contenido (`git range-diff` sin diferencias). Se empujó con `--force-with-lease` solo sobre mi rama `feature/HU-04-reglas-liquidacion`; nunca sobre `develop` ni `main`.
**Pendientes:** abrir el PR F2 + F3 hacia `develop` (Felipe, con la descripción que preparó el agente); mover CER-22 y CER-23 en Jira.
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
