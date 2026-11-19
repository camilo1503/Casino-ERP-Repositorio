# D-008 · Ajustes a los contratos C1, C2 y C5 durante la Fase 0

- **Estado:** Aceptada · **Autor:** Jorge (agente de Jorge) · **Fecha:** 2026-11-18
- **Afecta a:** Iván (C5, `/api/salud`), Felipe y Camilo (C2, `pedirSi`), todos (C1, namespaces)
- **Plan / tarea:** Plan-Jorge · J0

## Contexto
Al implementar la Fase 0 aparecieron detalles que el plan no resolvía o que conviene adelantar.

## Decisión
1. **`GET /api/salud` se agrega en la Fase 0**, no en la I5 de Iván. Está en `Program.cs` con `.AllowAnonymous()`. Así el núcleo JS tiene una prueba de vida limpia desde el principio. Iván solo verifica que siga anónimo.
2. **`pedirSi` acepta un módulo o una lista** (`pedirSi(['caja','inventario'], ...)`, basta uno). Con un solo módulo, un rol como el cajero dejaba de cargar los jugadores, que hoy sí carga.
3. **`PitBossApi.util`** expone además `dos`, `sumarDias`, `hoyReal` y `corrimiento`.
4. **Namespaces:** `CasinoErp.Infraestructura.Semillas` para `ISemilla` y las semillas; `CasinoErp.Infraestructura.Modulos` para los `ModuloX`.
5. `PrepararBaseDatos` lee `BaseDatos:RecrearAlIniciar` y `BaseDatos:CargarDatosDemo` sin `GetValue`, para no agregar el paquete `Microsoft.Extensions.Configuration.Binder` a Infraestructura.
6. **CI:** nuevo job `sin-marcas-ia` (AGENT.md §4.1).

## Consecuencias
`desarrollo/contratos.md`, `Plan-Ivan.md` (endpoint de salud) y `Plan-Jorge.md` (J0.7) ya reflejan estos cambios.
