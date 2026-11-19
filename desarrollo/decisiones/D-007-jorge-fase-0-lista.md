# D-007 · Fase 0 en develop

- **Estado:** Aceptada · **Autor:** Jorge (agente de Jorge) · **Fecha:** 2026-11-18
- **Afecta a:** todos

## Aviso
La Fase 0 está en `develop` desde el commit `8fff1ce`. Desde ahora:

1. `git switch develop; git pull --rebase origin develop` y, si ya tienes ramas, `git rebase origin/develop` en cada una.
2. Para registrar tu módulo:
   - entidades: un `IEntityTypeConfiguration<T>` en `src/CasinoErp.Infraestructura/Configuraciones/`;
   - servicios: en tu archivo `src/CasinoErp.Infraestructura/Modulos/Modulo<Nombre>.cs`, que ya está llamado;
   - datos de demo: una clase `ISemilla` (namespace `CasinoErp.Infraestructura.Semillas`) registrada en tu módulo.
   
   No edites `CasinoErpDbContext.cs` ni `ConfiguracionServicios.cs`.
3. Front: tu archivo `src/CasinoErp.Api/wwwroot/js/api/<tuyo>.js` ya existe y se carga. Usa `window.PitBossApi` (contrato C2). No edites `index.html`.
4. Pruebas de integración: `new CasinoErpFactory()` y `respuesta.LeerJsonAsync()` en `tests/CasinoErp.PruebasIntegracion/Utilidades/`.
5. Si cambias el modelo, arranca con `$env:BaseDatos__RecrearAlIniciar="true"` para recrear la base local.
6. El CI exige cobertura del dominio ≥ 80 % y rechaza commits con marcas de IA (AGENT.md §4.1).
7. Los PR los fusiona el integrador (D-009): abre tu PR hacia `develop` y anótalo en tu bitácora.
