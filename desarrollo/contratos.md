# Contratos entre módulos — Sprint 2

Este archivo recoge lo que un módulo **produce** y otro **consume**. Las firmas se implementan **exactamente** como están escritas. Para cambiar una, se crea una decisión `Propuesta` en `desarrollo/decisiones/` y la aprueba el dueño (AGENT.md §6).

| # | Contrato | Dueño | Lo consumen | En `develop` a más tardar |
|---|---|---|---|---|
| C1 | Extensión de la base de datos (`ISemilla`, módulos, configuraciones EF) | Jorge | Todos | Fase 0 (día 1) |
| C2 | Núcleo JS `PitBossApi` y archivos `js/api/*.js` | Jorge | Todos | Fase 0 (día 1) |
| C3 | Utilidades de pruebas de integración | Jorge | Todos | Fase 0 (día 1) |
| C4 | Personal: `Empleado`, `Cargo`, `Turno`, `TipoContrato`, `IEmpleadoRepositorio` | Felipe | Iván | Tarea F1 (día 1–2) |
| C5 | Seguridad: `Rol`, `Modulos`, `MatrizAcceso`, `AutorizarModuloAttribute`, ayudante de login para pruebas | Iván | Todos | `Modulos` en I1; atributo y ayudante en I5 |
| C6 | Cuentas de demo | Iván | Todos (pruebas manuales e integración) | I4 |
| C7 | Valores JSON de los enums | Dueño de cada enum | Front (todos) | Con cada enum |
| C8 | Inventario: `Ficha.StockMinimo`, `EnNivelCritico`, `AlertaStock`, `IObservadorStockCritico` | Camilo | Camilo (Compras), Jorge (arc42) | Tarea C1 |

---

## C1 · Cómo extiende cada módulo la base de datos y el contenedor de DI (Jorge, Fase 0)

```csharp
// src/CasinoErp.Infraestructura/Semillas/ISemilla.cs
namespace CasinoErp.Infraestructura.Semillas;
public interface ISemilla
{
    int Orden { get; }                       // Sprint1 = 0, Personal = 10, Nomina = 20, Compras = 30, Seguridad = 40
    void Cargar(CasinoErpDbContext db);      // debe ser idempotente: si ya hay datos, no hace nada
}
```

- `CasinoErpDbContext.OnModelCreating` termina con `modelBuilder.ApplyConfigurationsFromAssembly(typeof(CasinoErpDbContext).Assembly);`. Cada entidad nueva se registra con una clase `IEntityTypeConfiguration<T>` en `src/CasinoErp.Infraestructura/Configuraciones/<Entidad>Configuracion.cs`. Los repositorios nuevos usan `db.Set<T>()`, sin agregar `DbSet` al contexto.
- Namespaces: `CasinoErp.Infraestructura.Semillas` (`ISemilla`, `SemillaSprint1` y las semillas de cada módulo) y `CasinoErp.Infraestructura.Modulos` (`ModuloX`).
- `ConfiguracionServicios.AgregarCasinoErp` ya llama, desde la Fase 0, a `services.AgregarPersonal()`, `AgregarNomina()`, `AgregarCompras()` y `AgregarSeguridad()`. Están definidos (vacíos al principio) en `src/CasinoErp.Infraestructura/Modulos/ModuloPersonal.cs`, `ModuloNomina.cs`, `ModuloCompras.cs` y `ModuloSeguridad.cs`. Cada dueño llena solo el suyo, y ahí mismo registra sus semillas: `services.AddScoped<ISemilla, SemillaPersonal>();`.
- `PrepararBaseDatos(this IServiceProvider proveedor, IConfiguration configuracion)`:
  1. si `BaseDatos:RecrearAlIniciar` es `true`, ejecuta `EnsureDeleted()`;
  2. ejecuta `EnsureCreated()`;
  3. si `BaseDatos:CargarDatosDemo` es `true`, ejecuta cada `ISemilla` registrada en orden ascendente de `Orden`.
  
  Todo corre dentro de `db.Database.CreateExecutionStrategy().Execute(...)`, como en la rama de Azure.
