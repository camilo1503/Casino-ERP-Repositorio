# Plan Camilo — Inventario: stock crítico, proveedores y órdenes de compra (HU-05)

> **Para agentes:** antes de empezar, lee `AGENT.md`, `desarrollo/contratos.md` y tu bitácora `desarrollo/bitacoras/camilo.md`. Ejecuta las tareas en orden, con TDD estricto, y reporta según AGENT.md §8. Los pasos usan casillas (`- [ ]`).

**Objetivo:** que el inventario de fichas de la bóveda tenga un umbral mínimo por denominación; que el sistema **alerte automáticamente** cuando una operación deja el stock por debajo del umbral; y que esa alerta **genere un borrador de orden de compra** a un proveedor registrado. Incluye el CRUD de Proveedores que pide el Sprint Goal.

**HU de la guía:**
- *HU-05 Alerta de Stock Crítico (5 SP).* "Como administrador de inventario, quiero que el sistema me alerte automáticamente cuando el nivel de existencias de un producto baje del umbral mínimo predefinido, para evitar quiebres de stock."
- Funcionalidad avanzada *"Generación automática de órdenes de compra: configurar reglas para crear automáticamente un borrador de orden de compra al recibir una alerta de stock crítico (HU-05)"* y CRUD de Proveedores (historia de 3 SP en Jira).

**Demo que pide la guía:** *"Se simula una compra que reduce el stock por debajo del umbral, y se muestra la notificación de alerta."*

**Mockup de referencia** (`src/CasinoErp.Api/wwwroot/index.html`):
- `V.inventario`: tarjetas "Stock de fichas por mesa", "Alertas de stock" (fila `Bóveda · fichas de $X · N und. · umbral 1.000 · Pedir al proveedor`) y "Nivel de fichas en bóveda por denominación", con la línea roja del umbral.
- `ACT.vaultOrder` ("Pedir al proveedor"; hoy suma 1.500 fichas de inmediato).
- `ACT.restock`: la fórmula de reposición es `Math.ceil(minimo * 1.4) - actual`.
- `function alerts()`: alimenta el panel de alertas del inicio.
- `DENOMS`: 5K, 10K, 25K, 50K, 100K y 500K.

**Arquitectura de tu módulo (patrón observador + DIP):**

```
InventarioFichasService ──(stock < mínimo)──▶ IAlertaStockRepositorio (crea/actualiza/atiende AlertaStock)
          │                                   └─▶ IEnumerable<IObservadorStockCritico>.StockCriticoAsync(alerta, ficha)
          │                                                     ▲
          │                                        GeneradorOrdenesCompra  (crea OrdenCompra en Borrador)
          ▲
OrdenCompraService.RecibirAsync ──▶ InventarioFichasService.RegistrarEntradaAsync (sube el stock y atiende la alerta)
```

El inventario **no conoce** las compras: solo avisa a la abstracción `IObservadorStockCritico` (DIP). `GeneradorOrdenesCompra` y `OrdenCompraService` son clases separadas a propósito. Si fueran una sola, el inventario dependería de las órdenes y las órdenes del inventario, y el contenedor de DI entraría en un ciclo.

**Archivos que son tuyos:**
- `src/CasinoErp.Dominio/Inventario/**` (los archivos existentes `Ficha.cs` e `InventarioFichasService.cs` pasan a ser tuyos) y `src/CasinoErp.Dominio/Compras/**`
- En `src/CasinoErp.Infraestructura/`: `Configuraciones/AlertaStockConfiguracion.cs`, `Configuraciones/ComprasConfiguracion.cs`, `Repositorios/InventarioRepositorios.cs` (solo `AlertaStockRepositorio`), `Repositorios/ComprasRepositorios.cs`, `Modulos/ModuloCompras.cs`, `Semillas/SemillaUmbralesInventario.cs`, `Semillas/SemillaCompras.cs`
- `src/CasinoErp.Api/Controllers/InventarioController.cs` (solo se agregan endpoints), `ProveedoresController.cs`, `ComprasController.cs`
- `src/CasinoErp.Api/wwwroot/js/api/inventario.js` y `compras.js`
- En `tests/CasinoErp.PruebasUnitarias/`: `InventarioFichasServiceTests.cs` (solo el constructor), `Dobles/InventarioAlertasEnMemoria.cs`, `Dobles/ComprasEnMemoria.cs`, `AlertasStockTests.cs`, `ProveedorServiceTests.cs`, `OrdenesCompraTests.cs`
- `tests/CasinoErp.PruebasIntegracion/InventarioComprasApiTests.cs`
- `docs/arc42/05-bloques/inventario-compras.md`

**Contratos:** eres dueño de **C8**. Consumes **C1**, **C2** y **C3** (Fase 0 de Jorge) y **C5** (seguridad de Iván, desde C7).

---

## Orden y dependencias

| Tarea | Necesita en `develop` | Nota |
|---|---|---|
| C0 Arranque | — | |
| C1 Umbral y alertas (dominio) | — | Se puede desarrollar el día 1, pero **se fusiona junto con C2** (el constructor cambia y la API necesitaría registrar el repositorio) |
| C2 Persistencia y API de alertas | **Fase 0 (J0)** | Un solo PR con C1 |
| C3 Proveedores (dominio) | — | Archivos nuevos; día 1 |
| C4 Órdenes de compra (dominio) | C1 | |
| C5 Persistencia y API de compras | C2, C3, C4 | |
| C6 Front (`inventario.js`, `compras.js`) | Fase 0 (C2 JS), C5 | |
| C7 Autorización | **I5 de Iván** | |
| C8 arc42 y cierre | C6 | |

---

### Tarea C0 · Arranque

- [ ] **C0.1** `git switch develop; git pull --rebase origin develop; git switch -c feature/HU-05-alertas-stock`
- [ ] **C0.2** Ejecutar `dotnet test CasinoErp.sln` y anotar el resultado en la bitácora.
- [ ] **C0.3** En Jira, mover HU-05 y la subtarea C1 a "En curso". Copiar la tabla de tareas C0–C8 en tu bitácora.

