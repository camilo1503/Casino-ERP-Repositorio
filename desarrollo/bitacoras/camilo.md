# Bitácora · Camilo · Plan-Camilo

> Solo la edita Camilo. Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| C0 | Arranque | Hecha | — |
| C1 | Umbral y alertas (dominio, C8) | En revisión | [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8) |
| C2 | Persistencia y API de alertas | En revisión | [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8) |
| C3 | Proveedores (dominio) | Pendiente | — |
| C4 | Órdenes de compra (dominio) | Pendiente | — |
| C5 | Persistencia y API de compras | Pendiente | — |
| C6 | Front inventario.js y compras.js | Pendiente | — |
| C7 | Autorización | En curso (alertas cubiertas en #8; faltan Proveedores y Compras, C3–C5) | [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8) |
| C8 | arc42 bloque Inventario/Compras y cierre | Pendiente | — |

## Pendientes abiertos
- Mover en Jira HU-05 (CER-14) y las subtareas C0, C1 y C2 (CER-29, 30, 31) a "En curso" / "Listo" (C0.3). Lo hace Camilo a mano en Jira.

## Bloqueos abiertos
- (ninguno) La seguridad de Iván (#4 a #10) ya está en `develop` desde `cce0aa1`; C7 queda desbloqueada.

## Entradas

## 2026-11-19 04:40 · PR #8 corregido y secretos de GitHub · Terminada
**Hecho:**
- Respuesta al comentario de Jorge en el PR #8: `git merge origin/develop` otra vez (entraron F7, I5.9 y las evidencias); las pruebas de `InventarioComprasApiTests` ya usaban `ClienteComoAsync` desde `56272e0`; se agrega `Un_crupier_no_ve_alertas_de_inventario` (C7.3: `ana.torres` recibe 403 en `GET /api/inventario/alertas` y en `PUT .../minimo`).
- El PR #8 no crea controladores nuevos, así que no hay `[AutorizarModulo]` que poner: `InventarioController` ya lo tiene desde la I5 de Iván. `ProveedoresController` y `ComprasController` lo llevarán al nacer en C5.
- No se usó `pull --rebase`: la rama ya estaba publicada y reescribirla exigiría `push --force`, que AGENT.md §4 prohíbe.
- Cargados los 5 secretos de GitHub Actions para el workflow `terraform-plan.yml` (F9.6 / J4.3), con los valores de la bitácora de Jorge: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RG` y `TFSTATE_SA`. Verificado con `gh secret list`.
**Evidencia:** `dotnet build` → 0 advertencias. `dotnet test CasinoErp.sln` → 271 unitarias + 62 de integración aprobadas, 0 fallos.
**Decisiones:** merge en vez de rebase (ver arriba).
**Pendientes:** Jira; fusión del PR #8 (integrador); J4.3, la prueba del workflow, la hace Jorge.
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** C3 (proveedores, dominio).
**Commits/PR:** merge de `develop` y la prueba de C7 en el PR #8.

## 2026-11-18 23:35 · Integración de develop en el PR #8 y documento de evidencias · Terminada
**Hecho:**
- `feature/HU-05-alertas-stock`: `git merge origin/develop` (con *merge* y no *rebase*, porque la rama ya estaba publicada). Entró la seguridad de Iván sin conflictos; `InventarioController` conserva el `[AutorizarModulo(Modulos.Inventario, Modulos.Caja)]` de Iván y mis dos endpoints.
- `InventarioComprasApiTests` (y la clase anidada `Demo`) crean el cliente con `ClienteComoAsync("laura.gomez@pitboss.co")`, como pide C2.1 cuando la I5 está en `develop`. Sin ese cambio, las 4 pruebas fallaban con 401.
- Documento de evidencias de reuniones (APA, portada del Sprint 1) con 12 capturas de GitHub y de las pruebas, en la rama `docs/sprint-2-evidencias-reuniones`: PR [#13](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/13) y decisión [D-013](../decisiones/D-013-camilo-evidencias-reuniones.md) (va en ese PR).
**Evidencia:** `dotnet build` → 0 advertencias. `dotnet test CasinoErp.sln` → 271 unitarias + 58 de integración aprobadas, 0 fallos. Cobertura de líneas de `CasinoErp.Dominio` → 97,36 %.
**Decisiones:** D-013, renumerada por Jorge desde D-012 (el documento vive en `docs/sprint-2/evidencias/`, carpeta de Jorge; Camilo lo pidió). Roles Scrum registrados en el documento: Jorge, Scrum Master; Camilo, Product Owner.
**Pendientes:** Jira; los 5 secretos de GitHub para J4.3; 5 capturas del documento que solo puede tomar el equipo (tablero y backlog de Jira, una reunión de Teams por sprint).
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** C3 (proveedores, dominio) en `feature/HU-05-proveedores`.
**Commits/PR:** merge de `develop` y ajuste de pruebas en el PR #8; PR #13.

## 2026-11-18 23:15 · Cierre de sesión · C1 + C2 en revisión
**Hecho:** PR [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8) hacia `develop` (C1 + C2) abierto con la plantilla.
**Evidencia:** CI del PR #8 en verde: "Backend .NET (build + pruebas)" y el paso de marcas, ambos `pass`. En local, `dotnet test CasinoErp.sln` → 221 + 38 aprobadas, 0 fallos.
**Decisiones:** ninguna nueva.
**Pendientes:** que el integrador fusione el PR #8; mover las tarjetas de Jira (Camilo).
**Bloqueos / necesito de otros:** C7 espera la I5 de Iván, que ya está en el PR #9 (abierto, sin fusionar).
**Siguiente paso:** C3 (proveedores, dominio) en `feature/HU-05-proveedores`, tomada de `develop` actualizado.
**Commits/PR:** `d1cf642`, `5a60ede`; PR #8.

## 2026-11-18 22:49 · Tarea C2 · Terminada (en revisión)
**Hecho:**
- `Infraestructura/Configuraciones/AlertaStockConfiguracion.cs`: tabla `AlertasStock`, `CodigoFicha` máx. 20 requerido, `Estado` como texto máx. 15, índice no único (`CodigoFicha`, `Estado`).
- `Infraestructura/Repositorios/InventarioRepositorios.cs`: `AlertaStockRepositorio` con `db.Set<AlertaStock>()`, lista ordenada por `CreadaEn` descendente.
- `Infraestructura/Semillas/SemillaUmbralesInventario.cs` (`Orden = 5`): baja el mínimo de `FICHA-500K` de 1000 a 400; idempotente.
- `Infraestructura/Modulos/ModuloCompras.cs`: registra `IAlertaStockRepositorio` y la semilla.
- `Api/Controllers/InventarioController.cs` (solo se agregan endpoints): `PUT /api/inventario/fichas/{codigo}/minimo` y `GET /api/inventario/alertas?soloAbiertas=true`.
- `tests/CasinoErp.PruebasIntegracion/InventarioComprasApiTests.cs`: 4 pruebas (umbral de demo 400, actualizar mínimo, mínimo negativo → 400, compra que baja la bóveda del umbral → alerta `ABIERTA`).
**Evidencia:** `dotnet build CasinoErp.sln` → 0 advertencias. `dotnet test CasinoErp.sln` → 221 unitarias + 38 de integración aprobadas, 0 fallos. Antes de implementar, `--filter InventarioComprasApiTests` → 4 fallos (C2.2).
**Decisiones:**
- La prueba de la demo va en la clase anidada `InventarioComprasApiTests.Demo`, con su propia `CasinoErpFactory`, porque retira de `FICHA-500K` y `El_umbral_de_demo_de_500K_es_400` asume stock 420 (nota del plan en C2.1). El filtro `--filter InventarioComprasApiTests` sigue encontrando las 4.
- Los clientes se crean con `CreateClient()` porque I5 todavía no está en `develop` (lo permite C2.1). Se cambian a `ClienteComoAsync` en C7.
**Pendientes:** Jira (ver Pendientes abiertos).
**Bloqueos / necesito de otros:** ninguno para C3–C5.
**Siguiente paso:** C3 (proveedores, dominio) en `feature/HU-05-proveedores`.
**Commits/PR:** `d1cf642` (C1), `5a60ede` (C2); PR de C1 + C2 hacia `develop` (enlace en la entrada siguiente o en el PR de la rama `feature/HU-05-alertas-stock`).

## 2026-11-18 22:30 · Tarea C1 · Terminada
**Hecho:**
- `Dominio/Inventario/Ficha.cs`: `StockMinimo` (por defecto 1000) y `EnNivelCritico` (contrato C8).
- `Dominio/Inventario/AlertaStock.cs`: `EstadoAlerta`, `AlertaStock`, `IAlertaStockRepositorio`, `IObservadorStockCritico`.
- `Dominio/Inventario/InventarioFichasService.cs`: constructor nuevo, `ActualizarStockMinimoAsync`, `ListarAlertasAsync`, `MensajeMinimo` y `EvaluarStockAsync` al final de `RegistrarAsync`, `RegistrarEntradaAsync`, `RetirarAsync` y `ActualizarStockMinimoAsync`.
- Pruebas: `AlertasStockTests.cs` (11 pruebas del plan), `Dobles/InventarioAlertasEnMemoria.cs` (`AlertasEnMemoria`, `ObservadorEspia`) y el constructor de `InventarioFichasServiceTests`.
**Evidencia:** `--filter AlertasStockTests` antes de implementar → error de compilación (C1.2). Después: `dotnet test tests/CasinoErp.PruebasUnitarias` → 221 aprobadas, 0 fallos. Cobertura de líneas de `CasinoErp.Dominio` → 97,52 % (umbral 80 %).
**Decisiones:**
- `IFichaRepositorio` no tiene un método para guardar la ficha sin registrar un movimiento. `ActualizarStockMinimoAsync` llama a `IAlertaStockRepositorio.GuardarCambiosAsync()` para persistir el mínimo, porque la ficha y las alertas comparten el mismo `DbContext` por solicitud. Así no se cambia la interfaz del Sprint 1. Solo afecta a mi módulo.
**Pendientes:** ninguno. Entre C1 y C2, la API no arrancaba (faltaba registrar `IAlertaStockRepositorio`), como prevé el plan; por eso C1 no lleva PR propio.
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** C2.
**Commits/PR:** `d1cf642`.

## 2026-11-18 22:20 · Tarea C0 · Terminada
**Hecho:** rama `feature/HU-05-alertas-stock` tomada de `develop` (`241b185`).
**Evidencia:** `dotnet test CasinoErp.sln` en `develop` → 210 unitarias + 34 de integración aprobadas, 0 fallos.
**Estado de `develop` al empezar:** Fase 0 (`8fff1ce`) y contrato C4 (`f083b68`) fusionados. Seguridad de Iván: I1–I4 en PR abiertos #4–#7, sin I5.
**Pendientes:** C0.3 (Jira) lo hace Camilo a mano.
**Siguiente paso:** C1.

## 2026-11-18 · Creación de la bitácora · Terminada
**Hecho:** se crea la bitácora con el plan del Sprint 2.
**Siguiente paso:** primera tarea del plan.


<!-- Copia este bloque para cada entrada nueva -->
## AAAA-MM-DD HH:MM · Tarea X.Y · En curso | Terminada | Bloqueada
**Hecho:**
**Evidencia:**
**Decisiones:**
**Pendientes:**
**Bloqueos / necesito de otros:**
**Siguiente paso:**
**Commits/PR:**