- `DatosIniciales` del Sprint 1 queda envuelto en `SemillaSprint1 : ISemilla` (`Orden = 0`).
- Los repositorios de cada módulo van en `src/CasinoErp.Infraestructura/Repositorios/<Modulo>Repositorios.cs`.

## C2 · Núcleo JS `PitBossApi` (Jorge, Fase 0)

En la Fase 0, `conexion-api.js` se divide en estos archivos, cargados en este orden al final de `index.html`:

```html
<script src="js/api/nucleo.js"></script>      <!-- Jorge: pedir, conectar, cargas, utilidades -->
<script src="js/api/sprint1.js"></script>     <!-- Jorge: perfiles, mesas, caja, retención (código existente) -->
<script src="js/api/inventario.js"></script>  <!-- Camilo: sincFichas, vaultOrder, restock (código existente) + alertas -->
<script src="js/api/seguridad.js"></script>   <!-- Iván (vacío al inicio) -->
<script src="js/api/personal.js"></script>    <!-- Felipe (vacío al inicio) -->
<script src="js/api/compras.js"></script>     <!-- Camilo (vacío al inicio) -->
<script src="js/api/arranque.js"></script>    <!-- Jorge: llama a PitBossApi.iniciar() -->
```

API pública (`window.PitBossApi`):

```js
PitBossApi.estado                 // { online: boolean }
PitBossApi.pedir(metodo, ruta, cuerpo?)        // Promise<datos>; ruta sin '/api' ("/empleados"); lanza Error(mensaje de la API)
                                               //   401 en una ruta distinta de '/auth/login' -> llama a PitBossApi.alNoAutorizado(r) y luego lanza
                                               //   403 sin mensaje -> lanza Error('No tienes acceso a este módulo.')
PitBossApi.pedirSi(modulos, metodo, ruta, cuerpo?) // modulos: 'caja' o ['caja','inventario'] (basta uno); si hay sesión (S.user) y el rol no tiene ninguno, devuelve Promise<null>; si no, igual que pedir
PitBossApi.conectar(tabla, nombre, { antes, exito, guardar })   // misma semántica que el conectar() actual
PitBossApi.registrarCarga(nombre, orden, obtener, aplicar)      // obtener: async () => datos (o null); aplicar: (datos) => void, modifica S
PitBossApi.sincronizar()          // ejecuta todos los obtener() en paralelo; luego los aplicar(datos) en orden ascendente (se omite si datos es null); luego render()
PitBossApi.iniciar()              // GET /api/salud -> online; luego, si await PitBossApi.antesDeSincronizar() es true -> sincronizar()
PitBossApi.antesDeSincronizar     // async () => true   (Iván lo reemplaza: exige sesión)
PitBossApi.encabezados            // () => ({})          (Iván lo reemplaza: Authorization: Bearer ...)
PitBossApi.alNoAutorizado         // (respuesta) => {}   (Iván lo reemplaza: vuelve al login si 401)
PitBossApi.util                   // { dos, isoLocal, fechaApi, hora, sumarDias, hoyReal, corrimiento, aFront, docApi, docFront, codigoFicha, MESES }
```

- **Orden de las cargas** (el orden de aplicación importa: transacciones y ofertas buscan jugadores en `S.players`; mesas busca crupieres en `S.staff`):

  | Orden | Carga | Archivo | Dueño |
  |---|---|---|---|
  | 10 | `jugadores` | sprint1.js | Jorge |
  | 15 | `empleados` | personal.js | Felipe |
  | 20 | `mesas` (incluye rellenos) | sprint1.js | Jorge |
  | 30 | `transacciones` | sprint1.js | Jorge |
  | 40 | `configuracionRetencion` | sprint1.js | Jorge |
  | 50 | `ofertas` | sprint1.js | Jorge |
  | 60 | `fichas` | inventario.js | Camilo |
  | 61 | `alertasStock` | inventario.js | Camilo |
  | 70 | `usuarios` | seguridad.js | Iván |
  | 80 | `nomina` | personal.js | Felipe |
  | 90 | `proveedores` y `ordenesCompra` | compras.js | Camilo |

