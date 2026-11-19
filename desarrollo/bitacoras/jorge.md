# Bitácora · Jorge · Plan-Jorge

> Solo la edita Jorge (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| J0 | Fase 0 (bloqueante) | Hecha | `chore/fase-0` → `develop` (8fff1ce) |
| J1 | Sprint 2 en Jira | Hecha | sprint 35 activo |
| J2 | Backend remoto del state | A: hecha (sin fusionar) · B: bloqueada por F9 | rama `feature/HU-07-bootstrap-state` (`ce37957`) |
| J3 | Terraform con import | A: hecha (sin fusionar) · B: bloqueada por F9 | rama `feature/HU-07-bootstrap-state` (`815ddf6`) |
| J4 | Workflow terraform plan | A: hecha (sin fusionar) · B: bloqueada por F9 | rama `feature/HU-07-bootstrap-state` (`08f072a`) |
| J5 | Integración continua (permanente) | En curso | — |
| J6 | Despliegue del Sprint 2 en Azure | Pendiente | — |
| J7 | arc42 consolidación | En curso: J7.1–J7.3 y J7.5 hechos; J7.4 espera las secciones de los demás | rama `docs/HU-08-arc42-jorge` (`7d0d8a7`) |
| J8 | Artefactos Scrum | En curso: 4 documentos escritos; faltan capacidad, video, captura y retrospectiva de cada integrante | rama `docs/sprint-2-scrum` (`004a597`) |
| J9 | Cierre del sprint | Pendiente | — |

## Pendientes abiertos
- Avisar al equipo por el chat del grupo que la Fase 0 está en `develop` (D-007).
- J2: Felipe debe hacer la tarea **F9** de su plan (roles en la suscripción). Ya está escrita en `Plan-Felipe.md`; falta que la lea y avisarle por el chat del grupo.
- ~~Hacer J2-A, J3-A y J4-A, J7 y J8~~ (hechos, ver la entrada de hoy 2026-11-18 · J2-A a J8).
- **Fusionar las cuatro ramas de Jorge** cuando el CI esté en verde (`gh` no tiene sesión, así que el CI se revisa en GitHub): `docs/ajuste-acceso-azure-j2`, `docs/HU-08-arc42-jorge`, `docs/sprint-2-scrum` y `feature/HU-07-bootstrap-state`. Esta última **no** debe fusionarse antes de la parte B: el workflow `terraform-plan.yml` fallará en cada PR que cambie un `.tf` mientras no existan los secretos de Azure.
- Actualizar ADR-003 y la vista de despliegue (§7) cuando se aplique Terraform (parte B).
- J7.4: revisión cruzada cuando estén en `develop` `10-conceptos-seguridad.md` y los `05-bloques/*.md`.
- J8: llenar la capacidad del sprint, definir quién graba el video, guardar la captura `terraform-plan-pr.png` y pedir a cada integrante su parte de la retrospectiva (incluida la "Retrospectiva HU-04" de Felipe, F8.3).
- Crear la subtarea de Jira de F9 de Felipe.

## Notas para J5.2
- PR #3 (`feature/CER-6-despliegue-azure` → `main`): **Jorge decidió dejarlo abierto** hasta el cierre del sprint (J9). No se fusiona ni se cierra en las revisiones de PR; su contenido ya está en `develop`.

## Bloqueos abiertos
- Desde 2026-11-18: sin roles en la suscripción de Azure (parte B de J2, J3 y J4, y J6). Depende de **F9 de Felipe**.

## Entradas

## 2026-11-18 · Tareas J2-A, J3-A, J4-A, J7 y J8 · Parte A, J7 y J8 escritos · Terminada (sin fusionar)
**Hecho:**
- **J2-A** (`ce37957`): `infra/terraform/bootstrap/main.tf` y `outputs.tf` (grupo `rg-pitboss-tfstate`, cuenta de storage sin claves de acceso, contenedor privado `tfstate`), `.terraform.lock.hcl` y `.gitignore` con `.terraform/`, `*.tfstate*`, `backend.hcl` y `*.tfplan`.
- **J3-A** (`815ddf6`): `versions.tf`, `backend.tf`, `backend.hcl.example`, `variables.tf`, `main.tf`, `outputs.tf` y `README.md` en `infra/terraform/`.
- **J4-A** (`08f072a`): `.github/workflows/terraform-plan.yml` (disparo por `pull_request` a `develop` con `paths` de `.tf`, OIDC, `fmt`, `init`, `validate`, `plan` y resumen del job) y la sección OIDC del README de Terraform.
- **J7** (`7d0d8a7`): `docs/arc42/05-vista-bloques.md` (nivel 1 y tabla de bloques), `07-vista-despliegue.md`, `09-decisiones/ADR-001`, `ADR-003` y `ADR-004` y los enlaces del índice.
- **J8** (`004a597`): `docs/sprint-2/sprint-backlog.md`, `retrospectiva.md`, `investigacion-terraform-actions.md`, `guion-video.md` y la carpeta `evidencias/`.

**Evidencia:**
- Terraform (J2-A y J3-A): `terraform init -backend=false` y `terraform validate` → `Success! The configuration is valid.`; `terraform fmt -check -recursive` limpio.
- Workflow (J4-A): el YAML se parsea con PyYAML (7 pasos). **No se ha ejecutado en GitHub.**
- Revisión de marcas de IA de AGENT.md §4.1 sobre los commits de cada rama: sin coincidencias.
- J7 y J8 son solo documentos: no se corrió `dotnet test` ni se vieron renderizados los diagramas Mermaid.
- Estado de `develop` al partir (`dotnet test` de esta sesión): 210 unitarias + 18 de integración, 0 fallos.

**Decisiones:**
- **D-010.** Además:
  - J4: el paso final del workflow del plan no era YAML válido (`run: { echo ...; cat } >> ...`), así que se escribió como bloque `run: |` con `if: always()`.
  - J3: el proveedor `azurerm` 4.81 no tiene `use_free_limit` ni `free_limit_exhaustion_behavior`, así que la base `CasinoErp` solo se importa. Se agregaron `lifecycle { prevent_destroy = true }` e `ignore_changes`.
  - J3: se agregaron las variables `nombre_plan`, `admin_sql_login`, `admin_sql_object_id` y `reglas_firewall`, con valores por defecto vacíos que se completan al importar (J3.5).
- Se corrigió el contenido de `Comun` en arc42 §5 tras revisar el código: `Respuesta<T>` vive en la API.

**Pendientes:** la parte B de J2, J3 y J4; J6 y J9; la fusión de las cuatro ramas; el resto de J7.4 y de J8 (ver "Pendientes abiertos").

**Bloqueos / necesito de otros:** de Felipe, F9 (desde 2026-11-18); de Iván y Camilo, sus secciones de arc42 para J7.4.

**Siguiente paso:** cuando Felipe asigne los roles, comprobar con `az role assignment list` y ejecutar J2-B (bootstrap `apply`). Mientras tanto, J5 (revisar PR y ramas) en cada sesión.

**Commits/PR:** ramas `feature/HU-07-bootstrap-state` (`ce37957`, `815ddf6`, `08f072a`), `docs/HU-08-arc42-jorge` (`7d0d8a7`), `docs/sprint-2-scrum` (`004a597`) y `docs/ajuste-acceso-azure-j2` (F9 y D-010). Sin PR.

## 2026-11-18 · Tarea J2 · Acceso a Azure · Bloqueada
**Hecho:**
- Instalados con winget Azure CLI 2.93.0 y Terraform 1.17.1.
- `az login` con `jalcidesdiaz@ucundinamarca.edu.co` en la suscripción Azure for Students (`12f1bc21-94a3-4b28-9a90-8224d1e5fcc8`).
- Se comprobó el acceso: `az role assignment list` muestra solo **Contributor sobre la web app `pitboss-erp-8szxa`**. `az group list` vuelve vacío y `az group show -n rg-pitboss-erp` responde `AuthorizationFailed`. En el portal solo aparece la web app.
- Plan de Felipe: tarea **F9** (asignar Contributor + User Access Administrator, o Owner, sobre la suscripción completa).
- Plan de Jorge: J2, J3 y J4 se parten en parte A (sin Azure) y parte B (con Azure), con el orden J2-A → J3-A → J4-A → J7 → J8.

**Evidencia:** salidas de `az role assignment list` y `az group show` descritas arriba. No hubo cambios de código, así que no se corrieron pruebas nuevas (`dotnet test` de la sesión: 210 unitarias + 18 de integración, 0 fallos).

**Decisiones:** `D-010-jorge-acceso-azure-en-plan-felipe.md`. Se editó el plan de Felipe porque Jorge lo ordenó en el chat (AGENT.md §1).

**Pendientes:** que Felipe haga F9; J2-A, J3-A, J4-A, J7 y J8.

**Bloqueos / necesito de otros:** de Felipe, los roles en la suscripción (desde 2026-11-18).

**Siguiente paso:** J2-A (escribir `infra/terraform/bootstrap/*.tf` y validarlo con `init -backend=false`).

**Commits/PR:** rama `docs/ajuste-acceso-azure-j2`.

## 2026-11-18 · Tarea J5.2 · Revisión de ramas · Terminada
**Hecho:**
- No hay PR abiertos hacia `develop`. Había dos ramas de Felipe sin fusionar:
  - `feature/HU-04-nomina-motor` (F2 + F3). Se revisó:
    - CI en verde (run 44433902063);
    - sin marcas de IA;
    - solo toca archivos de Felipe;
    - los mensajes coinciden con el plan;
    - `PayrollEngine` es puro y documenta SRP, DIP y OCP;
    - 47 pruebas de nómina.
    
    → **Fusionada** en `develop` (`5ce30c7`).
  - `feature/HU-04-reglas-liquidacion` (F2): **no se fusiona por separado**, porque su contenido es idéntico al F2 rebaseado dentro de `nomina-motor` (verificado con `git diff`: sin diferencias). Queda obsoleta; Felipe puede borrarla.
- Jira: CER-22 (F2) y CER-23 (F3) pasan a Listo.

**Evidencia:** después de fusionar, `dotnet test` → 210 unitarias + 18 integración en verde; cobertura del dominio 97,35 % de líneas.

**Decisiones:** ninguna nueva (D-009).

**Pendientes:** ninguno.

**Bloqueos / necesito de otros:** ninguno.

**Siguiente paso:** J2 (acceso de Felipe a la suscripción de Azure).

**Commits/PR:** `5ce30c7`.

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
