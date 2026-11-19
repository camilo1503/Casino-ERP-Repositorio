# ADR-005 · Los valores legales de la nómina son parámetros por año, editables por API

- **Estado:** Aceptada
- **Fecha:** 2026-11-18
- **Autor:** Felipe (bloque Nómina, HU-04)
- **Origen:** decisión de equipo [D-003](../../../desarrollo/decisiones/D-003-equipo-parametros-nomina.md)
- **Relacionado:** [bloque Nómina](../05-bloques/nomina.md)

## Contexto

El cálculo de la nómina depende de valores que fija el Gobierno cada año: el salario mínimo (SMMLV), el auxilio de transporte y las tasas de seguridad social y parafiscales.

Para 2026, el salario mínimo es **$1.750.905** (Decreto 1469 de 2025) y el auxilio de transporte **$249.095** (Decreto 1470 de 2025). Esos decretos tienen una **disputa judicial abierta**: hubo una suspensión provisional y luego el Decreto 0159 de 2026. Los valores pueden cambiar en medio del año.

Si los valores estuvieran escritos en el código, cada cambio legal obligaría a modificar el motor, volver a probar, compilar y desplegar. Además, no se podría recalcular la nómina de un año con los valores de otro.

## Decisión

1. La entidad `ParametrosNomina` (`src/CasinoErp.Dominio/Nominas/ParametrosNomina.cs`) guarda **un registro por año**:
   - salario mínimo y auxilio de transporte (en pesos, `long`);
   - tasas del empleado (`decimal`): salud 4 %, pensión 4 %, fondo de solidaridad 1 %;
   - recargo nocturno 35 % y retención en prestación de servicios 10 %;
   - aportes del empleador: salud 8,5 %, pensión 12 %, ARL 0,522 % (riesgo I), caja de compensación 4 %, ICBF 3 % y SENA 2 %.
2. `ParametrosNomina.Decreto2026()` tiene los valores vigentes. La semilla `SemillaNomina` los carga si la tabla está vacía, y las pruebas usan esos mismos valores.
3. Los parámetros se consultan y editan por API:
   - `GET /api/nomina/parametros/{anio}`;
   - `PUT /api/nomina/parametros/{anio}`. El año de la ruta manda sobre el del cuerpo, y guardar es un *upsert* por año.
4. Al guardar, `NominaService.GuardarParametrosAsync` valida:
   - todas las tasas entre 0 y 1 → "Las tasas deben estar entre 0 y 1.";
   - salario mínimo > 0 → "El salario mínimo debe ser mayor que cero.";
   - auxilio ≥ 0 → "El auxilio de transporte no puede ser negativo.";
   - año ≥ 2020 → "El año no es válido.".
5. `NominaService.CalcularAsync(anio, mes)` usa los parámetros **del año de la nómina**. Si no existen, responde "No hay parámetros de nómina para {anio}.". El motor (`PayrollEngine`) los recibe como argumento y no los lee de ningún lado (SRP).
6. **Simplificación aceptada:** los aportes del empleador se calculan completos y **no** aplican la exoneración del Art. 114-1 del Estatuto Tributario. Esa norma libera de salud del empleador, SENA e ICBF a los trabajadores que ganan menos de 10 SMMLV.

## Consecuencias

**Positivas**
- Un cambio de salario mínimo o de tasa se aplica con un `PUT`, sin tocar código ni desplegar.
- Se pueden tener los parámetros de varios años y calcular cada nómina con los suyos.
- El motor es una función pura de (empleados, parámetros), fácil de probar con valores conocidos.

**Negativas / riesgos**
- Cualquiera con acceso al módulo **Turnos y nómina** (roles Gerente y Talento Humano, desde F7) puede cambiar valores legales. No hay historial de cambios: se registra solo el valor vigente del año.
- Cambiar los parámetros **no recalcula** las nóminas ya aprobadas. Es intencional: una nómina aprobada ya se pagó.
- Por la simplificación del Art. 114-1, el total de *aportes del empleador* queda sobrestimado para los salarios bajo 10 SMMLV. No afecta el neto que recibe el empleado.

**Deuda para el Sprint 3**
- Guardar un historial de cambios de los parámetros (quién, cuándo, valor anterior).
- Aplicar la exoneración del Art. 114-1 como una regla configurable.
- Pasar de `EnsureCreated` a EF Core Migrations (ADR-004) para cambiar el esquema de `ParametrosNomina` sin recrear la base.
