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
| F6 | Front personal.js | En revisión | [comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-front) |
| F7 | Autorización de mis controladores | Terminada (en revisión) | PR de `feature/HU-04-autorizacion` hacia `develop` |
| F8 | arc42 bloque Nómina y cierre | En revisión (demo ensayada: 2 min) | [comparar](https://github.com/camilo1503/Casino-ERP-Repositorio/compare/develop...feature/HU-04-arc42-nomina) |
| F9 | Acceso de Jorge a Azure (D-010, D-011) | Hecha (F9.6 pendiente: secretos de GitHub cuando Jorge los pida) | Sin PR (tarea de portal) |

## Pendientes abiertos
- ~~F0.3: mover HU-04 y la subtarea F1 a "En curso" en Jira~~ (resuelto: HU-04 en "En curso" y F0–F3 en "Listo").
- ~~F1.5: abrir el PR de F1~~ (resuelto: el integrador la fusionó en `develop`, D-009).
- ~~Comando de cobertura de AGENT.md §10 sin `coverlet.msbuild`~~ (resuelto: la Fase 0 agregó el paquete).
- La rama `feature/HU-04-reglas-liquidacion` quedó obsoleta (F2 ya entró por `feature/HU-04-nomina-motor`). No se fusiona; Felipe o Jorge pueden borrarla.
- Orden de fusión de mis ramas: F4 (`feature/HU-04-empleados-api`) → F5 (`feature/HU-04-nomina-api`) → F6 (`feature/HU-04-front`) → F8 (`feature/HU-04-arc42-nomina`). Cada una contiene a la anterior.
- ~~Jira: mover las tarjetas de F4–F8~~ (resuelto el 2026-11-18: CER-24, 25, 26 y 28 en "En curso" con un comentario de evidencia cada una; CER-27 en "Por hacer" con el comentario de bloqueo. El tablero no tiene columna "En revisión"; el integrador las pasa a "Listo" al fusionar). Falta: HU-04 (CER-13) a "Listo" cuando se fusione F7.
- ~~F8.4: ensayar la demo de 2 minutos~~ (resuelto: 2 minutos).

- F9.5: avisarle a Jorge por el chat del grupo que ya tiene los roles (lo hace Felipe).
- F9.6: cargar en GitHub los secretos `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RG` y `TFSTATE_SA` cuando Jorge entregue los valores (lo hace quien administra el repositorio).

## Bloqueos abiertos
- ~~F4, F5 y F6 esperan la Fase 0 de Jorge~~ (resuelto: Fase 0 en `develop` desde `8fff1ce`, D-007).
- ~~F7 espera la I5 de Iván~~ (resuelto en código el 2026-11-18: F7 hecha sobre la rama de Iván). La seguridad entró a `develop` en `cce0aa1` (merge normal, sin *squash*); el PR de F7 ya solo muestra mis cambios.

## Entradas

## 2026-11-18 23:31 · Tarea F7 · PR listo para abrir
**Hecho:** Jorge fusionó la seguridad de Iván en `develop` (`cce0aa1`, merge normal que incluye los PR #4 a #10) y anotó en su bitácora que el código de F7 queda cubierto por los atributos de la I5. Como no fue *squash*, `feature/HU-04-autorizacion` no necesita rebase: frente a `develop` solo trae el commit de F7 y esta entrada.
**Evidencia:** `develop` (`d88fc73`) + `4b502cf` combinados en una copia aparte, sin conflictos → `dotnet test CasinoErp.sln -c Release`: 260 unitarias + 57 de integración (54 de `develop` + 3 de F7), 0 fallos.
**Decisiones:** ninguna.
**Pendientes:** abrir el PR (Felipe) y fusionarlo (Jorge, D-009); HU-04 (CER-13) y CER-27 a "Listo" al fusionar.
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** ninguno de código.
**Commits/PR:** ver el commit `HU-04: Bitacora de Felipe, F7 lista para PR tras fusionar la seguridad`.

## 2026-11-18 23:23 · Tarea F7 · Terminada (en revisión)
**Hecho:**
- La I5 de Iván está terminada en su rama (`feature/HU-06-jwt`, PR #9, contenida en `feature/HU-06-front`, PR #10), pero todavía no está en `develop` ni hay aviso formal de "seguridad activa". Para no frenar el cierre de HU-04, la rama `feature/HU-04-autorizacion` sale de `origin/feature/HU-06-front` (sin seguimiento de esa rama, para que el push nunca vaya a la de Iván).
- F7.2: Iván ya había puesto `[AutorizarModulo(Modulos.Competencias, Modulos.Rrhh, Modulos.Mesas)]` en `EmpleadosController` y `[AutorizarModulo(Modulos.Rrhh)]` en `NominaController` (su bitácora, I5: "2 `using` y 1 atributo cada uno"). Verificado con `git diff`; no se toca de nuevo.
- F7.3: Iván ya había pasado `PersonalNominaApiTests` y `NominaApiTests` a `ClienteComoAsync("felipe.cortes@pitboss.co")` con `IAsyncLifetime`. Se agregan en `PersonalNominaApiTests`: `Un_cajero_no_puede_calcular_la_nomina` (laura.gomez → `POST /api/nomina/calculate` 403), `El_jefe_de_piso_lista_empleados_pero_no_ve_nomina` (daniela.vargas → `GET /api/empleados` 200 y `GET /api/nomina` 403) y `Sin_sesion_no_se_puede_listar_empleados` (sin token → 401).
**Evidencia:**
- `dotnet test CasinoErp.sln -c Release` → 317 aprobadas (260 unitarias + 57 de integración), 0 fallos.
- Cobertura de `CasinoErp.Dominio`: 97,22 % de líneas (umbral 80 %).
- F7.4 a mano en `http://localhost:5080` con la base recreada: Laura Gómez (Cajero) entra con la contraseña y el código de demo y solo ve "Caja / POS" e "Inventario de fichas"; no aparece "Turnos y nómina".
**Decisiones:** basar F7 en la rama de Iván en vez de esperar a `develop` (el plan dice F7.1 `pull --rebase` de `develop`). Cuando Jorge fusione #9 y #10, el PR de esta rama hacia `develop` solo mostrará el commit de F7.
**Pendientes:** abrir el PR de `feature/HU-04-autorizacion` cuando #9 y #10 estén en `develop` (si Jorge fusiona con *squash*, antes hay que pasar mi commit a una rama nueva desde `develop` con `cherry-pick`); HU-04 (CER-13) a "Listo" cuando se fusione.
**Bloqueos / necesito de otros:** Jorge fusiona #9 y #10 de Iván.
**Siguiente paso:** ninguno de código; solo el PR y el cierre en Jira.
**Commits/PR:** ver el commit `HU-04: Autorizacion por modulo en empleados y nomina`.

## 2026-11-18 21:20 · Tarea F9 · Terminada (F9.6 pendiente)
**Hecho:**
- Leídas la tarea F9 de `Plan-Felipe.md` y la decisión D-010 (Jorge las agregó en `develop`, commit `55568a7`).
- Antes: Jorge (`jalcidesdiaz@ucundinamarca.edu.co`, objeto `7d128dfa-90ee-4a1b-adb0-bf53f0e8d557`) tenía solo **Contributor sobre la web app** `pitboss-erp-8szxa`; Felipe es **Owner** de la suscripción `12f1bc21-94a3-4b28-9a90-8224d1e5fcc8`.
- Asignados a Jorge con Azure CLI, sobre la **suscripción completa**: **Contributor** y **Role Based Access Control Administrator con condición** (solo puede asignar o quitar Reader, Contributor y Storage Blob Data Contributor).
**Evidencia:** `az role assignment list --all --assignee 7d128dfa-90ee-4a1b-adb0-bf53f0e8d557` → Contributor (web app), Contributor (suscripción), Role Based Access Control Administrator (suscripción, con condición).
**Decisiones:** [D-011](../decisiones/D-011-felipe-roles-azure-con-condicion.md): en vez de User Access Administrator u Owner se usa un administrador de roles con condición, para que Jorge no pueda darse Owner ni dar acceso a terceros. Cubre lo que piden J2.3 y J4.1. Felipe eligió esta opción en el chat.
**Pendientes:** F9.5, avisarle a Jorge por el chat; F9.6, los secretos de GitHub cuando Jorge los pida.
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** F7 cuando Iván anuncie "seguridad activa".
**Commits/PR:** ver el commit `HU-04: Tarea F9, roles de Azure para Jorge con condicion (D-011)`.

## 2026-11-18 20:34 · Tarea F8 · Terminada (en revisión) · Cierre de la sesión
**Hecho:**
- Rama `feature/HU-04-arc42-nomina`, encadenada después de F6 para que esta bitácora no tenga conflictos al fusionar.
- `docs/arc42/05-bloques/nomina.md`: propósito y frontera del bloque (tabla de capas y archivos, endpoints de F4 y F5, dependencias, autorización de F7), diagrama Mermaid `classDiagram`, sección **SOLID en el código** (SRP por clase con su ruta, DIP con los constructores y el registro de `ModuloNomina`, OCP con el ejemplo del aprendiz SENA y la prueba que lo demuestra), diagrama Mermaid `sequenceDiagram` de `POST /api/nomina/calculate` (JWT y `[AutorizarModulo(Modulos.Rrhh)]`, `alt` para nómina aprobada, sin parámetros y errores del motor, `loop` por empleado, `opt` del borrador anterior), tabla de reglas con los casos de prueba de F2 y resumen de pruebas y cobertura.
- `docs/arc42/09-decisiones/ADR-005-parametros-nomina.md`: contexto (decretos 2026 en disputa judicial), decisión (parámetros por año editables por API, validaciones), consecuencias, simplificación del Art. 114-1 (D-003) y deuda para el Sprint 3.
**Evidencia:**
- Los dos diagramas se renderizaron sin errores con Mermaid 10.9.1 en el navegador (página de validación local).
- `dotnet test CasinoErp.sln` → 244 aprobadas, 0 fallos (sin cambios de código en F8).
**Decisiones:** el bloque documenta la autorización de F7 como prevista ("desde F7"), porque la I5 de Iván aún no está en `develop`; así la revisión cruzada de Jorge (J7.4) encuentra el control en el bloque.
**Pendientes:** ensayar la demo (F8.4) y anotar el tiempo; F7 cuando Iván anuncie "seguridad activa"; mover HU-04 en Jira.
**Bloqueos / necesito de otros:** F7 espera la I5 de Iván.
**Siguiente paso:** F7 (autorización por módulo en `EmpleadosController` y `NominaController` y las dos pruebas de 403).
**Commits/PR:** ver el commit `HU-08: arc42 bloque Nomina con diagrama de secuencia y ADR-005`.

### Guion de la demo de la Sprint Review (F8.4, máximo 2 minutos)
1. Iniciar sesión como **Felipe Cortés** (Coordinador de talento humano).
2. **Turnos y nómina** → mostrar la tarjeta "Nómina de octubre" → **Calcular nómina**.
3. En el modal, señalar a **Bruno Lara**: turno noche, recargo **$910.000**, sin auxilio (su IBC supera 2 SMMLV), neto **$3.229.200**.
4. Señalar a **Ana Torres**: turno tarde, auxilio de transporte **$249.095**, neto **$2.641.095**.
5. Mostrar el pie: total neto y aportes del empleador (parafiscales).
6. **Aprobar nómina** → toast "Nómina aprobada. Se pagará el 30/10/2026." → abrir otra vez el modal: etiqueta **Aprobada**, sin botón.
7. (Si sobra tiempo) Swagger: `POST /api/nomina/calculate` con el mismo mes → 400 "La nómina de 10/2026 ya está aprobada."
- Antes de la demo: arrancar con `BaseDatos__RecrearAlIniciar=true` para que la nómina de octubre no esté aprobada.
- **Tiempo del ensayo:** 2 minutos (ensayo de Felipe del 2026-11-18, app local con `BaseDatos:RecrearAlIniciar=true`). Cabe en el tiempo asignado (minutos 3–5 del guion de la Sprint Review).

## 2026-11-18 20:34 · Retrospectiva HU-04 (F8.3)
**(a) Tiempo real contra lo estimado.** HU-04 se estimó en 8 SP; el plan no traía horas. Trabajo del agente en esta sesión: F0 a F8 entre las 19:00 y las 20:34 (≈ 1 h 35 min), sin contar el tiempo de Felipe en revisión, Jira, PR y la espera del CI (≈ 3 a 5 min por push). Las tareas de dominio (F1–F3) fueron las más rápidas; F4–F6 tomaron más por la prueba manual en el navegador.
**(b) Casos de borde que no se vieron en el planning:**
- Ramas que dependen de otras sin fusionar: F2 se hizo sobre F1 y F5/F6/F8 en cadena. Al integrar `develop` con `pull --rebase` después de empujar, se reescribieron commits ya publicados y hubo que crear una rama nueva (`feature/HU-04-nomina-motor`), porque el push forzado está prohibido.
- Pruebas de integración que comparten base: la nómina de octubre "con 18 filas" dependía del orden de las pruebas de empleados; se resolvió con una clase de pruebas propia.
- El comando de cobertura de AGENT.md necesitaba `coverlet.msbuild` (lo agregó la Fase 0).
- Reglas que el plan no fijaba: salario exactamente igual al mínimo (se acepta), un retirado bajo el mínimo no bloquea la nómina, un error del motor no debe borrar el borrador anterior, el mes con dos dígitos en el mensaje (03/2026).
- En el front, `soloActivos=true` saca a los retirados de `S.staff`; si un usuario o una mesa apunta a un retirado, el mockup lo muestra como "sin asignar".
- Mientras se calcula la nómina, el modal muestra por un instante el cálculo anterior hasta que llega la respuesta.
**(c) Propuesta para la estimación del Sprint 3:**
- Sumar 1 SP a cada historia que dependa de otra rama o de otro integrante (coordinación, rebase y orden de fusión).
- Regla de Git: integrar `develop` con `pull --rebase` **solo antes del primer push** de una rama; después, usar `git merge origin/develop`.
- Pedir que el plan diga cómo aislar las pruebas de integración que cuentan registros (una factory por clase).
- Medir el tiempo real por tarea en la bitácora desde el inicio (hora de inicio y fin en cada entrada).

## 2026-11-18 20:30 · Tarea F6 · Terminada (en revisión)
**Hecho:**
- Rama `feature/HU-04-front`, tomada de la rama de F5 (F4 y F5 aún no están fusionadas y el front las necesita).
- Solo se editó `src/CasinoErp.Api/wwwroot/js/api/personal.js` (IIFE con `PitBossApi`; no se tocó `index.html`):
  1. carga `empleados` (orden 15): `S.staff` desde `GET /api/empleados?soloActivos=true`, conservando `id` (`s1`…`s18`) y certificación; los nuevos con `id = 'se' + id`;
  2. `FORM.employee` conectado a `POST /api/empleados`;
  3. carga `nomina` (orden 80): `S.nomina` = nómina 2026/10;
  4. `payroll()` redefinido: usa la nómina real si hay API y `S.nomina` (así `V.rrhh` muestra los totales reales sin tocarlo); si no, el cálculo del mockup;
  5. `ACT.modal` conectado: al abrir el modal de nómina en borrador se llama `POST /api/nomina/calculate`;
  6. `MODALS.payroll` nuevo con el título, subtítulo, columnas, etiquetas de contrato, pie (total neto y aportes del empleador) y botones del plan; sin API usa el del mockup;
  7. `ACT.approvePayroll` conectado a `POST /api/nomina/{id}/aprobar`.
**Evidencia (prueba manual F6.2, API con `BaseDatos:RecrearAlIniciar=true`, usuario Felipe Cortés · Coordinador de talento humano):**
1. Competencias → + Registrar empleado "Marta Gil", `1099888777`, Cajero → toast "Marta Gil registrado. Requiere examen de manejo de caja."; en la API: Cajero · Caja. Tras recargar la página, Marta sigue (19 empleados). ✅
2. Registrar otra vez `1099888777` → "Ya existe un empleado con ese documento." ✅
3. Turnos y nómina → Calcular nómina → 19 filas. Bruno Lara: recargo $910.000, auxilio —, neto $3.229.200. Ana Torres: auxilio $249.095, neto $2.641.095. Total neto $51.439.815; aportes del empleador $15.404.288. La tarjeta "Nómina de octubre" de `V.rrhh` muestra los mismos totales. ✅
4. Aprobar nómina → toast "Nómina aprobada. Se pagará el 30/10/2026."; API en `APROBADA`; al reabrir, etiqueta **Aprobada** y sin botón (no se recalcula). ✅
5. Como Ana Torres (Crupier, `sid s1`): **Mi mesa** muestra M-01 · Blackjack. ✅
6. Sin API (front servido solo con un servidor estático en el puerto 5081): toast de modo demostración y el modal muestra el cálculo de ejemplo del mockup (6 columnas, "Valores de ejemplo."). ✅
- `node --check personal.js` sin errores. `dotnet test CasinoErp.sln` → 244 aprobadas (210 + 34), 0 fallos.
**Decisiones:**
- Las cargas usan `pedirSi` (D-008) en vez de `if (!S.user || ...some(allowed))`: es la misma regla escrita con el ayudante del núcleo, y además permite que la nómina se cargue antes del inicio de sesión, igual que los empleados.
- La etiqueta "Aprobada" se muestra junto al título del modal.
**Pendientes:** fusionar F4 → F5 → F6 en ese orden (integrador); mover CER-26 en Jira.
**Bloqueos / necesito de otros:** F7 espera la I5 de Iván ("seguridad activa").
**Siguiente paso:** F8 (arc42 del bloque Nómina, diagrama de secuencia y ADR-005), que no depende de F7.
**Commits/PR:** ver el commit `HU-04: Front de empleados y nomina conectado a la API`.

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
