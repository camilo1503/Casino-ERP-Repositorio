# D-003 · Los valores legales de nómina 2026 se guardan como parámetros por año

- **Estado:** Aceptada · **Autor:** Equipo (plan inicial) · **Fecha:** 2026-11-18
- **Afecta a:** Felipe

## Contexto
Para 2026, el salario mínimo es $1.750.905 (Decreto 1469 de 2025) y el auxilio de transporte $249.095 (Decreto 1470 de 2025). Existe una disputa judicial (suspensión provisional y Decreto 0159 de 2026), así que estos valores pueden cambiar.

## Decisión
La entidad `ParametrosNomina` tiene un registro por año, editable con `PUT /api/nomina/parametros/{anio}`. La semilla carga `ParametrosNomina.Decreto2026()` y las pruebas usan esos mismos valores. Los aportes del empleador no aplican la exoneración del Art. 114-1 del Estatuto Tributario (simplificación documentada).

## Consecuencias
Si el valor cambia, se actualiza el parámetro sin tocar código.