- Cada archivo de módulo es una IIFE que usa `PitBossApi` y los globales del mockup (`S`, `V`, `MODALS`, `FORM`, `ACT`, `MODS`, `ORDER`, `ROLES`, `kpis`, `card`, `B`, `esc`, `money`, `grp`, `toast`, `render`, `allowed`, `cur`, `userById`, `staff`, `table`). **Nadie edita `index.html`.**
- Si `PitBossApi.estado.online` es `false`, cada módulo deja que el prototipo funcione con sus datos de ejemplo (modo demostración).

## C3 · Utilidades de pruebas de integración (Jorge, Fase 0)

```csharp
// tests/CasinoErp.PruebasIntegracion/Utilidades/CasinoErpFactory.cs  (se mueve desde ApiTests.cs)
public class CasinoErpFactory : WebApplicationFactory<Program>   // SQLite temporal, datos demo, RecrearAlIniciar=false

// tests/CasinoErp.PruebasIntegracion/Utilidades/ApiJson.cs
public static class ApiJson
{
    public static Task<JsonElement> LeerJsonAsync(this HttpResponseMessage respuesta);
}
```

Desde la tarea I5 (Iván) se agregan:

```csharp
// tests/CasinoErp.PruebasIntegracion/Utilidades/Autenticacion.cs
public static class Autenticacion
{
    public const string ContrasenaDemo = "Casino2026";
    public const string CodigoDemo = "482913";
    public static Task<HttpClient> ClienteComoAsync(this CasinoErpFactory factory, string correo);   // devuelve un cliente con Bearer
}
```

La factory agrega `Jwt:Clave` (40 caracteres de prueba) y `Seguridad:CodigoTotpDemo = 482913`. Cuando I5 llega a `develop`, toda prueba de integración nueva crea su cliente con `factory.ClienteComoAsync("<correo de un rol con acceso>")`.

## C4 · Personal (Felipe, tarea F1)

```csharp
namespace CasinoErp.Dominio.Personal;

public enum Cargo { Crupier, Cajero, JefeDePiso }
public enum Turno { Manana, Tarde, Noche }
public enum TipoContrato { Indefinido, TerminoFijo, PrestacionServicios }

public class Empleado
{
    public int Id { get; set; }
    public string Documento { get; set; } = "";      // solo dígitos, 6 a 10
    public string Nombre { get; set; } = "";
    public Cargo Cargo { get; set; }
    public string Juego { get; set; } = "";          // Blackjack|Ruleta|Póker|Baccarat|Craps para Crupier; "Caja"; "Supervisión"
    public Turno Turno { get; set; } = Turno.Tarde;
    public string Area { get; set; } = "Sin asignar";
    public TipoContrato TipoContrato { get; set; } = TipoContrato.Indefinido;
    public long SalarioBase { get; set; }
    public bool Activo { get; set; } = true;
    public string? MotivoRetiro { get; set; }
    public DateOnly FechaIngreso { get; set; }
}

public interface IEmpleadoRepositorio
{
    Task<bool> ExisteDocumentoAsync(string documento);
    Task<Empleado?> ObtenerPorDocumentoAsync(string documento);
    Task<IReadOnlyList<Empleado>> ListarAsync(bool soloActivos = false);
    Task AgregarAsync(Empleado empleado);
    Task GuardarCambiosAsync();
}
```

**Documentos de los empleados de demo** (los usa Iván para vincular usuarios): el empleado `i` del arreglo `staff` del mockup (con `i` desde 0) tiene `Documento = (1010200300 + i * 7919).ToString()`.

