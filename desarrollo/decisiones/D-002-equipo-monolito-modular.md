# D-002 · Monolito modular en vez de un microservicio de nómina

- **Estado:** Aceptada · **Autor:** Equipo (plan inicial) · **Fecha:** 2026-11-18
- **Afecta a:** Felipe (Nómina), Jorge (arc42 §5 y §9)

## Contexto
La Tarea 2 de la guía habla del "microservicio de Nómina". El repo es un monolito en capas, desplegado en un solo App Service F1 gratuito.

## Decisión
La nómina es un **módulo aislado** dentro del monolito: tiene su propia carpeta `Dominio/Nominas`, sus interfaces y su controlador, y no depende de otros módulos salvo de `IEmpleadoRepositorio`. Se documenta como ADR-001 en `docs/arc42/09-decisiones.md`, con el camino para extraerlo como servicio en el futuro.

## Consecuencias
No hay infraestructura extra ni costo adicional en Azure. La sección 5 de arc42 muestra el bloque Nómina con su frontera explícita.
