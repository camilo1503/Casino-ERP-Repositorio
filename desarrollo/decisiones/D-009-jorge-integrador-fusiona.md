# D-009 · El integrador busca y fusiona los PR hacia develop

- **Estado:** Aceptada · **Autor:** Jorge · **Fecha:** 2026-11-18
- **Afecta a:** todos (AGENT.md §4, Plan-Jorge J5.2)

## Contexto
Con cuatro personas trabajando en paralelo, esperar la aprobación cruzada de cada PR frena el trabajo. Jorge, como coordinador, pidió que su agente busque siempre los PR y los fusione.

## Decisión
- Solo el integrador (Jorge y su agente) fusiona a `develop`. Los demás abren el PR (o dejan la rama empujada con el enlace de comparación en su bitácora) y no fusionan.
- El integrador revisa los PR y ramas abiertos al inicio de cada sesión y al terminar cada tarea, y fusiona sin esperar otra aprobación cuando: el CI está en verde (backend, cobertura y marcas de IA); no hay conflictos y `dotnet test` pasa después de fusionar; y el PR respeta la DoD, los archivos permitidos y los contratos.
- Si no se cumple, no se fusiona y se deja un comentario con lo que falta.
- `main` solo recibe el PR de cierre del sprint.

## Consecuencias
Reemplaza la regla anterior de "una aprobación de otro integrante". La revisión de calidad queda a cargo del integrador y del CI.
