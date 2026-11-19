# Bitácora · Jorge · Plan-Jorge

> Solo la edita Jorge (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| J0 | Fase 0 (bloqueante) | Hecha | `chore/fase-0` → `develop` (8fff1ce) |
| J1 | Sprint 2 en Jira | Hecha | sprint 35 activo |
| J2 | Backend remoto del state | Pendiente | — |
| J3 | Terraform con import | Pendiente | — |
| J4 | Workflow terraform plan | Pendiente | — |
| J5 | Integración continua (permanente) | En curso | — |
| J6 | Despliegue del Sprint 2 en Azure | Pendiente | — |
| J7 | arc42 consolidación | Pendiente | — |
| J8 | Artefactos Scrum | Pendiente | — |
| J9 | Cierre del sprint | Pendiente | — |

## Pendientes abiertos
- Avisar al equipo por el chat del grupo que la Fase 0 está en `develop` (D-007).
- J2: pedirle a Felipe (usuario de GitHub `Flashtwiice`, dueño de la suscripción de Azure donde está `rg-pitboss-erp`) el rol Contributor + User Access Administrator para Jorge.

## Notas para J5.2
- PR #3 (`feature/CER-6-despliegue-azure` → `main`): **Jorge decidió dejarlo abierto** hasta el cierre del sprint (J9). No se fusiona ni se cierra en las revisiones de PR; su contenido ya está en `develop`.

## Bloqueos abiertos
- (ninguno)

## Entradas

## 2026-11-18 · Tarea J1 · Sprint 2 en Jira · Terminada
**Hecho:**
- Con la sesión de Jorge en su navegador (Brave, extensión Claude in Chrome) se creó el sprint **"CER Sprint 2"** (id 35) en el tablero CER: del 18-11-2026 al 02-12-2026, con el Sprint Goal.
- Se movieron al sprint CER-8 y CER-13 a CER-19, y se **inició** el sprint (estado `active`).
- El sprint vacío "CER Sprint 1" (id 1), que Jira crea por defecto y que nunca se usó, se dejó sin tocar.
- CER-48 (J1) pasó a Listo.

**Evidencia:**
- La API de sprints confirma `state: active` y 8 incidencias (6 historias + 2 tareas).
- En el tablero se ven las subtareas dentro de cada historia: HU-04 2/9, HU-05 0/6, Proveedores 0/3, coordinación 1/5.

**Decisiones:**
- Fechas del sprint: dos semanas desde hoy. Si la fecha de la Sprint Review es otra, se ajustan en Jira.
- No se renombró ni se borró el sprint 1 vacío.

**Pendientes:** ninguno de J1.

**Bloqueos / necesito de otros:** ninguno.

**Siguiente paso:** J2 (backend del state de Terraform), que necesita que Felipe dé acceso a su suscripción de Azure.

**Commits/PR:** —

## 2026-11-18 · Tarea J1 · Sprint 2 en Jira · En curso
**Hecho:**
- Creadas por API en el proyecto CER (etiqueta `sprint-2`):
  - 4 épicas: CER-9 a CER-12;
  - 6 historias con puntos y responsable: CER-13 a CER-18;
  - la tarea de coordinación CER-19;
  - 37 subtareas, una por tarea de cada plan (CER-20 a CER-56).
  
  Tabla completa en `desarrollo/README.md`.
- Estados: Listo → F0 (CER-20), F1 (CER-21), J0 (CER-47) y CER-8 (movida a la épica CER-12). En curso → CER-13, CER-19, J1 (CER-48) y J5 (CER-52).
- Confirmado por Jorge: el usuario de GitHub `Flashtwiice` es **Felipe**. Él hizo el despliegue manual de Azure, así que tiene la suscripción que necesita J2.

**Evidencia:** las 48 incidencias se ven en Jira con responsable y puntos.

**Decisiones:** la herramienta de Jira no permite crear sprints → el sprint se crea desde la interfaz del backlog.

**Pendientes:** crear el sprint y mover las incidencias (Jorge debe iniciar sesión en Jira en el navegador integrado).

**Bloqueos / necesito de otros:** inicio de sesión de Jorge en Jira.

**Siguiente paso:** crear el sprint "Sprint 2" con el Sprint Goal, agregar las incidencias e iniciarlo.

**Commits/PR:** —

## 2026-11-18 · Tarea J5.2 · Revisión de PR y ramas · Terminada
**Hecho:**
- PR abiertos (API de GitHub): solo #3 `feature/CER-6-despliegue-azure` → `main`. No se fusiona (ver pendientes).
- Rama sin PR `feature/HU-04-empleados-dominio` (F1 de Felipe, commits `6dca7ce` y `0d59388`). Se revisó:
  - CI en verde (run 44430511001);
  - sin marcas de IA;
  - solo toca archivos de Felipe (`Dominio/Personal`, `Dobles/PersonalEnMemoria.cs`, `EmpleadoServiceTests.cs`, su bitácora);
  - `Empleado.cs` idéntico al contrato C4;
  - los mensajes coinciden con el plan.
  
  → **Fusionada** en `develop` (`f083b68`).

**Evidencia:** después de fusionar, `dotnet test` → 160 unitarias + 18 integración en verde; cobertura del dominio 97,05 % de líneas.

**Decisiones:** ninguna nueva (se aplicó D-009).

**Pendientes:** decisión sobre el PR #3; identificar a Flashtwiice.

**Bloqueos / necesito de otros:** ninguno.

**Siguiente paso:** avisar a Iván que C4 ya está en `develop` (puede empezar I3).

**Commits/PR:** `f083b68`.

## 2026-11-18 · Tarea J0 / J5 · Fase 0 fusionada · Terminada
**Hecho:**
- El CI de GitHub sobre `chore/fase-0` (run 44430267032) terminó en verde: job backend (build, pruebas y cobertura ≥ 80 %) y job "Sin marcas de agua de IA".
- Jorge pidió que su agente busque siempre los PR y los fusione él mismo → regla del integrador en AGENT.md §4 y decisión D-009.
- `chore/fase-0` fusionada en `develop` con `--no-ff` (commit `8fff1ce`); `dotnet test` después de fusionar: 132 + 18 aprobadas. Empujada a `origin/develop`.
- Creado D-007 (Fase 0 en develop).

**Evidencia:** CI en verde en GitHub; pruebas locales en verde tras la fusión; revisión de marcas §4.1 sin coincidencias.

**Decisiones:** D-009 (el integrador fusiona sin esperar otra aprobación si el CI está en verde, no hay conflictos y se cumple la DoD).

**Pendientes:** avisar al equipo; atender la rama de Felipe.

**Bloqueos / necesito de otros:** ninguno.

**Siguiente paso:** J5.2 en cada sesión; J1 (Jira) y J2 (Azure).

**Commits/PR:** `8fff1ce` (merge de la Fase 0).

## 2026-11-18 · Tarea J0 · Fase 0 · En revisión
**Hecho:**
- Fusionada `origin/feature/CER-6-despliegue-azure` en la rama (sin conflictos).
- CI: disparadores para `develop`, `feature/**`, `chore/**` y `docs/**`; piso de cobertura ≥ 80 % del dominio (`coverlet.msbuild` 6.0.2); job `sin-marcas-ia` (AGENT.md §4.1).
- Contrato C1: `ISemilla`, `SemillaSprint1`, `Modulos/ModuloPersonal|Nomina|Compras|Seguridad.cs` (vacíos, ya llamados desde `AgregarCasinoErp`), `ApplyConfigurationsFromAssembly`, `PrepararBaseDatos(IConfiguration)` con `BaseDatos:RecrearAlIniciar` (D-006).
- `GET /api/salud` anónimo en `Program.cs`.
- Contrato C3: `Utilidades/CasinoErpFactory.cs` (movida desde `ApiTests.cs`) y `Utilidades/ApiJson.cs`; nuevas pruebas en `BaseDatosTests.cs` (recrear, conservar, salud).
- Contrato C2: `conexion-api.js` dividido en `js/api/nucleo.js`, `sprint1.js`, `inventario.js` (Camilo), `seguridad.js`, `personal.js`, `compras.js` (vacíos) y `arranque.js`; `index.html` solo cambia las etiquetas `<script>` y el texto "Sprint 2".
- Limpieza: borrado `registro.py` (CER-8); `docs/index.md` corregido; esqueleto de `docs/arc42/README.md`.

**Evidencia:**
- `dotnet test CasinoErp.sln` → 132 unitarias + 18 integración aprobadas, 0 fallos.
- Cobertura del dominio (comando de AGENT.md §10): 96,61 % de líneas, 88,23 % de ramas; supera el piso del 80 %.
- `node --check` sin errores en los 7 archivos JS.
- Regresión manual del front (J0.8), con la API local y `RecrearAlIniciar=true`; sin errores en la consola; `PitBossApi.estado.online = true`; datos de la API (8 jugadores, 19 mesas, 14 transacciones):
  1. Perfiles: jugador 1066777888 registrado desde el formulario → `GET /api/jugadores/1066777888` lo devuelve. ✔
  2. Catálogo: mesa M-19 registrada y actualizada (Salón VIP, $20.000–$400.000) → confirmado en la API. ✔
  3. Operación de mesas: relleno de M-04 aprobado → `APROBADO` en la API. ✔
  4. Caja: compra de 2 fichas de $50.000 para p1 → TX-00015 por $100.000; FICHA-50K pasa de 2.600 a 2.598. ✔
  5. Inventario: "Pedir al proveedor" ($25.000) → FICHA-25K pasa de 3.100 a 4.600. ✔
  6. Retención: oferta aceptada para p8 → `ACEPTADA` en la API y beneficio activo en el front. ✔ (Con p2 la API respondió 400 "El jugador ya recibió una oferta hoy": es la regla de negocio, no una falla.)
  7. Sin API: `iniciar()` rechaza (arranque.js muestra el aviso), `online = false` y el prototipo sigue funcionando en local. ✔
  
  La señal "Bloqueada" del prototipo queda al final del registro de retención, igual que en el Sprint 1.

**Decisiones:** ver `decisiones/D-008-jorge-ajustes-fase-0.md` (salud adelantado, `pedirSi` con lista, `util` ampliado, namespaces, sin `GetValue`, job de marcas de IA). D-007 queda reservada para el aviso de Fase 0 lista.

**Pendientes:** merge del PR y D-007; verificar el CI en GitHub.

**Bloqueos / necesito de otros:** ninguno (luego se decidió D-009).

**Siguiente paso:** tras el merge, D-007 + aviso al equipo; luego J1 (Jira) y J2 (backend del state, necesita acceso a la suscripción de Azure).

**Commits/PR:** rama `chore/fase-0` (ver PR hacia `develop`).

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
