# Desarrollo del Sprint 2 · PitBoss ERP

Esta carpeta organiza el trabajo del equipo en el Sprint 2. **Empieza por [`../AGENT.md`](../AGENT.md)**: ahí están las reglas obligatorias para personas y agentes.

## Sprint Goal

> Al final del Sprint 2, PitBoss ERP tendrá el CRUD de Talento Humano y de Proveedores (además de los módulos del Sprint 1, que cubren CRM/Perfiles, Inventario y Facturación/Caja), el **motor de cálculo de nómina** (HU-04), la **alerta automática de stock crítico** con orden de compra en borrador (HU-05) y el **control de acceso por roles** con autenticación de dos factores (HU-06). La infraestructura de Azure quedará descrita en Terraform con `terraform plan` en cada PR (HU-07) y las decisiones de seguridad y despliegue documentadas en arc42 (HU-08).

Guía oficial: [`referencias/guia-sprint-2.pdf`](referencias/guia-sprint-2.pdf) · Adaptación al casino: [`decisiones/D-001-equipo-adaptacion-casino.md`](decisiones/D-001-equipo-adaptacion-casino.md)

## Contenido de la carpeta

| Ruta | Qué es | Quién la edita |
|---|---|---|
| [`planes/Plan-Felipe.md`](planes/Plan-Felipe.md) | Talento Humano y Nómina (HU-04) | Felipe |
| [`planes/Plan-Ivan.md`](planes/Plan-Ivan.md) | Usuarios, roles y seguridad (HU-06) | Iván |
| [`planes/Plan-Camilo.md`](planes/Plan-Camilo.md) | Inventario, alertas y proveedores (HU-05) | Camilo |
| [`planes/Plan-Jorge.md`](planes/Plan-Jorge.md) | Fase 0, Terraform (HU-07), arc42 (HU-08), Scrum e integración | Jorge |
| [`contratos.md`](contratos.md) | Firmas compartidas entre módulos | Solo con una decisión aprobada |
| [`bitacoras/`](bitacoras/) | Avances, decisiones locales, pendientes y bloqueos de cada integrante | Cada quien la suya |
| [`decisiones/`](decisiones/) | Decisiones que afectan a más de un módulo (una por archivo) | Quien decide (archivo nuevo) |
| [`plantillas/`](plantillas/) | Plantillas de bitácora y decisión | Jorge |
| [`referencias/`](referencias/) | Guía del sprint | — |

El mockup de referencia es el front que ya está en el repo: `src/CasinoErp.Api/wwwroot/index.html`.

## Historias del sprint

| HU | Historia | SP | Responsable | Plan |
|---|---|---|---|---|
| HU-04 | Cálculo de nómina (+ CRUD de empleados) | 8 | Felipe | F0–F8 |
| HU-05 | Alerta de stock crítico | 5 | Camilo | C0–C2, C6–C8 |
| — | CRUD de Proveedores y orden de compra automática | 3 | Camilo | C3–C5 |
| HU-06 | Asignación de roles (autenticación y autorización) | 5 | Iván | I0–I8 |
| HU-07 | Infraestructura como código (Terraform + `terraform plan` en PR) | 13 | Jorge | J2–J4, J6 |
| HU-08 | Documentación arc42 de seguridad y despliegue | 3 | Jorge (consolida) + todos | J7, F8, C8, I7 |
| CER-8 | Borrar el código Python sobrante | — | Jorge | J0.9 |

**Jira:** https://jorgediazlozano1170709.atlassian.net/jira/software/projects/CER/boards/2/backlog. Las claves CER-NN del Sprint 2 se anotan aquí cuando Jorge las cree (J1).

| Elemento | Épica | Historia / tarea | Subtareas (una por tarea del plan) |
|---|---|---|---|
| HU-04 · Felipe | CER-9 Talento Humano y Nómina | CER-13 (8 SP) | F0–F8 = CER-20 … CER-28 |
| HU-05 · Camilo | CER-10 Inventario y Compras | CER-14 (5 SP) | C0, C1, C2, C6, C7, C8 = CER-29, 30, 31, 35, 36, 37 |
| Proveedores · Camilo | CER-10 | CER-15 (3 SP) | C3, C4, C5 = CER-32, 33, 34 |
| HU-06 · Iván | CER-11 Seguridad y Control de Acceso | CER-16 (5 SP) | I0–I8 = CER-38 … CER-46 |
| HU-07 · Jorge | CER-12 Infraestructura y Arquitectura | CER-17 (13 SP) | J2, J3, J4, J6 = CER-49, 50, 51, 53 |
| HU-08 · Jorge (+ todos) | CER-12 | CER-18 (3 SP) | J7 = CER-54 (F8, C8, I7 en sus historias) |
| Coordinación · Jorge | CER-12 | CER-19 (tarea) | J0, J1, J5, J8, J9 = CER-47, 48, 52, 55, 56; CER-8 (Python) dentro de CER-12 |