---

### Tarea C1 · Umbral mínimo y alerta de stock crítico (dominio, contrato C8)

**Archivos:**
- Modificar: `src/CasinoErp.Dominio/Inventario/Ficha.cs` (agregar `StockMinimo` y `EnNivelCritico` según C8)
- Crear: `src/CasinoErp.Dominio/Inventario/AlertaStock.cs` (`EstadoAlerta`, `AlertaStock`, `IAlertaStockRepositorio`, `IObservadorStockCritico`)
- Modificar: `src/CasinoErp.Dominio/Inventario/InventarioFichasService.cs`
- Modificar: `tests/CasinoErp.PruebasUnitarias/InventarioFichasServiceTests.cs` (solo el constructor: `new InventarioFichasService(_repo, new AlertasEnMemoria(), [], new RelojFijo())`)
- Crear: `tests/CasinoErp.PruebasUnitarias/Dobles/InventarioAlertasEnMemoria.cs` y `AlertasStockTests.cs`

**Interfaces que produce:**

```csharp
public interface IAlertaStockRepositorio
{
    Task<AlertaStock?> ObtenerAbiertaAsync(string codigoFicha);
    Task<IReadOnlyList<AlertaStock>> ListarAsync(bool soloAbiertas);
    Task AgregarAsync(AlertaStock alerta);
    Task GuardarCambiosAsync();
}

public class InventarioFichasService
{
    public const string MensajeMinimo = "Stock mínimo actualizado";
    public InventarioFichasService(IFichaRepositorio repo, IAlertaStockRepositorio alertas,
                                   IEnumerable<IObservadorStockCritico> observadores, TimeProvider reloj);
    // métodos existentes sin cambios de firma: RegistrarAsync, ConsultarAsync, ListarAsync, ListarMovimientosAsync, RegistrarEntradaAsync, RetirarAsync
    public Task<Ficha> ActualizarStockMinimoAsync(string codigo, int stockMinimo);
    public Task<IReadOnlyList<AlertaStock>> ListarAlertasAsync(bool soloAbiertas = true);
}
```

**Regla de evaluación** (método privado `EvaluarStockAsync(Ficha ficha)`). Se llama al final de `RegistrarAsync`, `RegistrarEntradaAsync`, `RetirarAsync` y `ActualizarStockMinimoAsync`, después de guardar la ficha:

```
abierta = await alertas.ObtenerAbiertaAsync(ficha.Codigo)
si ficha.EnNivelCritico:
    si abierta es null:
        nueva = AlertaStock { CodigoFicha, StockActual = ficha.Stock, StockMinimo = ficha.StockMinimo, CreadaEn = reloj }
        await alertas.AgregarAsync(nueva)
        para cada observador: await observador.StockCriticoAsync(nueva, ficha)
    si no:
        abierta.StockActual = ficha.Stock; abierta.StockMinimo = ficha.StockMinimo; await alertas.GuardarCambiosAsync()
si no, y abierta no es null:
    abierta.Estado = Atendida; abierta.AtendidaEn = reloj; abierta.StockActual = ficha.Stock; await alertas.GuardarCambiosAsync()
```

**Mensajes:**
- `ActualizarStockMinimoAsync` con un valor negativo → `El stock mínimo no puede ser negativo`, con el mismo estilo sin punto final que el resto del inventario del Sprint 1.
- Si la ficha no existe → `NoEncontradoException("Ficha no encontrada")` (ya existe en `ConsultarAsync`).

- [ ] **C1.1 Escribir las pruebas que fallan** (`AlertasStockTests`; `Existe(codigo, stock, minimo)` agrega una `Ficha` a `FichasEnMemoria`; `_espia = new ObservadorEspia()` guarda las alertas en `List<AlertaStock> Avisos`)

```csharp
[Fact] public async Task Un_retiro_que_deja_el_stock_bajo_el_minimo_crea_alerta_y_avisa()
{
    Existe("FICHA-500K", stock: 420, minimo: 400);
    var ficha = await _servicio.RetirarAsync("FICHA-500K", 25);
    ficha.Stock.Should().Be(395);
    ficha.EnNivelCritico.Should().BeTrue();
    var alerta = (await _servicio.ListarAlertasAsync()).Single();
    alerta.CodigoFicha.Should().Be("FICHA-500K");
    alerta.StockActual.Should().Be(395);
    alerta.StockMinimo.Should().Be(400);
    alerta.Estado.Should().Be(EstadoAlerta.Abierta);
    alerta.CreadaEn.Should().Be(new DateTime(2026, 10, 26, 15, 0, 0, DateTimeKind.Utc));
    _espia.Avisos.Should().ContainSingle();
}
[Fact] public async Task Retiros_repetidos_bajo_el_minimo_no_duplican_la_alerta_ni_el_aviso()   // 2 retiros -> 1 alerta abierta, StockActual = valor final, 1 aviso
[Fact] public async Task Una_entrada_que_supera_el_minimo_atiende_la_alerta()                    // Estado Atendida, AtendidaEn == reloj; ListarAlertasAsync() vacío; ListarAlertasAsync(false) tiene 1
[Fact] public async Task Una_entrada_que_no_alcanza_el_minimo_mantiene_la_alerta_abierta()       // StockActual actualizado
[Fact] public async Task Quedar_exactamente_en_el_minimo_no_es_critico()                          // stock 400, minimo 400 -> sin alerta
[Fact] public async Task Subir_el_minimo_por_encima_del_stock_dispara_la_alerta()                // ActualizarStockMinimoAsync("FICHA-5K", 5_000) con stock 4_200 -> 1 alerta, 1 aviso
[Fact] public async Task Bajar_el_minimo_por_debajo_del_stock_atiende_la_alerta()
[Fact] public async Task Rechaza_minimo_negativo()                                                // "El stock mínimo no puede ser negativo"
[Fact] public async Task Actualizar_minimo_de_ficha_inexistente_lanza_no_encontrado()
[Fact] public async Task Registrar_una_ficha_con_stock_inicial_bajo_el_minimo_crea_alerta()      // RegistrarAsync("FICHA-1M", 1_000_000, "Dorada", 10) -> alerta (mínimo por defecto 1000)
[Fact] public async Task Avisa_a_todos_los_observadores()                                         // dos espías -> cada uno con 1 aviso
```

