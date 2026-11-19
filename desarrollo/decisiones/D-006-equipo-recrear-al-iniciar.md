# D-006 · `BaseDatos:RecrearAlIniciar` para aplicar el esquema nuevo mientras se usa `EnsureCreated`

- **Estado:** Aceptada · **Autor:** Equipo (plan inicial) · **Fecha:** 2026-11-18
- **Afecta a:** todos

## Contexto
`EnsureCreated` no modifica tablas existentes. Las bases locales y la de Azure se crearon con el modelo del Sprint 1, así que las tablas nuevas no aparecerían.

## Decisión
Se agrega la opción `BaseDatos:RecrearAlIniciar` (por defecto `false`). Cuando está en `true`, la base se borra y se recrea con las semillas al arrancar. En local se usa con `$env:BaseDatos__RecrearAlIniciar="true"`; en Azure se activa una sola vez en el despliegue del Sprint 2 y se vuelve a `false`. La migración a EF Core Migrations queda como deuda técnica para el Sprint 3 (ADR-004).

## Consecuencias
Los datos de la base de Azure son de demo y se pierden al recrear, lo cual es aceptable.
