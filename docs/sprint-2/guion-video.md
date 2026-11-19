# Guion del video de arc42 (Tarea 2 de la guía, máximo 60 s)

| Tiempo | Qué se muestra | Qué se dice |
|---|---|---|
| 0–15 s | Sección 5: `05-bloques/nomina.md`, el bloque Nómina y su frontera | El módulo de nómina está aislado: expone sus endpoints y solo depende de `IEmpleadoRepositorio` y de su persistencia. |
| 15–40 s | Sección 10: la matriz de roles y `[AutorizarModulo(Modulos.Rrhh)]` | Solo el rol con acceso a RR. HH. puede llamar a `NominaController`; el resto recibe 403. |
| 40–60 s | Las dos secciones lado a lado | Cada control de la sección 10 tiene su lugar en la sección 5: el atributo está en el controlador del bloque Nómina. Las dos secciones son coherentes. |

**Quién graba:** _por definir._ **Enlace del video:** _pendiente._

## Resultado de la revisión cruzada (J7.4)

Pendiente: se hace cuando `10-conceptos-seguridad.md` (Iván) y `05-bloques/*.md` (Felipe, Camilo e Iván) estén en `develop`. Las incoherencias se anotarán como comentarios en el PR de su dueño y el resultado quedará aquí.