- [ ] **C1.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter AlertasStockTests`. Esperado: error de compilación.
- [ ] **C1.3** Implementar. Ajustar el constructor en `InventarioFichasServiceTests`.
- [ ] **C1.4** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias`. Esperado: todas aprobadas, incluidas las del Sprint 1.
- [ ] **C1.5** Commit y push a la rama (**sin PR todavía**): `HU-05: Umbral minimo por denominacion y alerta de stock critico (dominio)`.

---

### Tarea C2 · Persistencia y API de alertas (requiere la Fase 0; mismo PR que C1)

**Archivos:**
- Crear: `src/CasinoErp.Infraestructura/Configuraciones/AlertaStockConfiguracion.cs`, `Repositorios/InventarioRepositorios.cs`, `Semillas/SemillaUmbralesInventario.cs`
- Modificar: `src/CasinoErp.Infraestructura/Modulos/ModuloCompras.cs`
- Modificar: `src/CasinoErp.Api/Controllers/InventarioController.cs`
- Pruebas: `tests/CasinoErp.PruebasIntegracion/InventarioComprasApiTests.cs`

**Decisiones de implementación:**
- `AlertaStockConfiguracion`: tabla `AlertasStock`; `CodigoFicha` máx. 20 y requerido; `Estado` como `string` (máx. 15); índice no único en (`CodigoFicha`, `Estado`).
- `Ficha.StockMinimo` no necesita configuración: EF lo mapea por convención y `EnNivelCritico`, al ser solo lectura, no se mapea. **No se toca `CasinoErpDbContext.cs`.**
- `AlertaStockRepositorio(CasinoErpDbContext db)` usa `db.Set<AlertaStock>()`. `ListarAsync` ordena por `CreadaEn` descendente.
- `ModuloCompras.AgregarCompras()` registra `AddScoped<IAlertaStockRepositorio, AlertaStockRepositorio>()` (con el comentario `// HU-05: alertas del inventario`) y `AddScoped<ISemilla, SemillaUmbralesInventario>()`. Como `InventarioFichasService` ya está registrado en el Sprint 1 y los observadores llegan por `IEnumerable`, si no hay ninguno se inyecta una lista vacía.
- `SemillaUmbralesInventario : ISemilla` con `Orden = 5`: si `FICHA-500K` existe y su `StockMinimo` sigue en 1000, lo baja a **400**. Así la demo arranca **por encima** del umbral (stock 420) y una compra de 25 fichas de $500.000 lo cruza (queda en 395). Los demás valores se quedan en 1000. Es idempotente.
- Endpoints nuevos en `InventarioController`:

| Verbo y ruta | Cuerpo | Respuesta |
|---|---|---|
| `PUT /api/inventario/fichas/{codigo}/minimo` | `StockMinimoRequest(int StockMinimo)` | `Respuesta<Ficha>("Stock mínimo actualizado", ficha)` |
| `GET /api/inventario/alertas?soloAbiertas=true` | — | `IReadOnlyList<AlertaStock>` |

  Las respuestas de `entradas` y `retiros` ya devuelven la `Ficha`, que ahora incluye `stockMinimo` y `enNivelCritico`; el front usa ese dato.

- [ ] **C2.1 Escribir las pruebas de integración que fallan** (si I5 ya está en `develop`, el cliente se crea con `ClienteComoAsync("laura.gomez@pitboss.co")`; si no, con `CreateClient()`)

```csharp
[Fact] public async Task La_compra_que_baja_la_boveda_del_umbral_genera_alerta()   // la demo de la Sprint Review
{
    var r = await _cliente.PostAsJsonAsync("/api/inventario/fichas/FICHA-500K/retiros", new { cantidad = 25 });
    var ficha = (await r.LeerJsonAsync()).GetProperty("datos");
    ficha.GetProperty("stock").GetInt32().Should().Be(395);
    ficha.GetProperty("enNivelCritico").GetBoolean().Should().BeTrue();
    var alertas = await _cliente.GetFromJsonAsync<JsonElement>("/api/inventario/alertas");
    alertas.EnumerateArray().Should().ContainSingle(a => a.GetProperty("codigoFicha").GetString() == "FICHA-500K"
                                                       && a.GetProperty("estado").GetString() == "ABIERTA");
}
[Fact] public async Task El_umbral_de_demo_de_500K_es_400()          // GET /api/inventario/fichas/FICHA-500K -> stockMinimo 400, enNivelCritico false
[Fact] public async Task Actualiza_el_minimo()                       // PUT .../FICHA-5K/minimo {stockMinimo: 1200} -> 200, mensaje "Stock mínimo actualizado"
[Fact] public async Task Minimo_negativo_responde_400()              // { mensaje: "El stock mínimo no puede ser negativo" }
```

Cada clase de pruebas tiene su propia base, pero las pruebas de una misma clase la comparten. Por eso la prueba que retira de `FICHA-500K` no debe correr junto con otra que asuma `stock 420`; usar otra ficha si hace falta.

- [ ] **C2.2** Ejecutar `dotnet test tests/CasinoErp.PruebasIntegracion --filter InventarioComprasApiTests`. Esperado: falla.
- [ ] **C2.3** Implementar.
- [ ] **C2.4** Ejecutar `dotnet test CasinoErp.sln` y el comando de cobertura. Esperado: verde y ≥ 80 %.
- [ ] **C2.5** PR (C1 + C2): `HU-05: Alerta automatica de stock critico con umbral por denominacion`. Anotarlo en la bitácora.

