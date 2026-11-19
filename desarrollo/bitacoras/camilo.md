# Bitácora · Camilo · Plan-Camilo

> Solo la edita Camilo. Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| C0 | Arranque | Hecha | — |
| C1 | Umbral y alertas (dominio, C8) | Hecha (fusionada) | [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8) |
| C2 | Persistencia y API de alertas | Hecha (fusionada) | [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8) |
| C3 | Proveedores (dominio) | Hecha (fusionada) | [#16](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/16) |
| C4 | Órdenes de compra (dominio) | Hecha (fusionada) | [#16](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/16) |
| C5 | Persistencia y API de compras | Hecha (fusionada) | [#16](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/16) |
| C6 | Front inventario.js y compras.js | Hecha (fusionada) | [#17](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/17) |
| C7 | Autorización | Hecha (fusionada) | [#8](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/8), [#16](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/16) |
| C8 | arc42 bloque Inventario/Compras y cierre | Hecha (fusionada) | [#18](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/18) |

## Pendientes abiertos
- Mover en Jira HU-05 (CER-14), la historia de Proveedores (CER-15) y las subtareas CER-29 a CER-37. Lo hace Camilo a mano en Jira.
- Ensayar en persona la parte de la demo (pasos 1 a 3 de C6.2) y anotar el tiempo (C8.3).

## Bloqueos abiertos
- (ninguno)

## Entradas

## 2026-11-19 · Cierre del plan · Terminada
**Hecho:** el integrador fusionó los PR #16 (C3–C5 y C7), #17 (C6) y #18 (C8). Todas las tareas C0–C8 de Plan-Camilo están en `develop` (`6caa823`).
**Evidencia:** en `develop`, `dotnet test CasinoErp.sln` → 310 unitarias + 72 de integración aprobadas, 0 fallos.
**Decisiones:** ninguna nueva.
**Pendientes:** mover en Jira CER-14, CER-15 y CER-29 a CER-37 a "Listo" (el intento de iniciar sesión desde el navegador automatizado no se completó; lo hace Camilo a mano); ensayo en persona de la demo (C8.3).
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** Sprint Review.
**Commits/PR:** #16, #17, #18.

## 2026-11-18 23:55 · Retrospectiva HU-05 (C8.2)
**(a) Tiempo real contra lo estimado.** HU-05 (5 SP) y Proveedores (3 SP) se estimaron en 8 SP; el plan no traía horas. Trabajo registrado en esta bitácora: C0–C2 de 22:20 a 22:50 del 18-11 (≈ 30 min), integración con la seguridad de Iván ≈ 30 min y C3–C8 de 23:00 a 23:55 (≈ 1 h). Total ≈ 2 h, sin contar la revisión del integrador ni la espera del CI (≈ 1 min por push).
**(b) Qué salió bien.** El contrato C8 y la interfaz `IObservadorStockCritico` permitieron hacer la alerta (C1) sin tocar Compras, y enganchar el generador de órdenes en C4 sin cambiar el inventario. Con TDD, cada tarea de dominio pasó en verde al primer intento de implementación; la cobertura del dominio quedó en 97,64 %.
**(c) Qué mejorar.**
- Las pruebas de integración que cambian datos de la demo (retirar de `FICHA-500K`) necesitan su propia base desde el principio; aquí se resolvió con clases anidadas con su propia `CasinoErpFactory`.
- Al llegar la seguridad activa, las pruebas creadas con `CreateClient()` fallaron con 401: conviene escribirlas desde el inicio con `ClienteComoAsync` aunque la I5 todavía no esté (o con un cliente que se cambie en un solo lugar).
- El borrador de transacción del mockup trae fichas precargadas ($2.500.000 en otras denominaciones): en la demo hay que vaciarlas antes de sumar las 25 fichas de $500.000, o el total sale en $15.000.000 en vez de $12.500.000.
**(d) Impedimentos.** C7 esperó la seguridad de Iván (resuelto al fusionarse `cce0aa1`). El PR #8 quedó retenido una vuelta por las pruebas sin sesión. Sin acceso de las herramientas a Jira: las tarjetas las mueve Camilo a mano.

## 2026-11-18 23:55 · Tareas C3 a C8 · Terminadas (en revisión)
**Hecho:**
- **C3** (`d448724`): `Dominio/Compras/Proveedor.cs` y `ProveedorService.cs`; `ProveedorServiceTests` (17 pruebas) y `Dobles/ComprasEnMemoria.cs`.
- **C4** (`79a1e21`): `OrdenCompra.cs`, `GeneradorOrdenesCompra.cs` (observador) y `OrdenCompraService.cs`; `OrdenesCompraTests` (22 pruebas, incluido el flujo completo retiro → alerta → orden → recepción con el inventario real).
- **C5 + C7** (`6c23f12`): `ComprasConfiguracion.cs`, `ComprasRepositorios.cs`, `SemillaCompras.cs` (orden 30), registro en `ModuloCompras.cs`, `ProveedoresController` con `[AutorizarModulo(Modulos.Proveedores)]` y `ComprasController` con `[AutorizarModulo(Modulos.Proveedores, Modulos.Inventario)]`. `InventarioComprasApiTests` pasa a 15 pruebas (CRUD de proveedores, NIT inválido, orden inexistente 404, orden manual, OC-0001 desde la alerta, envío y recepción con stock 560, cajero 403 en proveedores, crupier 403, sin sesión 401).
- **C6** (`dce201a`): `inventario.js` (umbral por denominación, carga `alertasStock` 61 con toast, `alerts()`, `V.inventario` con el umbral de cada denominación y "Pedir al proveedor" como orden en borrador) y `compras.js` (módulo **Proveedores y compras** en el grupo Caja y bóveda para Gerente y Jefe de piso, vista con KPI, tablas de órdenes y proveedores, modales `supplier`, `orderSend` y `orderNew` y sus acciones).
- **C8**: `docs/arc42/05-bloques/inventario-compras.md` (frontera, endpoints, autorización, `classDiagram`, sección "Patrón observador y DIP", `sequenceDiagram` de la demo, reglas y pruebas).
**Evidencia:**
- `dotnet build` → 0 advertencias. `dotnet test CasinoErp.sln` → 310 unitarias + 72 de integración aprobadas, 0 fallos. Cobertura de líneas de `CasinoErp.Dominio` → 97,64 %.
- **C6.2 en el navegador** (API local con `BaseDatos__RecrearAlIniciar=true`, Edge con Playwright), 15 de 15 verificaciones: (1) Laura Gómez compra 25 fichas de $500.000 para María Fernanda López, total $12.500.000 → toast `Stock crítico: FICHA-500K quedó en 395 unidades (mínimo 400). Se creó un borrador de orden de compra.` y alerta "Bóveda · fichas de $500.000 … umbral 400" en Inventario; (2) la alerta aparece en el panel de inicio del gerente; (3) Daniel Ruiz ve `OC-0001 · FICHA-500K · 165 · Fichas Andinas S.A.S. · Alerta de stock · Borrador`, la envía y la recibe → toast `Orden recibida: el stock de la bóveda se actualizó.`, stock 560 y la alerta desaparece; (4) registra `902000111-3`, el duplicado muestra `Ya existe un proveedor con ese NIT.` y lo desactiva; (5) "Pedir al proveedor" crea una orden manual en borrador sin cambiar el stock; (6) Laura no ve "Proveedores y compras" y Ana Torres no ve Inventario.
**Decisiones (solo mi módulo):**
- `alerts()`: las alertas de la bóveda se insertan junto a las demás alertas "bad" (antes de las "warn") y no al final, porque el panel de inicio solo muestra las cuatro primeras; al final no se veían (paso 2 de C6.2).
- `ProveedorService.ConsultarAsync` también recorta el NIT (el plan lo pide al registrar), para que un NIT con espacios encuentre al proveedor.
- C6 va encadenada sobre C5 y C8 sobre C6 (como las ramas de Iván), porque C4 necesita el C1 del PR #8, que sigue abierto.
- C8.3: no hubo ensayo en persona. El recorrido automatizado de los seis pasos de C6.2 tardó 38 s; Camilo debe ensayar los pasos 1 a 3 (máximo 2 minutos).
**Pendientes:** Jira; ensayo en persona de la demo.
**Bloqueos / necesito de otros:** fusión en orden: PR #8 → proveedores → front → arc42 (integrador).
**Siguiente paso:** ninguna tarea del plan pendiente; atender comentarios de los PR.
**Commits/PR:** `d448724`, `79a1e21`, `6c23f12`, `dce201a` y el commit de C8; [#16](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/16), [#17](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/17), [#18](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/18).

## 2026-11-18 23:40 · PR #8 corregido y secretos de GitHub · Terminada
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