**Sprint activo:** "CER Sprint 2" (id 35), del 18-11-2026 al 02-12-2026, con el Sprint Goal de arriba. Contiene CER-8 y CER-13 a CER-19; las subtareas van dentro de su historia en el tablero (una búsqueda JQL `sprint = 35` solo muestra las 8 incidencias padre). El sprint vacío "CER Sprint 1" (id 1), que Jira creó por defecto, se dejó sin tocar.

Total del sprint: 37 SP. Al terminar una tarea del plan, mueve su subtarea a **Listo**; mientras trabajas en ella, a **En curso**.

## Olas de trabajo (qué se puede hacer en paralelo)

```
Ola 1 (día 1, todos a la vez)
  Jorge  : J0 Fase 0  ─────────────────────────────▶ (bloquea la parte de infraestructura/API/front de los demás)
  Felipe : F1 dominio de empleados (contrato C4) → F2 reglas → F3 motor
  Iván   : I1 matriz de acceso → I2 autenticación
  Camilo : C1 alertas (dominio) → C3 proveedores → C4 órdenes
Ola 2 (cuando la Fase 0 está en develop)
  Felipe : F4 API empleados → F5 API nómina        Camilo : C2 API alertas → C5 API compras
  Iván   : I3 usuarios (necesita C4) → I4 infraestructura
  Jorge  : J1 Jira → J2/J3 Terraform → J4 workflow
Ola 3 (hito "Seguridad activa")
  Iván   : I5 JWT + autorización en toda la API → todos hacen pull --rebase y anotan sus controladores (F7, C7)
Ola 4 (front, en paralelo)
  Felipe : F6 personal.js   Camilo : C6 inventario.js + compras.js   Iván : I6 seguridad.js
Ola 5 (cierre)
  Todos  : arc42 de su módulo (F8, C8, I7) → Jorge: J7 consolidación, J8 Scrum, J6 despliegue, J9 cierre
```

## Hitos

| Hito | Señal | Qué hacen los demás |
|---|---|---|
| Fase 0 en develop ✅ (2026-11-18, `8fff1ce`) | `decisiones/D-007-jorge-fase-0-lista.md` | `git pull --rebase origin develop`; empezar las tareas de infraestructura, API y front |
| Contrato C4 en develop ✅ (2026-11-18, `f083b68`) | PR de F1 fusionado | Iván empieza I3 |
| Seguridad activa | `decisiones/D-0NN-ivan-seguridad-activa.md` | `pull --rebase`; pruebas de integración con `ClienteComoAsync`; `[AutorizarModulo]` en los controladores nuevos |
| Congelamiento | Jorge lo anuncia en el tablero | Solo correcciones; todo en `develop` para desplegar |
| Sprint Review | — | Demo (ver abajo) |

## Tablero (lo actualiza Jorge, leyendo bitácoras y PR)

| Plan | Tarea en curso | Hechas | En revisión (PR) | Bloqueos |
|---|---|---|---|---|
| Felipe | F2 | F1 (contrato C4, `f083b68`) | — | — |
| Iván | I0 | — | — | — |
| Camilo | C0 | — | — | — |
| Jorge | J1 / J5 | J0 (Fase 0, `8fff1ce`) | — | — |

## Guion de la Sprint Review (aprox. 10 minutos)

| Min | Quién | Qué se muestra | Cuenta |
|---|---|---|---|
| 0–1 | Jorge | Sprint Goal, arquitectura (arc42 §5 nivel 1) y despliegue en Azure (§7) | — |
| 1–3 | Iván | **HU-06:** login con 2FA → el admin asigna el rol Cajero a Sebastián → Sebastián solo ve Caja e Inventario → 403 en `/api/nomina` | ivan.leon, sebastian.prada |
| 3–5 | Felipe | **HU-04:** Turnos y nómina → Calcular → Bruno Lara (recargo nocturno, sin auxilio) y Ana Torres (con auxilio) → Aprobar | felipe.cortes |
| 5–7 | Camilo | **HU-05:** compra de 25 fichas de $500.000 en caja → toast de stock crítico → alerta en Inventario → orden OC-0001 en borrador → enviar y recibir → la alerta desaparece | laura.gomez, daniel.ruiz |
| 7–9 | Jorge | **HU-07:** PR con cambio en `.tf` → check "Terraform plan" → state remoto. **HU-08:** recorrido por arc42 §7 y §10 | — |
| 9–10 | Todos | Preguntas | — |

Las cuentas de demo están en [`contratos.md` §C6](contratos.md). La contraseña es `Casino2026` y el código TOTP de demo es `482913`.
