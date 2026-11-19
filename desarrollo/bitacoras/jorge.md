# Bitácora · Jorge · Plan-Jorge

> Solo la edita Jorge (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| J0 | Fase 0 (bloqueante) | Hecha | `chore/fase-0` → `develop` (8fff1ce) |
| J1 | Sprint 2 en Jira | Hecha | sprint 35 activo |
| J2 | Backend remoto del state | Hecha (bootstrap aplicado) | `9be6eec` |
| J3 | Terraform con import | Hecha (importado, plan revisado, sin aplicar) | `9be6eec` |
| J4 | Workflow terraform plan | Workflow e identidad listos; faltan los 5 secretos (los carga Camilo) y la prueba del PR (J4.3) | `9be6eec` |
| J5 | Integración continua (permanente) | En curso | — |
| J6 | Despliegue del Sprint 2 en Azure | Pendiente | — |
| J7 | arc42 consolidación | En curso: J7.1–J7.3 y J7.5 hechos; J7.4 espera las secciones de los demás | fusionada en `develop` (`1c9546b`) |
| J8 | Artefactos Scrum | En curso: 4 documentos escritos; faltan capacidad, video, captura y retrospectiva de cada integrante | fusionada en `develop` (`fe5141a`) |
| J9 | Cierre del sprint | Pendiente | — |

## Pendientes abiertos
- Avisar al equipo por el chat del grupo que la Fase 0 está en `develop` (D-007).
- J2: Felipe debe hacer la tarea **F9** de su plan (roles en la suscripción). Ya está escrita en `Plan-Felipe.md`; falta que la lea y avisarle por el chat del grupo.
- ~~Hacer J2-A, J3-A y J4-A, J7 y J8~~ (hechos, ver la entrada de hoy 2026-11-18 · J2-A a J8).
- ~~Fusionar las ramas de docs de Jorge~~ (hecho, ver la entrada de J5.2 de hoy). Falta **`feature/HU-07-bootstrap-state`**: no debe fusionarse antes de la parte B, porque el workflow `terraform-plan.yml` fallará en cada PR que cambie un `.tf` mientras no existan los secretos de Azure.
- Actualizar ADR-003 y la vista de despliegue (§7) cuando se aplique Terraform (parte B).
- J7.4: revisión cruzada cuando estén en `develop` `10-conceptos-seguridad.md` y los `05-bloques/*.md`.
- J8: llenar la capacidad del sprint, definir quién graba el video, guardar la captura `terraform-plan-pr.png` y pedir a cada integrante su parte de la retrospectiva (incluida la "Retrospectiva HU-04" de Felipe, F8.3).
- Crear la subtarea de Jira de F9 de Felipe.

## Notas para J5.2
- PR #3 (`feature/CER-6-despliegue-azure` → `main`): **Jorge decidió dejarlo abierto** hasta el cierre del sprint (J9). No se fusiona ni se cierra en las revisiones de PR; su contenido ya está en `develop`.

## Bloqueos abiertos
- De acceso a Azure: ninguno (F9 resuelta, D-011). Para J4.3 hacen falta los 5 secretos de GitHub, que solo puede cargar el administrador del repositorio (Camilo); Jorge tiene `push` pero no `admin`.

## Entradas

## 2026-11-19 · Tarea J5.2 · Fusión del PR #8 (HU-05 C1–C2), F7 final y cierre de Iván · Terminada
**Hecho:**
- Camilo actualizó el PR #8 (merge de `develop`, pruebas de integración con `ClienteComoAsync` y una prueba de autorización del crupier). Con el CI en verde y sin marcas de IA, se **fusionó** → `d51e4e6`.
- **Fusionada** `feature/HU-04-autorizacion` (Felipe, PR #15): tarjeta de nómina con el estado real tras aprobar (corrige el hallazgo de la prueba manual), su retrospectiva de HU-04 en `docs/sprint-2/retrospectiva.md` y el encabezado numerado de `nomina.md` → `9aaa9f9`.
- **Fusionada** `feature/HU-06-seguridad-activa` (Iván, PR #14): cierre de Plan-Ivan, retrospectiva y ensayo de la demo en su bitácora → `cc3c5a8`.
- README raíz y tablero actualizados.

**Evidencia:** después de fusionar los tres, `dotnet test CasinoErp.sln` → 271 unitarias + 62 de integración, 0 fallos; cobertura del dominio con el comando de AGENT.md §10 (PowerShell): 97,36 % de líneas, 90,24 % de ramas y 94,88 % de métodos. `node --check` sin errores en `personal.js`. Revisión de marcas de IA sin coincidencias. Los PR #8, #14 y #15 figuran como fusionados.

**Decisiones:** ninguna.

**Pendientes:** C3 a C8 de Camilo (proveedores, órdenes de compra, front, autorización y arc42); los secretos de GitHub (F9.6, Camilo); J4.3; J7.4 (bloque de Camilo); J6; J9.

**Bloqueos / necesito de otros:** Camilo (C3–C8 y secretos).

**Siguiente paso:** J5.2 cuando Camilo suba C3 en adelante.

**Commits/PR:** `d51e4e6`, `9aaa9f9`, `cc3c5a8`.

## 2026-11-19 · Tareas J9.1, J9.3 (parcial) y Jira · Prueba manual local, cobertura y tarjetas · Terminada
**Hecho:**
- **Cobertura medida** con el comando de AGENT.md §10 sobre `develop` (`8072dcf`): `CasinoErp.Dominio` 97,22 % de líneas, 90,04 % de ramas y 94,64 % de métodos; 260 pruebas unitarias. README actualizado (`ba9c08d`).
- **Prueba manual local** (`dotnet run`, `BaseDatos__RecrearAlIniciar=true`, navegador integrado):
  1. Login con 2FA como `felipe.cortes`: el menú muestra solo Panel gerencial, Competencias y Turnos y nómina. ✔
  2. Turnos y nómina → Calcular nómina: 18 empleados; Ana Torres neto $2.641.095 (con auxilio); Bruno Lara recargo $910.000, sin auxilio, neto $3.229.200; Daniela Vargas (jefe de piso) $3.864.000; total neto $49.074.720 y aportes del empleador $14.713.782. ✔ Coincide con los ejemplos de arc42.
  3. Aprobar nómina: toast «Nómina aprobada. Se pagará el 30/10/2026.» y la API confirma `APROBADA` con 18 empleados. ✔
  4. Laura Gómez (cajero): solo ve Caja / POS e Inventario de fichas. ✔
  5. Iván León (administrador): ve Usuarios y roles (10 cuentas activas, 1 inactiva, 9 roles). ✔
  6. Cambio de rol por API (Sebastián Prada, de ANFITRION a CAJERO): pasa de `inicio, perfiles, retencion, catalogo` a `caja, inventario`; nómina → 403. ✔
  7. Matriz por API para 7 cuentas: coincide con la sección 10 de arc42 (p. ej. felipe: nómina y empleados 200, caja e inventario 403; laura: caja e inventario 200, nómina 403). Sin sesión: 401. Código 2FA incorrecto: 401. Cuenta inactiva (`carolina.rios`): 401. ✔
- **Jira:** CER-49 (J2) y CER-50 (J3) a Listo; CER-51 (J4), CER-54 (J7), CER-55 (J8), CER-17 (HU-07) y CER-18 (HU-08) a En curso. Creada **CER-57** (F9, subtarea de CER-13) en En curso, porque falta F9.6 (los secretos de GitHub).

**Hallazgos (para el dueño de cada archivo, no corregidos):**
- `personal.js` (Felipe): después de aprobar la nómina y de volver a entrar, la tarjeta «Nómina de octubre» sigue mostrando la etiqueta «Borrador» aunque la API devuelve `APROBADA`. El total sí se actualiza.
- `index.html` (pie del menú lateral): sigue diciendo «v0.3 · Sprint 1». La pantalla de login ya dice «Sprint 2».

**Evidencia:** pruebas y cobertura descritas arriba; el servidor local se detuvo al terminar.

**Decisiones:** ninguna.

**Pendientes:** que Felipe revise la etiqueta de estado y el texto «Sprint 1»; falta la prueba manual de HU-05 (PR #8) y la del despliegue en Azure; J4.3; J6.

**Bloqueos / necesito de otros:** Camilo (PR #8 y los secretos de GitHub).

**Siguiente paso:** J5.2 cuando Camilo actualice el PR #8.

**Commits/PR:** `ba9c08d` (README).

## 2026-11-19 · Tarea J5.2 · Fusión de F7, D-012 y evidencias de reuniones · Terminada
**Hecho:**
- Revisadas y **fusionadas** tres ramas, con el CI en verde, sin marcas de IA y sin conflictos:
  - `feature/HU-04-autorizacion` (Felipe, F7: tres pruebas de autorización por módulo en `PersonalNominaApiTests` y su bitácora).
  - `feature/HU-06-seguridad-activa` (Iván, I5.9: D-012 de seguridad activa y su bitácora).
  - `docs/sprint-2-evidencias-reuniones` (Camilo: documento `.docx` de evidencias de reuniones Scrum y 12 capturas en `docs/sprint-2/evidencias/`, más su decisión).
- **Colisión de numeración:** Camilo (23:25) e Iván (23:26) tomaron D-012. Se dejó D-012 para la seguridad activa (se cita en otros archivos) y la decisión de Camilo se **renumeró a D-013** (archivo, título y una nota). Es un cambio al archivo de otro integrante, hecho por el integrador para resolver la colisión.
- Revisadas las capturas de Camilo (el repositorio es público): son páginas públicas de GitHub y una terminal; la única información personal es el nombre de usuario de Windows de Camilo en `tests.png`.
- PR #8 (HU-05, Camilo): sigue abierto y retenido, sin cambios desde mi comentario.

**Evidencia:** después de fusionar, `dotnet test CasinoErp.sln` → 260 unitarias + 57 de integración, 0 fallos. Revisión de marcas de IA sin coincidencias.

**Decisiones:** D-013 (renumeración, ver arriba). D-013 queda en estado `Propuesta`: el dueño de `docs/` (Jorge) debe aprobarla.

**Pendientes:** que Camilo actualice el PR #8; aprobar D-013; cinco capturas que solo puede tomar el equipo (Jira y las reuniones de Teams, según D-013).

**Bloqueos / necesito de otros:** Camilo, el PR #8 y los secretos de GitHub.

**Siguiente paso:** J5.2 cuando Camilo actualice el PR #8.

**Commits/PR:** fusiones de las tres ramas y la renumeración de D-013 en este commit.

## 2026-11-18 · Tarea J5.2 · Fusión de la seguridad de Iván · Terminada (HU-05 de Camilo retenida)
**Hecho:**
- Revisados los 8 PR abiertos hacia `develop` (#4 a #11), con el CI en verde en todos y sin marcas de IA. Las ramas de Iván están encadenadas (cada una contiene a la anterior).
- **Fusionada** `feature/HU-06-front` (la punta de la cadena, que incluye los PR #4, #5, #6, #7, #9 y #10) → `cce0aa1`. GitHub los marcó como fusionados.
- **Fusionada** `feature/HU-06-arc42-seguridad` (PR #11) → `2932d93`.
- **No fusionada** `feature/HU-05-alertas-stock` (PR #8, Camilo): al probarla junto con la seguridad activa fallan 4 pruebas de integración de `InventarioComprasApiTests` con 401, porque llaman a la API sin sesión. Comentado en el PR con lo que falta (rebase, `ClienteComoAsync`, `[AutorizarModulo]` en controladores nuevos y volver a probar).
- Con la seguridad activa, `NominaController` y `EmpleadosController` ya llevan `[AutorizarModulo]` (los puso Iván en I5): queda cubierta la parte de código de F7 de Felipe.
- J7.4 parcial: la sección 10 de Iván coincide con el bloque Nómina de Felipe (resultado en `docs/sprint-2/guion-video.md`).

**Evidencia:** después de fusionar, `dotnet test CasinoErp.sln` → 260 unitarias + 54 de integración, 0 fallos. Probada también la combinación con el PR #8: 271 unitarias + 4 integración fallidas (por eso se retuvo). La cobertura del dominio la comprobó el CI de cada rama (el comando local no se pudo repetir con este SDK).

**Decisiones:** ninguna nueva (D-009). Iván crea D-012 con el hash `cce0aa1`.

**Pendientes:** que Camilo actualice el PR #8; avisar a Iván (D-012, CER-43, CER-46 y CER-16), a Felipe (F7) y a Camilo (C7); el resto de J7.4; J4.3; J6.

**Bloqueos / necesito de otros:** de Camilo, el PR #8 corregido y los secretos de GitHub.

**Siguiente paso:** J5.2 cuando Camilo actualice el PR #8.

**Commits/PR:** `cce0aa1`, `2932d93`; PR #4 a #7 y #9 a #11 fusionados.

## 2026-11-18 · Tareas J2-B, J3-B y J4-B · Terminadas (J4.3 pendiente)
**Hecho:**
- **F9 resuelta:** Felipe asignó Contributor y *RBAC Administrator* con condición sobre la suscripción (D-011). Verificado con `az role assignment list`.
- **J2-B:** `terraform apply` del bootstrap → `rg-pitboss-tfstate`, `stpitbosstfjfie3s` y el contenedor `tfstate` (4 recursos). `Storage Blob Data Contributor` asignado a Jorge sobre la cuenta; se verificó el acceso al contenedor. El `terraform.tfstate` del bootstrap está copiado en `C:\Users\Asus M3604Y\.pitboss-terraform\bootstrap.tfstate` (fuera del repo).
- **J3-B:** `init` con el backend remoto e importados los 7 recursos: grupo, plan `plan-pitboss-erp`, web app, servidor SQL, base `CasinoErp` y las reglas `MacFelipe` y `PermitirServiciosAzure`. Se corrigió `main.tf` para igualar a Azure (commit `03f6abd`). Plan final: `1 to add, 1 to change, 0 to destroy` (clave JWT, tres app settings nuevos y `ip_restriction_default_action`). No se aplicó.
- **J4-B:** la App Registration falló (`Insufficient privileges`, directorio de la universidad). Se creó la identidad administrada `id-github-pitboss-terraform` con credencial federada para `repo:camilo1503/Casino-ERP-Repositorio:pull_request`, con Contributor sobre `rg-pitboss-erp` y Storage Blob Data Contributor sobre la cuenta del state.
- Fusionadas en `develop`: la rama de Felipe con F9 y D-011 (`1bf6a6f`) y `feature/HU-07-bootstrap-state` (`9be6eec`).

**Evidencia:** CI de ambas ramas en verde. Después de fusionar, `dotnet test CasinoErp.sln` → 210 unitarias + 34 de integración, 0 fallos. `terraform validate` y `fmt -check` correctos. Revisión de marcas de IA sin coincidencias.

**Decisiones:**
- Opción B (identidad administrada) para el OIDC, porque Entra ID no deja registrar aplicaciones.
- `https_only` se deja en `false` (igual que Azure); subirlo a `true` queda como mejora aparte.
- Los valores personales (correo del administrador SQL y la IP `MacFelipe`) quedan solo en `local.tfvars` (ignorado por Git). Consecuencia: el `plan` del CI propondrá diferencias en esos valores hasta decidirlo; ver pendientes.

**Pendientes:**
- Que Camilo cargue los 5 secretos en GitHub (valores abajo) y probar el workflow con un PR (J4.3 y captura).
- Decidir si el correo del administrador SQL y la IP de Felipe pueden ir al repositorio, para que el plan del CI dé cero diferencias.

**Valores de los secretos (son identificadores, no credenciales):** `AZURE_CLIENT_ID` = `66213a21-fc51-4a76-86bd-1748718bfc34`; `AZURE_TENANT_ID` = `07da67a0-1f43-4e8c-977f-5f88b6470ee6`; `AZURE_SUBSCRIPTION_ID` = `12f1bc21-94a3-4b28-9a90-8224d1e5fcc8`; `TFSTATE_RG` = `rg-pitboss-tfstate`; `TFSTATE_SA` = `stpitbosstfjfie3s`.

**Bloqueos / necesito de otros:** de Camilo, la carga de los secretos de GitHub.

**Siguiente paso:** J4.3 cuando estén los secretos; J6 cuando estén en `develop` los PR de Iván y Camilo.

**Commits/PR:** `03f6abd`, `1bf6a6f`, `9be6eec`.

## 2026-11-18 · Tarea J5.2 · Revisión de ramas de Felipe · Terminada
**Hecho:**
- No hay PR abiertos hacia `develop`. Felipe empujó cuatro ramas encadenadas (cada una contiene a la anterior): `feature/HU-04-empleados-api` (F4), `-nomina-api` (F5), `-front` (F6) y `-arc42-nomina` (F8).
- Se revisó la última, que contiene las cuatro:
  - CI en verde.
  - Sin marcas de IA en sus commits.
  - Solo toca archivos del módulo de Felipe (controladores, configuraciones, repositorios, semillas, módulos DI, `personal.js`, pruebas de integración de personal y nómina, arc42 de Nómina y ADR-005) y su bitácora.
  - Se fusiona sin conflictos.
  → **Fusionada** en `develop` (`04e37c9`).
- Las ramas intermedias quedan incluidas y no hace falta fusionarlas. `feature/HU-04-reglas-liquidacion` sigue obsoleta (F2 entró por `nomina-motor`).
- Tablero actualizado: Felipe tiene hechas F1–F6 y F8; F7 espera la I5 de Iván.
- Con `docs/arc42/05-bloques/nomina.md` ya en `develop`, el enlace del §5 de arc42 funciona y J7.4 solo espera la sección 10 de Iván.

**Evidencia:** después de fusionar, `dotnet test CasinoErp.sln` → 210 unitarias + 34 de integración (16 nuevas), 0 fallos. Revisión de marcas de IA sin coincidencias.

**Decisiones:** ninguna nueva (D-009).

**Pendientes:** que Felipe haga F9; Iván y Camilo no tienen ramas en el remoto todavía.

**Bloqueos / necesito de otros:** F9 de Felipe; las secciones de Iván y Camilo para J7.4.

**Siguiente paso:** J5.2 en la próxima sesión.

**Commits/PR:** `04e37c9`.

## 2026-11-18 · Tarea J5.2 · Fusión de ramas de Jorge · Terminada
**Hecho:**
- `gh` quedó con sesión (cuenta JorgeDLZ). No hay PR abiertos hacia `develop`.
- Se revisó el CI de las cuatro ramas de Jorge: todas en verde.
- Fusionadas en `develop` con `--no-ff`, sin conflictos:
  - `docs/ajuste-acceso-azure-j2` (F9, D-010 y bitácora) → `10674bf`.
  - `docs/HU-08-arc42-jorge` (arc42 y ADR 001, 003 y 004) → `1c9546b`.
  - `docs/sprint-2-scrum` (backlog, retrospectiva, investigación de Terraform y guion) → `fe5141a`.
- **No se fusionó** `feature/HU-07-bootstrap-state` (J2-A, J3-A y J4-A): el workflow `terraform-plan.yml` fallaría en cada PR que cambie un `.tf` mientras no existan los secretos de Azure. Se fusiona tras la parte B.
- Tablero de `desarrollo/README.md` actualizado.

**Evidencia:** después de fusionar, `dotnet test CasinoErp.sln` → 210 unitarias + 18 de integración, 0 fallos. Revisión de marcas de IA (AGENT.md §4.1) sin coincidencias. El CI de `develop` terminó en verde después del push.

**Decisiones:** ninguna nueva (D-009).

**Pendientes:** ver "Pendientes abiertos".

**Bloqueos / necesito de otros:** F9 de Felipe.

**Siguiente paso:** avisar a Felipe por el chat del grupo que F9 ya está en `develop`; J5.2 en la próxima sesión.

**Commits/PR:** `10674bf`, `1c9546b`, `fe5141a`.

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
