# Sprint 2 · Sprint Backlog

**Sprint:** "CER Sprint 2" (id 35 en Jira) · del 18-11-2026 al 02-12-2026 · Jira: https://jorgediazlozano1170709.atlassian.net/jira/software/projects/CER/boards/2/backlog

## Sprint Goal

> Al final del Sprint 2, PitBoss ERP tendrá el CRUD de Talento Humano y de Proveedores (además de los módulos del Sprint 1, que cubren CRM/Perfiles, Inventario y Facturación/Caja), el **motor de cálculo de nómina** (HU-04), la **alerta automática de stock crítico** con orden de compra en borrador (HU-05) y el **control de acceso por roles** con autenticación de dos factores (HU-06). La infraestructura de Azure quedará descrita en Terraform con `terraform plan` en cada PR (HU-07) y las decisiones de seguridad y despliegue documentadas en arc42 (HU-08).

## Capacidad

La llena el equipo en el Sprint Planning; aquí no se inventan cifras.

| Integrante | Días disponibles en el sprint | Velocidad del Sprint 1 (SP) |
|---|---|---|
| Felipe | _por completar_ | _por completar_ |
| Iván | _por completar_ | _por completar_ |
| Camilo | _por completar_ | _por completar_ |
| Jorge | _por completar_ | _por completar_ |

## Historias del sprint

Total: **37 SP**.

| HU | Historia | SP | Responsable | Jira (historia) | Épica |
|---|---|---|---|---|---|
| HU-04 | Cálculo de nómina (y CRUD de empleados) | 8 | Felipe | CER-13 | CER-9 Talento Humano y Nómina |
| HU-05 | Alerta de stock crítico | 5 | Camilo | CER-14 | CER-10 Inventario y Compras |
| — | CRUD de Proveedores y orden de compra automática | 3 | Camilo | CER-15 | CER-10 |
| HU-06 | Asignación de roles (autenticación y autorización) | 5 | Iván | CER-16 | CER-11 Seguridad y Control de Acceso |
| HU-07 | Infraestructura como código (Terraform y `terraform plan` en PR) | 13 | Jorge | CER-17 | CER-12 Infraestructura y Arquitectura |
| HU-08 | Documentación arc42 de seguridad y despliegue | 3 | Jorge, con todos | CER-18 | CER-12 |
| — | Coordinación (J0, J1, J5, J8, J9) y CER-8 (borrar el código Python) | — | Jorge | CER-19 y CER-8 | CER-12 |

Historias de usuario de la guía:
- **HU-04:** "Como jefe de RR. HH., quiero poder calcular la nómina mensual de los empleados activos, aplicando deducciones y parafiscales según su tipo de contrato (ej. cotizaciones a seguridad social, impuestos), para generar reportes de pago precisos."
- **HU-05:** "Como administrador de inventario, quiero que el sistema me alerte automáticamente cuando el nivel de existencias de un producto baje del umbral mínimo predefinido, para evitar quiebres de stock."
- **HU-06:** "Como administrador del sistema, quiero poder asignar roles de usuario (ej. 'Admin', 'Compras', 'RR. HH.') para controlar el acceso a funcionalidades y módulos específicos."
- **HU-07 y HU-08:** el texto completo está en las historias CER-17 y CER-18 de Jira.

## Tareas por plan

Una subtarea de Jira por tarea del plan. Los planes completos están en `desarrollo/planes/`.

| Plan | Responsable | Tareas | Subtareas Jira |
|---|---|---|---|
| Felipe | Felipe | F0 Arranque · F1 Dominio de empleados (contrato C4) · F2 Parámetros y reglas de liquidación · F3 `PayrollEngine` y `NominaService` · F4 Persistencia y API de empleados · F5 Persistencia y API de nómina · F6 Front `personal.js` · F7 Autorización · F8 arc42 y cierre · **F9 Acceso de Jorge a Azure (D-010)** | CER-20 a CER-28 (F9 aún sin subtarea) |
| Camilo | Camilo | C0 Arranque · C1 Alerta de stock crítico (dominio) · C2 API de alertas · C3 Proveedores (dominio) · C4 Órdenes de compra · C5 API de proveedores y compras · C6 Front · C7 Autorización · C8 arc42 y cierre | CER-29 a CER-37 |
| Iván | Iván | I0 Arranque · I1 `Rol`, `Modulos` y `MatrizAcceso` · I2 `Usuario` y autenticación · I3 Gestión de cuentas y roles · I4 Infraestructura y cuentas de demo · I5 JWT y autorización (hito "Seguridad activa") · I6 Front · I7 arc42 sección 10 y ADR-002 · I8 Cierre y demo | CER-38 a CER-46 |
| Jorge | Jorge | J0 Fase 0 · J1 Jira · J2 Backend remoto del state · J3 Terraform con import · J4 Workflow `terraform plan` · J5 Integración continua · J6 Despliegue · J7 arc42 · J8 Artefactos Scrum · J9 Cierre | CER-47 a CER-56 |

## Definition of Done (AGENT.md §7)

- [ ] `dotnet build` sin advertencias nuevas y `dotnet test CasinoErp.sln` en verde.
- [ ] Cobertura de líneas de `CasinoErp.Dominio` ≥ 80 % (el CI lo exige).
- [ ] Pruebas nuevas para cada regla y mensaje del plan.
- [ ] Probado a mano en el navegador cuando toca el front (anotar qué se probó).
- [ ] Sección de arc42 de su módulo actualizada si cambió la arquitectura.
- [ ] Bitácora actualizada (avance, decisiones, pendientes).
- [ ] Tarjeta de Jira movida y enlazada al PR.

## Hitos

| Hito | Estado al 2026-11-18 |
|---|---|
| Fase 0 en `develop` | Hecho (`8fff1ce`) |
| Contrato C4 en `develop` | Hecho (`f083b68`) |
| Seguridad activa (I5) | Pendiente |
| Congelamiento | Pendiente |
| Sprint Review | Pendiente (guion en `desarrollo/README.md`) |