---

### Tarea C3 · Proveedores (dominio)

**Rama:** `feature/HU-05-proveedores`.

**Archivos:** `src/CasinoErp.Dominio/Compras/Proveedor.cs`, `ProveedorService.cs`; pruebas `Dobles/ComprasEnMemoria.cs` (`ProveedoresEnMemoria`) y `ProveedorServiceTests.cs`.

**Interfaces que produce:**

```csharp
namespace CasinoErp.Dominio.Compras;

public class Proveedor
{
    public int Id { get; set; }
    public string Nit { get; set; } = "";          // ^\d{9}-\d$  p. ej. "901234567-1"
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";       // en minúsculas
    public string Telefono { get; set; } = "";
    public bool Activo { get; set; } = true;
}
public interface IProveedorRepositorio
{
    Task<bool> ExisteNitAsync(string nit);
    Task<Proveedor?> ObtenerPorNitAsync(string nit);
    Task<IReadOnlyList<Proveedor>> ListarAsync(bool soloActivos = false);   // ordenado por Id
    Task AgregarAsync(Proveedor proveedor);
    Task GuardarCambiosAsync();
}
public class ProveedorService(IProveedorRepositorio repo)
{
    public const string MensajeRegistrado = "Proveedor registrado", MensajeActualizado = "Proveedor actualizado",
                        MensajeDesactivado = "Proveedor desactivado", MensajeReactivado = "Proveedor reactivado";
    public Task<Proveedor> RegistrarAsync(string nit, string nombre, string correo, string telefono);
    public Task<Proveedor> ActualizarAsync(string nit, string nombre, string correo, string telefono);
    public Task<Proveedor> DesactivarAsync(string nit);
    public Task<Proveedor> ReactivarAsync(string nit);
    public Task<Proveedor> ConsultarAsync(string nit);
    public Task<IReadOnlyList<Proveedor>> ListarAsync(bool soloActivos = false);
}
```

**Reglas** (los mensajes terminan en punto, como en el mockup):

| Regla | Mensaje |
|---|---|
| NIT que no cumple `^\d{9}-\d$` (después de `Trim()`) | `El NIT no es válido.` |
| Nombre vacío | `El nombre es obligatorio.` |
| Correo que no cumple `^[^@\s]+@[^@\s]+\.[^@\s]+$` | `El correo no es válido.` |
| Teléfono vacío | `El teléfono es obligatorio.` |
| NIT ya registrado | `Ya existe un proveedor con ese NIT.` |
| Desactivar uno inactivo | `El proveedor ya está inactivo.` |
| Reactivar uno activo | `El proveedor ya está activo.` |
| No existe | `NoEncontradoException("Proveedor no encontrado")` |

- [ ] **C3.1 Escribir las pruebas que fallan**

```csharp
[Fact] public async Task Registra_un_proveedor()                  // "901234567-1", "Fichas Andinas S.A.S.", "VENTAS@FICHASANDINAS.CO", "+57 601 555 0101" -> Activo, correo en minúsculas
[Theory, InlineData("90123456-1"), InlineData("901234567"), InlineData("901234567-12"), InlineData("ABC")]
public async Task Rechaza_nit_invalido(string nit)                // "El NIT no es válido."
[Fact] public async Task Rechaza_nit_duplicado()                  // "Ya existe un proveedor con ese NIT."
[Fact] public async Task Rechaza_nombre_vacio()
[Fact] public async Task Rechaza_correo_invalido()                // "ventas@" -> "El correo no es válido."
[Fact] public async Task Rechaza_telefono_vacio()
[Fact] public async Task Actualiza_nombre_correo_y_telefono()
[Fact] public async Task Desactiva_sin_borrar()                   // Activo false; sigue en ListarAsync(); no aparece en ListarAsync(true)
[Fact] public async Task No_desactiva_dos_veces()
[Fact] public async Task Reactiva()
[Fact] public async Task Consultar_inexistente_lanza_no_encontrado()
```

