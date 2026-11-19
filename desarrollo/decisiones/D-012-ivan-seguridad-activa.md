# D-012 · Seguridad activa: toda la API exige token desde `cce0aa1`

- **Estado:** Aceptada
- **Autor:** Iván (agente de Iván)
- **Fecha:** 2026-11-18
- **Afecta a:** todos (contratos C3, C5 y C6); en especial Felipe (F7) y Camilo (C5, C7 y el PR #8)
- **Plan / tarea:** Plan-Ivan · Tarea I5.9 (hito "Seguridad activa" de `desarrollo/README.md`)

## Contexto
La I5 de Iván activa la autenticación con JWT y la autorización por módulo en toda la API. El plan exige avisar con un día de anticipación (Iván avisó al grupo el 2026-11-18) y publicar esta decisión después del merge.

## Opciones consideradas
1. **Activar la seguridad en toda la API de una vez (elegida, es lo que pide el plan):** un solo punto de corte, fácil de comunicar; obliga a todos a migrar sus pruebas en el mismo momento.
2. **Activarla por controlador, poco a poco:** menos impacto inmediato, pero deja rutas abiertas sin control y complica la demo de la HU-06.

## Decisión
**Desde el commit `cce0aa1` de `develop`** (merge de `feature/HU-06-front`, que incluye los PR #4 a #10), **toda la API exige token**:
- Sin token, o con un token de un usuario desactivado o con otro rol: **401**.
- Con un token de un rol que no tiene el módulo: **403**.
- Siguen siendo anónimos: `GET /api/salud`, `POST /api/auth/login`, el front estático (`/`, `index.html`, `js/`) y Swagger.

## Qué debe hacer cada uno
1. **Actualizar:** `git switch develop; git pull --rebase origin develop`. En cada rama abierta: `git rebase origin/develop`.
2. **Recrear la base local una vez** (llega la tabla `Usuarios` con las 11 cuentas de demo, contrato C6): `$env:BaseDatos__RecrearAlIniciar="true"; dotnet run --project src/CasinoErp.Api`.
3. **Pruebas de integración nuevas:** crea el cliente con el ayudante de C3, nunca con `CreateClient()`:
   ```csharp
   public class MiApiTests : IClassFixture<CasinoErpFactory>, IAsyncLifetime
   {
       private readonly CasinoErpFactory _factory;
       private HttpClient _cliente = null!;
       public MiApiTests(CasinoErpFactory factory) => _factory = factory;
       public async Task InitializeAsync() => _cliente = await _factory.ClienteComoAsync("daniel.ruiz@pitboss.co");
       public Task DisposeAsync() => Task.CompletedTask;
   }
   ```
   No inicies sesión con `.GetAwaiter().GetResult()` en el constructor: se bloquea con el contexto de xUnit. Usa `IAsyncLifetime` como en el ejemplo. Elige una cuenta cuyo rol tenga el módulo (tabla de C5 y C6). Para factories derivadas con `WithWebHostBuilder` existe `ClienteComoAsync(factoryDerivada, correo)`.
4. **Controladores nuevos:** anótalos con `[AutorizarModulo(...)]` según la tabla de C5 (`using CasinoErp.Api.Seguridad;` y `using CasinoErp.Dominio.Seguridad;`). Comprobación: `grep -L "AutorizarModulo\|AllowAnonymous" src/CasinoErp.Api/Controllers/*.cs` no debe listar nada.
5. **Front:** `PitBossApi.pedir` ya manda el token y vuelve al login ante un 401. Usa `pedirSi(modulos, …)` para no pedir datos de módulos que el rol no tiene.
6. **Clave de desarrollo:** ya está en `appsettings.Development.json` (`Jwt:Clave`), junto con `Seguridad:CodigoTotpDemo = 482913`. En Azure, Terraform genera `Jwt__Clave`. No subas otra clave al repo.

## Consecuencias por integrante
- **Felipe (F7):** `EmpleadosController` y `NominaController` **ya quedaron anotados** en la I5, según C5, y `PersonalNominaApiTests` y `NominaApiTests` ya inician sesión como `felipe.cortes@pitboss.co`. A la F7 solo le queda revisarlo y anotar cualquier controlador nuevo.
- **Camilo (PR #8, `feature/HU-05-alertas-stock`):** al hacer `rebase` sobre `develop`, las 4 pruebas de `InventarioComprasApiTests` (y su clase anidada `Demo`) fallan con 401 porque usan `factory.CreateClient()`. Cámbialas por `ClienteComoAsync("daniel.ruiz@pitboss.co")` con el patrón del punto 3 (lo pide Plan-Ivan I5.5). `InventarioController` ya tiene `[AutorizarModulo(Modulos.Inventario, Modulos.Caja)]` y Git combina tus cambios sin conflicto (se probó en local). `ProveedoresController` y `ComprasController` deben llevar los atributos de C5 cuando existan.
- **Jorge:** la demo de la HU-06 y el guion del video pueden usar las cuentas de C6 (contraseña `Casino2026`, código `482913`). Para la Review, `Seguridad__CodigoTotpDemo` debe estar configurado en Azure (`var.codigo_totp_demo`).
- Documentación: `docs/arc42/10-conceptos-seguridad.md`, `docs/arc42/05-bloques/seguridad.md` y ADR-002 (`2932d93`).

## Aprobación
- Dueño del contrato C5/C6: Iván · 2026-11-18 · Aprobada (aviso al grupo hecho antes del merge)