| Empleado | s-id | i | Documento |
|---|---|---|---|
| Ana Torres | s1 | 0 | 1010200300 |
| Laura Gómez | s3 | 2 | 1010216138 |
| Miguel Á. Soto | s4 | 3 | 1010224057 |
| Daniela Vargas | s17 | 16 | 1010327004 |
| Bruno Lara | s18 | 17 | 1010334923 |

## C5 · Seguridad (Iván)

```csharp
namespace CasinoErp.Dominio.Seguridad;

public enum Rol { Gerente, Administrador, Anfitrion, JefePiso, Crupier, Marketing, Cajero, TalentoHumano, Cumplimiento }

public static class Modulos
{
    public const string Inicio = "inicio", Perfiles = "perfiles", Promociones = "promociones", Retencion = "retencion",
        Catalogo = "catalogo", MiMesa = "mimesa", Mesas = "mesas", Caja = "caja", Inventario = "inventario",
        Competencias = "competencias", Rrhh = "rrhh", Cumplimiento = "cumplimiento", Reportes = "reportes",
        Usuarios = "usuarios", Proveedores = "proveedores";
}

public static class MatrizAcceso
{
    public static IReadOnlySet<string> ModulosDe(Rol rol);
    public static bool PuedeAcceder(Rol rol, string modulo);
    public static IReadOnlyList<Rol> RolesCon(params string[] modulos);
}

// src/CasinoErp.Api/Seguridad/AutorizarModuloAttribute.cs  (llega con I5)
public sealed class AutorizarModuloAttribute : AuthorizeAttribute
{
    public AutorizarModuloAttribute(params string[] modulos);   // Roles = string.Join(",", MatrizAcceso.RolesCon(modulos))
}
```

**Matriz de acceso.** Es la constante `ROLES` del mockup, sin `campana` (vive dentro de promociones), más `proveedores` para Gerente y JefePiso:

| Rol | Módulos |
|---|---|
| Gerente | inicio, perfiles, promociones, retencion, catalogo, mesas, caja, inventario, competencias, rrhh, cumplimiento, reportes, proveedores |
| Administrador | usuarios, reportes |
| Anfitrion | inicio, perfiles, retencion, catalogo |
| JefePiso | inicio, catalogo, mesas, inventario, competencias, proveedores |
| Crupier | mimesa |
| Marketing | inicio, perfiles, promociones, retencion, catalogo |
| Cajero | caja, inventario |
| TalentoHumano | inicio, competencias, rrhh |
| Cumplimiento | inicio, cumplimiento, reportes, competencias, caja |

**Módulos que exige cada controlador** (basta con uno). Cuando I5 llega a `develop`, Iván anota los controladores que existan en ese momento. Los que se creen después los anota su dueño.

| Controlador | `[AutorizarModulo(...)]` | Dueño |
|---|---|---|
| JugadoresController | `Modulos.Perfiles, Modulos.Retencion, Modulos.Caja` | Iván (Sprint 1) |
| MesasController | `Modulos.Catalogo, Modulos.Mesas, Modulos.MiMesa` | Iván (Sprint 1) |
| CajaController | `Modulos.Caja, Modulos.Cumplimiento` | Iván (Sprint 1) |
| RetencionController | `Modulos.Retencion, Modulos.Caja, Modulos.MiMesa` | Iván (Sprint 1) |
| RellenosController | `Modulos.Mesas, Modulos.Inventario` | Iván (Sprint 1) |
| InventarioController | `Modulos.Inventario, Modulos.Caja` | Iván (Sprint 1) |
| ResumenController | `Modulos.Inicio` | Iván (Sprint 1) |
| EmpleadosController | `Modulos.Competencias, Modulos.Rrhh, Modulos.Mesas` | Felipe |
| NominaController | `Modulos.Rrhh` | Felipe |
| ProveedoresController | `Modulos.Proveedores` | Camilo |
| ComprasController (órdenes de compra) | `Modulos.Proveedores, Modulos.Inventario` (el cajero ve y recibe órdenes, pero no administra proveedores) | Camilo |
| UsuariosController | `Modulos.Usuarios` | Iván |
| AuthController | `[AllowAnonymous]` en login; `[Authorize]` en `yo` y `cambiar-contrasena` | Iván |

