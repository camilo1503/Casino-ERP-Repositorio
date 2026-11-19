# Sprint 2 · Retrospectiva

> Plantilla preparada antes de que termine el sprint. Cada integrante escribe lo suyo; aquí no se inventa contenido ajeno.

## Qué salió bien

- **Felipe:** _¿Qué de tu forma de trabajar o del plan te ayudó más a avanzar?_
- **Iván:** _¿Qué de tu forma de trabajar o del plan te ayudó más a avanzar?_
- **Camilo:** _¿Qué de tu forma de trabajar o del plan te ayudó más a avanzar?_
- **Jorge:** _¿Qué de tu forma de trabajar o del plan te ayudó más a avanzar?_

## Qué se puede mejorar

- **Felipe:** _¿Qué harías distinto en el próximo sprint, y por qué?_
- **Iván:** _¿Qué harías distinto en el próximo sprint, y por qué?_
- **Camilo:** _¿Qué harías distinto en el próximo sprint, y por qué?_
- **Jorge:** _¿Qué harías distinto en el próximo sprint, y por qué?_

## Impedimentos

- **Felipe:** _¿Qué te frenó y cuánto tiempo?_
- **Iván:** _¿Qué te frenó y cuánto tiempo?_
- **Camilo:** _¿Qué te frenó y cuánto tiempo?_
- **Jorge:** Desde el 2026-11-18 la parte B de J2, J3 y J4 (y J6) dependía de que Felipe asignara los roles de Azure (tarea F9, D-010). _Completar con la fecha en que llegó el acceso._

## Tarea 1 de la guía: ¿por qué la HU-04 pudo subestimarse?

HU-04 se estimó en 8 SP como un "cálculo de nómina". El trabajo real incluyó el CRUD de empleados y un conjunto de reglas legales que no se ven en el texto de la historia. La evidencia está en la bitácora de Felipe (`desarrollo/bitacoras/felipe.md`), cuyas entradas de F2 y F3 describen las pruebas de nómina escritas para cubrir estos casos de borde:

1. **Auxilio de transporte en el límite exacto de 2 SMMLV:** ¿se paga con exactamente 2 salarios mínimos o solo por debajo?
2. **Fondo de solidaridad desde 4 SMMLV:** es una deducción que solo aparece para salarios altos.
3. **Recargo nocturno que saca al crupier del auxilio:** el recargo suma al devengado y puede superar el tope del auxilio.
4. **Prestación de servicios sin aportes:** es otro tipo de contrato con otras reglas, lo que obliga a tener reglas intercambiables (`IReglaLiquidacion`).
5. **El decreto del salario 2026 en disputa judicial:** los valores legales no pueden ir fijos en el código, así que se guardan como parámetros por año editables (D-003).

La historia hablaba de "deducciones y parafiscales según el tipo de contrato", pero no listaba esos casos. Cada uno era una regla de negocio por entender, validar y probar, y el tamaño solo se vio al empezar el trabajo.

## Regla de estimación para el Sprint 3

> **Toda HU con norma legal (impuestos, facturación electrónica DIAN, nómina) suma un spike de 2 SP en el Sprint Planning y se estima después del spike, con una lista explícita de casos de borde.**

## Acciones de mejora

| Acción | Dueño | Estado |
|---|---|---|
| Spike de 2 SP antes de estimar HU con norma legal (regla de arriba) | Todos, en el Sprint 3 | Propuesta |
| CI con `terraform plan` en cada PR a `develop` | Jorge | Escrito en la parte A de J4; falta probarlo (parte B) |
| Pasar de `EnsureCreated` a EF Core Migrations (ADR-004) | Por asignar | Propuesta para el Sprint 3 |
