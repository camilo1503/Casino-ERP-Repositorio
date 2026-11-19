# D-010 · La asignación de roles en Azure pasa al plan de Felipe (F9) y J2–J4 se parten en partes A y B

- **Estado:** Aceptada · **Autor:** Jorge (agente de Jorge) · **Fecha:** 2026-11-18
- **Afecta a:** Felipe (`Plan-Felipe.md`, nueva tarea F9), Jorge (`Plan-Jorge.md`, J2–J4 y orden de trabajo)
- **Plan / tarea:** Plan-Jorge · J2 · Plan-Felipe · F9

## Contexto
La suscripción de Azure donde están `rg-pitboss-erp` y `pitboss-erp-8szxa` es de Felipe (usuario de GitHub `Flashtwiice`). Al iniciar J2 se comprobó con `az role assignment list` que Jorge (`jalcidesdiaz@ucundinamarca.edu.co`) solo tiene **Contributor sobre la web app `pitboss-erp-8szxa`**: no ve el grupo de recursos ni puede crear `rg-pitboss-tfstate`, importar el resto de recursos ni asignar roles. Solo quien es Owner de la suscripción puede darle esos permisos.

## Opciones consideradas
1. **Dejarlo como una nota en el plan de Jorge:** nadie lo ve en el plan de quien tiene que hacerlo y J2–J6 quedan bloqueadas sin dueño del desbloqueo.
2. **Tarea explícita en el plan de Felipe (elegida):** Felipe es el único que puede hacerlo; queda con casillas verificables y en su bitácora.

## Decisión
- Se agrega la **tarea F9** a `Plan-Felipe.md`: asignar a Jorge **Contributor + User Access Administrator** (o Owner) sobre la **suscripción completa**, verificarlo y avisar. F9.6 le pide además cargar los secretos de GitHub para el workflow de J4.
- En `Plan-Jorge.md`, J2, J3 y J4 se parten en **parte A** (escribir y validar el código sin Azure: `init -backend=false`, `validate`, `fmt`) y **parte B** (todo lo que toca Azure). Hasta que llegue F9 el orden es J2-A → J3-A → J4-A → J7 → J8.

## Consecuencias
- Felipe tiene una tarea más (pequeña, sin código ni PR); conviene hacerla antes de F4.
- J6 (despliegue) y J9 (cierre) siguen dependiendo de la parte B; si F9 no se hace a tiempo, el despliegue en Azure del Sprint 2 está en riesgo.
- Se editó el plan de otro integrante porque Jorge, que dirige al agente, lo pidió en el chat (AGENT.md §1 y §9); Felipe debe leer F9 y confirmarla en su bitácora.

## Aprobación
- Dueño del plan afectado: Felipe · pendiente de lectura · Jorge la ordenó el 2026-11-18