`GET /api/salud` existe desde la Fase 0 (`Program.cs`, con `.AllowAnonymous()`), responde `{ "estado": "ok" }` y debe seguir siendo anónimo cuando se active la seguridad.

## C6 · Cuentas de demo (Iván, tarea I4)

Contraseña de todas: **`Casino2026`**. El código TOTP de demo es **`482913`** y solo se acepta cuando `Seguridad:CodigoTotpDemo` está configurado (Development y pruebas).

| Correo | Rol | Estado | Empleado vinculado |
|---|---|---|---|
| daniel.ruiz@pitboss.co | Gerente | Activo | — |
| ivan.leon@pitboss.co | Administrador | Activo | — |
| sebastian.prada@pitboss.co | Anfitrion | Activo | — |
| daniela.vargas@pitboss.co | JefePiso | Activo | 1010327004 |
| ana.torres@pitboss.co | Crupier | Activo | 1010200300 |
| jorge.diaz@pitboss.co | Marketing | Activo | — |
| laura.gomez@pitboss.co | Cajero | Activo | 1010216138 |
| felipe.cortes@pitboss.co | TalentoHumano | Activo | — |
| andrea.mendez@pitboss.co | Cumplimiento | Activo | — |
| miguel.soto@pitboss.co | Crupier | Activo | 1010224057 |
| carolina.rios@pitboss.co | Cajero | **Inactivo** | — |

## C7 · Valores JSON de los enums

La API usa `JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)`. Los valores que verá el front:

| Enum | Valores JSON |
|---|---|
| `Cargo` | `CRUPIER`, `CAJERO`, `JEFE_DE_PISO` |
| `Turno` | `MANANA`, `TARDE`, `NOCHE` |
| `TipoContrato` | `INDEFINIDO`, `TERMINO_FIJO`, `PRESTACION_SERVICIOS` |
| `EstadoNomina` | `BORRADOR`, `APROBADA` |
| `EstadoAlerta` | `ABIERTA`, `ATENDIDA` |
| `EstadoOrden` | `BORRADOR`, `ENVIADA`, `RECIBIDA`, `CANCELADA` |
| `Rol` | `GERENTE`, `ADMINISTRADOR`, `ANFITRION`, `JEFE_PISO`, `CRUPIER`, `MARKETING`, `CAJERO`, `TALENTO_HUMANO`, `CUMPLIMIENTO` |

Claves de rol en el front (`ROLES` del mockup): `GERENTE→gerente`, `ADMINISTRADOR→admin`, `ANFITRION→anfitrion`, `JEFE_PISO→jefepiso`, `CRUPIER→crupier`, `MARKETING→marketing`, `CAJERO→cajero`, `TALENTO_HUMANO→rrhh`, `CUMPLIMIENTO→cumplimiento`.

## C8 · Inventario (Camilo, tarea C1)

```csharp
namespace CasinoErp.Dominio.Inventario;

// En Ficha:
public int StockMinimo { get; set; } = 1000;           // el "umbral 1.000" del mockup
public bool EnNivelCritico => Stock < StockMinimo;

public enum EstadoAlerta { Abierta, Atendida }
public class AlertaStock
{
    public int Id { get; set; }
    public string CodigoFicha { get; set; } = "";
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public EstadoAlerta Estado { get; set; } = EstadoAlerta.Abierta;
    public DateTime CreadaEn { get; set; }
    public DateTime? AtendidaEn { get; set; }
}
public interface IObservadorStockCritico { Task StockCriticoAsync(AlertaStock alerta, Ficha ficha); }
```