- [ ] **C3.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter ProveedorServiceTests`. Esperado: falla.
- [ ] **C3.3** Implementar.
- [ ] **C3.4** Ejecutar el mismo comando. Esperado: verde.
- [ ] **C3.5** Commit: `HU-05: Dominio de proveedores`.

---

### Tarea C4 · Órdenes de compra: generación automática y ciclo de vida (dominio)

**Archivos:** `src/CasinoErp.Dominio/Compras/OrdenCompra.cs`, `GeneradorOrdenesCompra.cs`, `OrdenCompraService.cs`; pruebas `Dobles/ComprasEnMemoria.cs` (`OrdenesEnMemoria`) y `OrdenesCompraTests.cs`.

**Interfaces que produce:**

```csharp
public enum EstadoOrden { Borrador, Enviada, Recibida, Cancelada }
public class OrdenCompra
{
    public const string OrigenAlerta = "Alerta de stock", OrigenManual = "Manual";
    public int Id { get; set; }
    public string? ProveedorNit { get; set; }
    public string CodigoFicha { get; set; } = "";
    public int Cantidad { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Borrador;
    public string Origen { get; set; } = OrigenManual;
    public DateTime CreadaEn { get; set; }
    public DateTime? ActualizadaEn { get; set; }
    public string Codigo => $"OC-{Id:D4}";
}
public interface IOrdenCompraRepositorio
{
    Task<OrdenCompra?> ObtenerAsync(int id);
    Task<IReadOnlyList<OrdenCompra>> ListarAsync();                 // más recientes primero
    Task<bool> ExisteAbiertaAsync(string codigoFicha);              // Borrador o Enviada
    Task AgregarAsync(OrdenCompra orden);
    Task GuardarCambiosAsync();
}
public class GeneradorOrdenesCompra(IOrdenCompraRepositorio ordenes, IProveedorRepositorio proveedores, TimeProvider reloj)
    : IObservadorStockCritico
{
    public Task StockCriticoAsync(AlertaStock alerta, Ficha ficha);
}
public class OrdenCompraService(IOrdenCompraRepositorio ordenes, IProveedorRepositorio proveedores,
                                InventarioFichasService inventario, TimeProvider reloj)
{
    public const string MensajeCreada = "Orden de compra creada", MensajeEnviada = "Orden enviada",
                        MensajeRecibida = "Orden recibida", MensajeCancelada = "Orden cancelada";
    public Task<OrdenCompra> CrearManualAsync(string codigoFicha, int cantidad, string? nit);
    public Task<OrdenCompra> EnviarAsync(int id, string? nit);
    public Task<OrdenCompra> RecibirAsync(int id);
    public Task<OrdenCompra> CancelarAsync(int id);
    public Task<IReadOnlyList<OrdenCompra>> ListarAsync();
}
```

**Reglas del generador:**
1. Si `await ordenes.ExisteAbiertaAsync(alerta.CodigoFicha)`, no hace nada.
2. `cantidad = Math.Max(1, (int)Math.Ceiling(alerta.StockMinimo * 1.4m) - alerta.StockActual)`.
3. `proveedor = (await proveedores.ListarAsync(soloActivos: true)).FirstOrDefault()`; si no hay ninguno, `ProveedorNit = null`.
4. Agrega la orden con `Origen = OrigenAlerta`, `Estado = Borrador` y `CreadaEn = reloj`.

**Reglas del servicio:**

| Situación | Resultado |
|---|---|
| `CrearManualAsync`: ficha inexistente | `NoEncontradoException("Ficha no encontrada")` (se valida con `inventario.ConsultarAsync`) |
| `CrearManualAsync`: cantidad ≤ 0 | `La cantidad debe ser mayor que cero` |
| `CrearManualAsync`: NIT inexistente | `NoEncontradoException("Proveedor no encontrado")` |
| `CrearManualAsync`: proveedor inactivo | `El proveedor está inactivo.` |
| `EnviarAsync`: si llega `nit`, se asigna (con las mismas dos validaciones); si al final no hay proveedor | `Asigna un proveedor antes de enviar la orden.` |
| `RecibirAsync` | Llama a `inventario.RegistrarEntradaAsync(CodigoFicha, Cantidad)`, lo que atiende la alerta |
| Transición no permitida | `La orden no está en un estado válido para esta acción.` |
| Orden inexistente | `NoEncontradoException("Orden de compra no encontrada")` |

Transiciones válidas: Borrador → Enviada (Enviar); Enviada → Recibida (Recibir); Borrador o Enviada → Cancelada (Cancelar). En cada cambio, `ActualizadaEn = reloj`.

- [ ] **C4.1 Escribir las pruebas que fallan** (para `RecibirAsync` se usa un `InventarioFichasService` real con `FichasEnMemoria`, `AlertasEnMemoria` y el generador como observador)

```csharp
[Fact] public async Task La_alerta_genera_un_borrador_para_el_primer_proveedor_activo()
{
    _proveedores.Datos.AddRange([P("900765432-8", activo: false), P("901234567-1")]);
    var alerta = new AlertaStock { CodigoFicha = "FICHA-500K", StockActual = 395, StockMinimo = 400 };
    await _generador.StockCriticoAsync(alerta, ficha500K);
    var o = (await _ordenes.ListarAsync()).Single();
    o.Cantidad.Should().Be(165);                 // ceil(400 * 1.4) - 395
    o.Estado.Should().Be(EstadoOrden.Borrador);
    o.Origen.Should().Be("Alerta de stock");
    o.ProveedorNit.Should().Be("901234567-1");
    o.CreadaEn.Should().Be(new DateTime(2026, 10, 26, 15, 0, 0, DateTimeKind.Utc));
}
[Fact] public async Task No_crea_otra_orden_si_ya_hay_una_abierta_para_la_ficha()
[Fact] public async Task Si_la_orden_anterior_se_recibio_una_nueva_alerta_crea_otra()
[Fact] public async Task Sin_proveedores_activos_el_borrador_queda_sin_proveedor()
[Fact] public async Task Flujo_completo_retiro_alerta_orden_recepcion()
{   // inventario real: retiro 25 de FICHA-500K (420, min 400) -> alerta + borrador; Enviar; Recibir -> stock 560; alerta Atendida
}
[Fact] public async Task Crea_orden_manual()                           // CrearManualAsync("FICHA-25K", 1500, "901234567-1") -> Origen "Manual"
[Fact] public async Task Orden_manual_valida_cantidad()                // 0 -> "La cantidad debe ser mayor que cero"
[Fact] public async Task Orden_manual_con_ficha_inexistente()          // NoEncontradoException "Ficha no encontrada"
[Fact] public async Task Enviar_exige_proveedor()                      // "Asigna un proveedor antes de enviar la orden."
[Fact] public async Task Enviar_asigna_el_proveedor_recibido()
[Fact] public async Task Enviar_rechaza_proveedor_inactivo()           // "El proveedor está inactivo."
[Fact] public async Task No_recibe_un_borrador()                       // "La orden no está en un estado válido para esta acción."
[Fact] public async Task No_cancela_una_recibida()
[Fact] public async Task Cancela_un_borrador()                         // Estado Cancelada, ActualizadaEn == reloj
[Fact] public async Task El_codigo_tiene_cuatro_digitos()              // Id 7 -> "OC-0007"
```

- [ ] **C4.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter OrdenesCompraTests`. Esperado: falla.
- [ ] **C4.3** Implementar.
- [ ] **C4.4** Ejecutar la suite unitaria y el comando de cobertura. Esperado: verde y ≥ 80 %.
- [ ] **C4.5** Commit: `HU-05: Generacion automatica y ciclo de vida de ordenes de compra`.

---

### Tarea C5 · Persistencia y API de proveedores y compras

**Archivos:**
- Crear: `src/CasinoErp.Infraestructura/Configuraciones/ComprasConfiguracion.cs` (`ProveedorConfiguracion` y `OrdenCompraConfiguracion`), `Repositorios/ComprasRepositorios.cs`, `Semillas/SemillaCompras.cs`
- Modificar: `Modulos/ModuloCompras.cs`
- Crear: `src/CasinoErp.Api/Controllers/ProveedoresController.cs`, `ComprasController.cs`
- Pruebas: `InventarioComprasApiTests.cs` (se amplía)

**Decisiones de implementación:**
- `Proveedor`: tabla `Proveedores`; índice único en `Nit` (máx. 12); `Nombre` máx. 120; `Correo` máx. 160; `Telefono` máx. 30.
- `OrdenCompra`: tabla `OrdenesCompra`; `Estado` como `string` (máx. 15); `Origen` máx. 30; `CodigoFicha` máx. 20; `ProveedorNit` máx. 12, opcional; `Ignore(o => o.Codigo)`.
- `ModuloCompras.AgregarCompras()`, además de lo de C2: los repositorios `IProveedorRepositorio` e `IOrdenCompraRepositorio`; `ProveedorService`; `OrdenCompraService`; `AddScoped<IObservadorStockCritico, GeneradorOrdenesCompra>()`; y `AddScoped<ISemilla, SemillaCompras>()`.
- `SemillaCompras : ISemilla` con `Orden = 30`. Si no hay proveedores, crea:
  - `901234567-1`, "Fichas Andinas S.A.S.", `ventas@fichasandinas.co`, `+57 601 555 0101`
  - `900765432-8`, "Suministros de Juego Ltda.", `pedidos@suministrosdejuego.co`, `+57 604 555 0202`
- **Serialización:** `OrdenCompra.Codigo` debe salir en el JSON (las propiedades de solo lectura se serializan). Verificarlo en la prueba.
- `ProveedoresController(ProveedorService s)`, ruta `api/proveedores`:

| Verbo y ruta | Cuerpo | Respuesta |
|---|---|---|
| `GET /api/proveedores?soloActivos=false` | — | lista |
| `GET /api/proveedores/{nit}` | — | `Proveedor` |
| `POST /api/proveedores` | `ProveedorRequest(string Nit, string Nombre, string Correo, string Telefono)` | 201 `Respuesta<Proveedor>("Proveedor registrado", p)` |
| `PUT /api/proveedores/{nit}` | `ProveedorRequest` (se ignora el `Nit` del cuerpo) | `Respuesta<Proveedor>("Proveedor actualizado", p)` |
| `POST /api/proveedores/{nit}/desactivar` | — | `"Proveedor desactivado"` |
| `POST /api/proveedores/{nit}/reactivar` | — | `"Proveedor reactivado"` |

- `ComprasController(OrdenCompraService s)`, ruta `api/compras/ordenes`:

| Verbo y ruta | Cuerpo | Respuesta |
|---|---|---|
| `GET /api/compras/ordenes` | — | lista |
| `POST /api/compras/ordenes` | `OrdenManualRequest(string CodigoFicha, int Cantidad, string? Nit)` | 201 `"Orden de compra creada"` |
| `POST /api/compras/ordenes/{id:int}/enviar` | `EnviarOrdenRequest(string? Nit)` | `"Orden enviada"` |
| `POST /api/compras/ordenes/{id:int}/recibir` | — | `"Orden recibida"` |
| `POST /api/compras/ordenes/{id:int}/cancelar` | — | `"Orden cancelada"` |

- [ ] **C5.1 Escribir las pruebas de integración que fallan**

```csharp
[Fact] public async Task La_alerta_de_la_compra_crea_una_orden_en_borrador()
{   // retiro 25 de FICHA-500K -> GET /api/compras/ordenes contiene { codigoFicha:"FICHA-500K", estado:"BORRADOR", origen:"Alerta de stock", cantidad:165, proveedorNit:"901234567-1", codigo:"OC-0001" }
}
[Fact] public async Task Enviar_y_recibir_la_orden_repone_la_boveda_y_atiende_la_alerta()
{   // enviar -> ENVIADA; recibir -> RECIBIDA; GET fichas/FICHA-500K stock 560; GET alertas (abiertas) sin FICHA-500K
}
[Fact] public async Task CRUD_de_proveedor()                       // POST 902000111-3 -> 201; PUT nombre; desactivar; GET ?soloActivos=true no lo incluye
[Fact] public async Task Nit_invalido_responde_400()               // { mensaje: "El NIT no es válido." }
[Fact] public async Task Orden_inexistente_responde_404()          // POST /api/compras/ordenes/999/enviar -> 404 { mensaje: "Orden de compra no encontrada" }
```

- [ ] **C5.2** Ejecutar `dotnet test tests/CasinoErp.PruebasIntegracion --filter InventarioComprasApiTests`. Esperado: falla.
- [ ] **C5.3** Implementar.
- [ ] **C5.4** Ejecutar `dotnet test CasinoErp.sln` y el comando de cobertura. Esperado: verde y ≥ 80 %.
- [ ] **C5.5** PR (C3 + C4 + C5): `HU-05: CRUD de proveedores y orden de compra automatica ante stock critico`.

---

### Tarea C6 · Front: alertas de stock y pantalla de Proveedores y compras

**Rama:** `feature/HU-05-front`. **Solo se editan `js/api/inventario.js` y `js/api/compras.js`** (contrato C2; no se toca `index.html`).

**`inventario.js`:**
1. Conservar lo que trae de la Fase 0 (carga `fichas` 60, `vaultOrder`, `restock`). Además, en la carga `fichas`, guardar los umbrales: `S.umbralBoveda = {}` y `S.umbralBoveda[f.denominacion] = f.stockMinimo`.
2. **Carga `alertasStock` (orden 61):** `obtener` → si `!S.user` o `['inventario','caja'].some(allowed)`, llama a `pedir('GET', '/inventario/alertas')`; si no, `null`. `aplicar(lista)`:
   - Calcula las alertas nuevas: códigos que no estaban en `S.alertasApi` antes. Solo si `S.alertasApi` ya existía, para no mostrar toasts en la primera carga.
   - Por cada alerta nueva: `toast('Stock crítico: ' + a.codigoFicha + ' quedó en ' + grp(a.stockActual) + ' unidades (mínimo ' + grp(a.stockMinimo) + '). Se creó un borrador de orden de compra.', true)`.
   - Luego `S.alertasApi = lista`.
3. **Reemplazar `alerts()`:** `const alertsLocal = alerts; alerts = function () { ... }`. Devuelve `alertsLocal()` más, si `allowed('inventario')`, una entrada por cada alerta de `S.alertasApi || []` con `{ k:'bad', t:'Stock de fichas crítico', d:'Bóveda · ' + money(den) + ' · ' + grp(a.stockActual) + ' und. (mínimo ' + grp(a.stockMinimo) + ')', go:'inventario' }`. `den` sale del código (`FICHA-500K` → 500000).
4. **Reemplazar `V.inventario`:** copiar la función del mockup a `inventario.js` y cambiar solo esto:
   - `low`: si `S.alertasApi` existe, se calcula como `DENOMS.filter(([v]) => S.alertasApi.some(a => a.codigoFicha === codigoFicha(v)))`; si no, la regla original `S.vault[v] < 1000`.
   - El texto `umbral 1.000` pasa a `umbral ${grp((S.umbralBoveda||{})[v] ?? 1000)}`.
   - El botón **Pedir al proveedor** de cada fila de bóveda sigue con `data-act="vaultOrder" data-arg="${v}"`.
   - El pie del gráfico pasa a `La línea roja es el umbral mínimo de cada denominación`.
5. **`ACT.vaultOrder`:** reemplazar el `conectar` de la Fase 0 para que, con la API en línea, **no** sume 1.500 de inmediato. Se guarda `const original = ACT.vaultOrder`. La versión nueva: si `PitBossApi.estado.online`, llama a `pedir('POST', '/compras/ordenes', { codigoFicha: codigoFicha(+v), cantidad: 1500, nit: null })`, luego `toast('Orden de compra creada en borrador. Revísala en Proveedores y compras.')` y `sincronizar()`, y muestra el mensaje de la API como toast de error si falla. Sin API, `original(v)`.

**`compras.js`:**
1. **Registrar el módulo en el menú:**
   - `MODS.proveedores = { l:'Proveedores y compras', g:3, tag:'HU-05', sub:'Proveedores de fichas y órdenes de compra automáticas' }`
   - `ORDER.splice(ORDER.indexOf('inventario') + 1, 0, 'proveedores')`
   - `ROLES.gerente.views.push('proveedores')` y `ROLES.jefepiso.views.push('proveedores')` (igual que en `MatrizAcceso`, contrato C5)
2. **Cargas (orden 90):** `proveedores` → si `allowed('proveedores')`, `GET /proveedores`; `ordenesCompra` → si `['proveedores','inventario'].some(allowed)`, `GET /compras/ordenes`. Guardar en `S.proveedores` y `S.ordenes`.
3. **`V.proveedores = () => ...`** con los helpers del mockup:
   - Si no hay API: `card('Proveedores y compras', '<p class="hint">Este módulo necesita conexión con la API.</p>')`.
   - `kpis([['Proveedores activos', n, 'de ' + total], ['Órdenes en borrador', nb, 'Por enviar', nb ? 'warn' : 'ok'], ['Órdenes enviadas', ne, 'Por recibir'], ['Recibidas este mes', nr, 'Bóveda repuesta', 'ok']])`.
   - **Tarjeta "Órdenes de compra"**: tabla `Código | Ficha | Cantidad | Proveedor | Origen | Estado | Acciones`, con el estado como badge (`Borrador` warn, `Enviada` gold, `Recibida` ok, `Cancelada`). Acciones según el estado: Borrador → `Enviar` (`data-act="modal" data-arg="orderSend" data-id="{id}"`) y `Cancelar` (`data-act="orderCancel" data-arg="{id}"`); Enviada → `Recibir` (`data-act="orderReceive" data-arg="{id}"`) y `Cancelar`. Botón de la tarjeta: `+ Orden manual` (`data-act="modal" data-arg="orderNew"`).
   - **Tarjeta "Proveedores"**: tabla `NIT | Nombre | Correo | Teléfono | Estado | Acciones` (`Editar` → `data-act="modal" data-arg="supplier" data-id="{nit}"`; `Desactivar`/`Reactivar` → `data-act="supplierOff"`/`"supplierOn"` con `data-arg="{nit}"`). Botón: `+ Registrar proveedor` (`data-act="modal" data-arg="supplier"`).
4. **Modales:** `MODALS.supplier` (campos `nit`, de solo lectura al editar; `name`; `email`; `phone`; `<form data-form="supplier">`), `MODALS.orderSend` (select de proveedores activos, `<form data-form="orderSend">` con un input oculto `id`) y `MODALS.orderNew` (select de denominación según `DENOMS`, cantidad y proveedor opcional; `<form data-form="orderNew">`). Si hay error, se muestra en `m.err` como en los modales del mockup.
5. **Acciones (funciones nuevas, sin `conectar`):** cada una llama a la API, luego `S.modal = null`, `toast(mensaje)` y `await PitBossApi.sincronizar()`. Si falla: `S.modal.err = error.message; render()` (o un toast de error si no hay modal).
   - `FORM.supplier` → `POST /proveedores` o `PUT /proveedores/{nit}`, con los toasts `Proveedor registrado.` / `Proveedor actualizado.`
   - `FORM.orderSend` → `POST /compras/ordenes/{id}/enviar {nit}`, toast `Orden enviada.`
   - `FORM.orderNew` → `POST /compras/ordenes`, toast `Orden de compra creada.`
   - `ACT.supplierOff` / `ACT.supplierOn` → `POST .../desactivar` y `.../reactivar`
   - `ACT.orderReceive` → `POST .../recibir`, toast `Orden recibida: el stock de la bóveda se actualizó.`
   - `ACT.orderCancel` → `POST .../cancelar`, toast `Orden cancelada.`

- [ ] **C6.1** Implementar `inventario.js` y `compras.js` como IIFE que usan `PitBossApi`.
- [ ] **C6.2 Probar a mano** (`$env:BaseDatos__RecrearAlIniciar="true"; dotnet run --project src/CasinoErp.Api`, http://localhost:5080). Es el **guion de la Sprint Review**; anotar cada resultado en la bitácora:
  1. Como **Laura Gómez** (cajera), en **Caja / POS**, una compra para María Fernanda López con 25 fichas de $500.000 ($12.500.000). Esperado: el toast `Stock crítico: FICHA-500K quedó en 395 unidades (mínimo 400)…` y, en **Inventario de fichas**, la fila de alerta de la bóveda de $500.000 con umbral 400. La transacción queda "En revisión" porque supera el umbral de cumplimiento de $5.000.000 (regla del Sprint 1). Es lo esperado y sirve para mostrar también Cumplimiento.
  2. En el panel de inicio de un rol con `inventario`, la alerta aparece en la lista.
  3. Como **Daniel Ruiz** (gerente), en **Proveedores y compras**, aparece la orden `OC-0001` de FICHA-500K por 165 unidades, en Borrador, para "Fichas Andinas S.A.S.". **Enviar** → Enviada; **Recibir** → toast; en Inventario la alerta desaparece y el stock de $500.000 queda en 560.
  4. Registrar el proveedor `902000111-3`; registrarlo de nuevo → `Ya existe un proveedor con ese NIT.`; desactivarlo.
  5. En Inventario, **Pedir al proveedor** en una fila crea una orden manual en Borrador (el stock **no** cambia todavía).
  6. **Laura Gómez** no ve el menú "Proveedores y compras". **Ana Torres** (crupier) no ve Inventario.
- [ ] **C6.3** Commit y PR: `HU-05: Front de alertas de stock y modulo Proveedores y compras`.

---

### Tarea C7 · Autorización (después de la I5 de Iván)

- [ ] **C7.1** `git pull --rebase origin develop` y luego rebase de tus ramas abiertas.
- [ ] **C7.2** Poner `[AutorizarModulo(Modulos.Proveedores)]` en `ProveedoresController` y `[AutorizarModulo(Modulos.Proveedores, Modulos.Inventario)]` en `ComprasController` (contrato C5). `InventarioController` ya lo anota Iván.
- [ ] **C7.3** En `InventarioComprasApiTests`, crear los clientes con `ClienteComoAsync` (`daniel.ruiz@pitboss.co` para compras y proveedores, `laura.gomez@pitboss.co` para el retiro desde caja) y agregar:

```csharp
[Fact] public async Task Un_cajero_ve_las_ordenes_pero_no_administra_proveedores()
{
    var cajero = await _factory.ClienteComoAsync("laura.gomez@pitboss.co");
    (await cajero.GetAsync("/api/compras/ordenes")).StatusCode.Should().Be(HttpStatusCode.OK);
    (await cajero.PostAsJsonAsync("/api/proveedores", new { nit = "903000111-1", nombre = "X", correo = "x@x.co", telefono = "1" }))
        .StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
[Fact] public async Task Un_crupier_no_ve_alertas_de_inventario()   // ana.torres: GET /api/inventario/alertas -> 403
```

- [ ] **C7.4** Ejecutar `dotnet test CasinoErp.sln` (verde). Commit: `HU-05: Autorizacion por modulo en proveedores y compras`.

---

### Tarea C8 · arc42 (bloque Inventario y Compras) y cierre

- [ ] **C8.1** `docs/arc42/05-bloques/inventario-compras.md`:
  - propósito y frontera;
  - un `classDiagram` Mermaid (`Ficha`, `AlertaStock`, `InventarioFichasService`, `IObservadorStockCritico`, `GeneradorOrdenesCompra`, `OrdenCompraService`, `Proveedor`, `OrdenCompra` y los repositorios);
  - la sección **"Patrón observador y DIP"**, que explica por qué el inventario no depende de compras y por qué el generador está separado del servicio (para evitar el ciclo de DI);
  - un `sequenceDiagram` del flujo de la demo: Cajero → Front (`sprint1.js`, `FORM.tx`) → `POST /api/caja/transacciones` → `POST /api/inventario/fichas/FICHA-500K/retiros` → `InventarioFichasService.RetirarAsync` → `EvaluarStockAsync` → `AlertaStockRepositorio.AgregarAsync` → `GeneradorOrdenesCompra.StockCriticoAsync` → `OrdenCompraRepositorio.AgregarAsync` → respuesta con `enNivelCritico = true` → Front, carga `alertasStock` y toast;
  - la tabla de reglas (umbral, fórmula de cantidad, transiciones de la orden).
- [ ] **C8.2** **Aporte a la retrospectiva:** entrada "Retrospectiva HU-05" en tu bitácora con horas reales contra lo estimado, qué salió bien, qué mejorar y los impedimentos.
- [ ] **C8.3** Ensayar tu parte de la demo (pasos 1 a 3 de C6.2, máximo 2 minutos) y anotar el tiempo.
- [ ] **C8.4** Commit y PR: `HU-08: arc42 bloque Inventario y Compras`. Mover HU-05 y la historia de Proveedores a "Listo" en Jira. Escribir la entrada final de la bitácora.
