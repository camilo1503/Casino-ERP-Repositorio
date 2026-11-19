# ADR-001 · Monolito modular en vez de un microservicio de nómina

- **Estado:** Aceptada · **Fecha:** 2026-11-18 · **Origen:** decisión D-002

## Contexto
La guía del sprint habla del "microservicio de Nómina". El repositorio es un monolito en capas que se despliega en un único App Service F1 gratuito. Un servicio aparte exigiría otro recurso en Azure, otra base y comunicación entre servicios, sin que el tamaño del equipo ni la carga lo justifiquen.

## Decisión
La nómina es un **módulo aislado dentro del monolito**: carpeta propia (`Dominio/Nominas`), interfaces propias, controlador propio, y solo depende de `IEmpleadoRepositorio`. El motor de cálculo (`PayrollEngine`) es puro, sin base de datos ni reloj.

## Consecuencias
- No hay infraestructura extra ni costo adicional en Azure.
- La frontera del módulo queda explícita en la sección 5, así que extraerlo como servicio más adelante implicaría mover la carpeta, exponer un contrato HTTP en lugar de `IEmpleadoRepositorio` y separar su persistencia.
- Mientras sea un monolito, un error grave en un módulo puede tumbar todo el proceso.
