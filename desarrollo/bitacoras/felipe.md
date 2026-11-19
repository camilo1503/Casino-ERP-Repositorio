# Bitácora · Felipe · Plan-Felipe

> Solo la edita Felipe (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| F0 | Arranque | Hecha (falta mover Jira) | — |
| F1 | Dominio de empleados (contrato C4) | En revisión | (enlace al crear el PR) |
| F2 | Parámetros y reglas de liquidación | Pendiente | — |
| F3 | PayrollEngine y NominaService | Pendiente | — |
| F4 | Persistencia y API de empleados | Pendiente | — |
| F5 | Persistencia y API de nómina | Pendiente | — |
| F6 | Front personal.js | Pendiente | — |
| F7 | Autorización de mis controladores | Pendiente | — |
| F8 | arc42 bloque Nómina y cierre | Pendiente | — |

## Pendientes abiertos
- F0.3: mover HU-04 y la subtarea F1 a "En curso" en Jira (lo hace Felipe; el agente no tiene acceso a Jira).
- F1.5: abrir el PR de F1 hacia `develop` y pedirle la revisión a Iván.
- El comando de cobertura de AGENT.md §10 (`/p:CollectCoverage=true`) necesita el paquete `coverlet.msbuild`, que no está en el proyecto de pruebas. Mientras tanto se mide con `--collect:"XPlat Code Coverage"` (coverlet.collector, ya instalado).

## Bloqueos abiertos
- F4, F5 y F6 esperan la Fase 0 de Jorge (D-007). F7 espera la I5 de Iván. Al 2026-11-18 ninguna está en `develop`.

## Entradas

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
