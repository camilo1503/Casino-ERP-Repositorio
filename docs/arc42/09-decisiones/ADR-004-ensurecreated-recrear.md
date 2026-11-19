# ADR-004 · `EnsureCreated` y `RecrearAlIniciar` en vez de migraciones (deuda para el Sprint 3)

- **Estado:** Aceptada, con deuda técnica · **Fecha:** 2026-11-18 · **Origen:** decisión D-006

## Contexto
`EnsureCreated` crea el esquema solo si la base está vacía y nunca modifica tablas existentes. Las bases locales y la de Azure se crearon con el modelo del Sprint 1, así que las tablas del Sprint 2 no aparecerían.

## Decisión
Se agrega la opción `BaseDatos:RecrearAlIniciar` (por defecto `false`). Cuando es `true`, la API borra la base con `EnsureDeleted()` y la crea de nuevo con `EnsureCreated()` y las semillas (`ISemilla`). En local se activa con `$env:BaseDatos__RecrearAlIniciar="true"`. En Azure se activa una sola vez en el despliegue del Sprint 2 y se vuelve a `false`.

## Consecuencias
- Cada vez que cambia el modelo hay que recrear la base; los datos son de demostración, así que perderlos es aceptable ahora.
- **No sirve para datos reales.** Antes de tener datos de producción hay que pasar a EF Core Migrations. Queda como tarea del Sprint 3.
- Dejar `RecrearAlIniciar=true` en la nube borraría los datos en cada reinicio; Terraform lo fija en `false` por defecto.
