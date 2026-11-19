# D-004 · Extensión sin conflictos: `ISemilla`, módulos DI y el front dividido en `js/api/*.js`

- **Estado:** Aceptada · **Autor:** Equipo (plan inicial) · **Fecha:** 2026-11-18
- **Afecta a:** todos (contratos C1, C2 y C3)

## Contexto
Cuatro personas trabajando en paralelo chocarían en `CasinoErpDbContext.cs`, `ConfiguracionServicios.cs`, `DatosIniciales.cs`, `conexion-api.js` e `index.html`.

## Decisión
En la Fase 0, Jorge introduce:
- `ApplyConfigurationsFromAssembly`;
- un archivo `Modulo<Nombre>.cs` por módulo, con su llamada ya puesta;
- `ISemilla` para los datos de demo;
- la división de `conexion-api.js` en `nucleo.js`, `sprint1.js`, `inventario.js`, `seguridad.js`, `personal.js`, `compras.js` y `arranque.js`.

Las vistas nuevas del front se agregan desde JS asignando propiedades a los objetos globales del mockup, sin editar `index.html`.

## Consecuencias
Cada integrante trabaja en archivos propios. La Fase 0 bloquea la parte de infraestructura y front de los demás, así que tiene prioridad máxima.
