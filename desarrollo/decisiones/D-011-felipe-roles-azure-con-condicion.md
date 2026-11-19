# D-011 · Jorge recibe Contributor y un administrador de roles con condición, en vez de User Access Administrator

- **Estado:** Aceptada
- **Autor:** Felipe (agente de Felipe)
- **Fecha:** 2026-11-18
- **Afecta a:** Jorge (Plan-Jorge J2-B, J3-B, J4-B y J6), Plan-Felipe F9 (D-010)
- **Plan / tarea:** Plan-Felipe · F9

## Contexto
La tarea F9 (D-010) pide asignarle a Jorge (`jalcidesdiaz@ucundinamarca.edu.co`), sobre la suscripción *Azure for Students* `12f1bc21-94a3-4b28-9a90-8224d1e5fcc8`, los roles **Contributor + User Access Administrator**, o **Owner**.

**User Access Administrator** sobre la suscripción permite asignar **cualquier** rol a cualquier identidad, incluido Owner. En la práctica equivale a ser dueño de la suscripción de un integrante. Además, la suscripción es personal y su crédito de estudiante se consume con los recursos que se creen. Según Plan-Jorge, Jorge solo necesita asignar roles para dos cosas: `Storage Blob Data Contributor` sobre el state (J2.3) y los roles del workflow de GitHub con OIDC (J4.1).

## Opciones consideradas
1. **Owner o Contributor + User Access Administrator (plan original):** cubre todo, pero permite escalar a Owner y dar acceso a terceros.
2. **Contributor + Role Based Access Control Administrator con condición (elegida):** es la "delegación con condiciones" que recomienda Microsoft. Jorge puede asignar y quitar **solo** los roles Reader, Contributor y Storage Blob Data Contributor; no puede darse Owner ni User Access Administrator.

## Decisión
El 2026-11-18 a las 21:20 se asignaron a Jorge, sobre la suscripción completa:
- **Contributor**;
- **Role Based Access Control Administrator**, con la condición (versión 2.0) de que solo puede crear o borrar asignaciones de estos roles:
  - Reader (`acdd72a7-3385-48ef-bd42-f606fba81ae7`);
  - Contributor (`b24988ac-6180-42a0-ab88-20f7382dd24c`);
  - Storage Blob Data Contributor (`ba92f5b4-2d11-453d-a403-e96b0029c9fe`).

Se conserva su Contributor anterior sobre la web app `pitboss-erp-8szxa`. Verificado con `az role assignment list --all --assignee 7d128dfa-90ee-4a1b-adb0-bf53f0e8d557`.

## Consecuencias
- Jorge puede hacer J2-B (crear `rg-pitboss-tfstate`, la cuenta de storage y asignarse Storage Blob Data Contributor), J3-B (leer e importar los recursos de `rg-pitboss-erp`) y J4-B (asignar Reader o Contributor a la identidad del workflow).
- Si Jorge necesita asignar **otro** rol (por ejemplo, `User Access Administrator` a la identidad del workflow), Azure lo rechaza con `AuthorizationFailed`. En ese caso Jorge lo pide en el chat y Felipe lo asigna, o se amplía la condición con una nueva decisión.
- El registro de la aplicación en Microsoft Entra ID para OIDC (J4.1) **no** depende de estos roles: lo controla el directorio de la universidad y puede estar restringido para estudiantes.
- La carga de los secretos de GitHub (F9.6) sigue pendiente. La hace quien administra el repositorio cuando Jorge entregue los valores.

## Aprobación
- Dueño de la suscripción: Felipe · 2026-11-18 · Aprobada
- Afectado: Jorge · pendiente de lectura
