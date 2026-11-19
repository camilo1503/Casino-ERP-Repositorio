# Plan Jorge — Fase 0, Infraestructura como Código (HU-07), arc42 (HU-08), Scrum e integración

> **Para agentes:** antes de empezar, lee `AGENT.md`, `desarrollo/contratos.md` y tu bitácora `desarrollo/bitacoras/jorge.md`. Ejecuta las tareas en orden, con TDD donde haya código, y reporta según AGENT.md §8. Los pasos usan casillas (`- [ ]`).

**Objetivo:** (1) dejar el repositorio listo para que cuatro personas trabajen en paralelo sin conflictos (Fase 0, **bloqueante: es lo primero**); (2) codificar en Terraform la infraestructura de Azure que ya existe y correr `terraform plan` en cada PR a `develop` (HU-07, 13 SP); (3) consolidar arc42 y los artefactos Scrum (HU-08, 3 SP); (4) integrar, desplegar y cerrar el sprint.

**Rol en el equipo:** coordinador de integración. Jorge revisa los PR de los demás, mantiene `develop` en verde y es el único que edita `desarrollo/README.md` (el tablero) y que hace el PR de `develop` a `main`.

**Archivos que son tuyos:**
- `.github/` · `infra/`
- `docs/arc42/README.md`, `docs/arc42/05-vista-bloques.md` (solo el nivel 1), `07-vista-despliegue.md`, `09-decisiones/ADR-001…ADR-004`
- `docs/sprint-2/`
- `wwwroot/js/api/nucleo.js`, `sprint1.js`, `arranque.js`
- `desarrollo/README.md`, `desarrollo/decisiones/README.md`, `README.md`
- En la Fase 0, además: `CasinoErpDbContext.cs`, `ConfiguracionServicios.cs`, `Program.cs`, `index.html` (solo las etiquetas `<script>` y el texto "Sprint 1"), `ApiTests.cs`, los `.csproj` de pruebas y `DatosIniciales.cs`

**Jira:** HU-07 (13 SP) y HU-08 (3 SP), más la tarea existente **CER-8** "Remove leftover Python code from the repository", que se resuelve en J0.

---

## Orden y dependencias

| Tarea | Depende de | Desbloquea |
|---|---|---|
| J0 Fase 0 | — | Infraestructura, API y front de Felipe, Iván y Camilo |
| J1 Jira del Sprint 2 | — | Seguimiento del equipo |
| J2 Backend remoto del state | Acceso a la suscripción de Azure | J3 |
| J3 Terraform con import | J2 | J4 y J6 |
| J4 Workflow `terraform plan` | J3 | Criterio "IaC en PR" de la guía |
| J5 Integración continua (rol permanente) | J0 | — |
| J6 Despliegue del Sprint 2 en Azure | J3 y los PR de los demás en `develop` | J9 |
| J7 arc42 (consolidación) | Las secciones de cada integrante | J8 (video) |
| J8 Artefactos Scrum | J7 | Retrospectiva |
| J9 Cierre del sprint | Todo | — |

---

### Tarea J0 · Fase 0: preparar el repo para trabajo en paralelo (PRIORIDAD MÁXIMA)

**Rama:** `chore/fase-0`, tomada de `develop`. La rama `develop` ya existe: la creó el commit que subió esta carpeta.

**Archivos:**
- Fusionar `origin/feature/CER-6-despliegue-azure` en la rama.
- Modificar: `.github/workflows/ci.yml`, `tests/CasinoErp.PruebasUnitarias/CasinoErp.PruebasUnitarias.csproj`
- Crear:
  - `src/CasinoErp.Infraestructura/Semillas/ISemilla.cs` y `Semillas/SemillaSprint1.cs`
  - `src/CasinoErp.Infraestructura/Modulos/ModuloPersonal.cs`, `ModuloNomina.cs`, `ModuloCompras.cs`, `ModuloSeguridad.cs`
  - las carpetas `src/CasinoErp.Infraestructura/Configuraciones/` y `src/CasinoErp.Infraestructura/Repositorios/` (con un `.gitkeep`)
- Modificar: `CasinoErpDbContext.cs`, `ConfiguracionServicios.cs`, `Program.cs`, `appsettings.json`, `appsettings.Development.json`
- Mover y crear: `tests/CasinoErp.PruebasIntegracion/Utilidades/CasinoErpFactory.cs` (desde `ApiTests.cs`) y `Utilidades/ApiJson.cs`
- Crear: `src/CasinoErp.Api/wwwroot/js/api/nucleo.js`, `sprint1.js`, `inventario.js`, `seguridad.js`, `personal.js`, `compras.js`, `arranque.js`
- Eliminar: `src/CasinoErp.Api/wwwroot/js/conexion-api.js` y `registro.py` (CER-8)
- Modificar: `src/CasinoErp.Api/wwwroot/index.html` (solo las etiquetas `<script>` del final y el texto `Prototipo funcional · Sprint 1` → `Prototipo funcional · Sprint 2`) y `docs/index.md`

**Interfaces:**
- Produce: los contratos **C1, C2 y C3** de `desarrollo/contratos.md`, exactamente como están escritos.

- [ ] **J0.1 Partir de `develop` y fusionar el despliegue de Azure**

```powershell
git switch develop; git pull --rebase origin develop; git switch -c chore/fase-0
git merge --no-ff origin/feature/CER-6-despliegue-azure -m "CER-6: Integrar la preparacion para Azure SQL en develop"
dotnet test CasinoErp.sln
```

Esperado: 132 + 15 pruebas aprobadas. Si hay conflicto en `README.md`, se conservan las dos secciones (la del incremento y la de "Despliegue en Azure").

- [ ] **J0.2 Ajustar el CI**

En `ci.yml`:
- `push.branches: [main, develop, 'feature/**', 'chore/**', 'docs/**']` y `pull_request.branches: [main, develop]`.
- Agregar este paso después de "Pruebas unitarias e integración con cobertura":

```yaml
      - name: Cobertura mínima del dominio (DoD ≥ 80 %)
        run: >
          dotnet test tests/CasinoErp.PruebasUnitarias -c Release --no-build
          /p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line
          /p:Include="[CasinoErp.Dominio]*"
```

En `CasinoErp.PruebasUnitarias.csproj`, agregar `<PackageReference Include="coverlet.msbuild" Version="6.0.2" />`.

Agregar también el job `sin-marcas-ia` (AGENT.md §4.1). Hace checkout con `fetch-depth: 0` y revisa los mensajes de los commits nuevos del push (`github.event.before..github.sha`) o del PR (`base.sha..head.sha`). Falla si alguno coincide (sin distinguir mayúsculas) con `co-authored-by:.*(claude|anthropic|openai|copilot|gpt)|generated (with|by) .*\b(claude|ai)\b|noreply@anthropic\.com`. Si el push crea una rama nueva (`before` en ceros), revisa los commits que no están en `origin/main`.

- [ ] **J0.3 Escribir la prueba que falla de `RecrearAlIniciar`** (en `tests/CasinoErp.PruebasIntegracion/BaseDatosTests.cs`)

```csharp
[Fact] public async Task RecrearAlIniciar_vuelve_a_crear_la_base_con_los_datos_demo()
{
    using var f = new CasinoErpFactory().WithWebHostBuilder(b => b.UseSetting("BaseDatos:RecrearAlIniciar", "true"));
    var jugadores = await f.CreateClient().GetFromJsonAsync<JsonElement>("/api/jugadores");
    jugadores.GetArrayLength().Should().BeGreaterOrEqualTo(4);
}
```

Esta prueba usa la API sin autenticación. Cuando llegue I5, Iván la migra a `ClienteComoAsync`.

- [ ] **J0.4 Implementar el contrato C1**

- `ISemilla` (firma exacta de C1). `SemillaSprint1 : ISemilla` con `Orden = 0`, que delega en `DatosIniciales.Cargar(db)`.
- Cada `ModuloX.cs` es `public static class ModuloX { public static IServiceCollection AgregarX(this IServiceCollection s) => s; }`.
- En `AgregarCasinoErp`, después de los registros actuales: `services.AddScoped<ISemilla, SemillaSprint1>(); services.AgregarPersonal().AgregarNomina().AgregarCompras().AgregarSeguridad();`.
- `PrepararBaseDatos(this IServiceProvider proveedor, IConfiguration configuracion)`: dentro de la estrategia de ejecución, si `configuracion.GetValue("BaseDatos:RecrearAlIniciar", false)` es `true`, llama a `EnsureDeleted()`; luego `EnsureCreated()`; luego, si `CargarDatosDemo` es `true`, ejecuta `foreach (var s in scope.ServiceProvider.GetServices<ISemilla>().OrderBy(x => x.Orden)) s.Cargar(db);`.
- En `Program.cs`: `app.Services.PrepararBaseDatos(builder.Configuration);`.
- En `OnModelCreating`, como última línea: `modelBuilder.ApplyConfigurationsFromAssembly(typeof(CasinoErpDbContext).Assembly);`.
- En `appsettings.json`: `"BaseDatos": { "Proveedor": "Sqlite", "CargarDatosDemo": true, "RecrearAlIniciar": false }`.

- [ ] **J0.5 Implementar el contrato C3**

Mover `CasinoErpFactory` a `Utilidades/CasinoErpFactory.cs` (mismo namespace) y agregarle `builder.UseSetting("BaseDatos:RecrearAlIniciar", "false");`. Crear `ApiJson.LeerJsonAsync(this HttpResponseMessage r)`, que devuelve `r.Content.ReadFromJsonAsync<JsonElement>()`. En `ApiTests.cs`, el helper privado `Json(...)` pasa a usar `LeerJsonAsync`.

- [ ] **J0.6 Verificar la parte .NET**

Ejecutar: `dotnet test CasinoErp.sln` y luego el comando de cobertura (AGENT.md §10).
Esperado: 148 pruebas aprobadas (147 + la nueva) y coverlet sin `below the specified threshold`.

- [ ] **J0.7 Dividir `conexion-api.js` según el contrato C2**

Se mueve el código existente **sin cambiar su comportamiento**:

| Archivo nuevo | Qué lleva de `conexion-api.js` |
|---|---|
| `nucleo.js` | `BASE`, `pedir()` (ahora mezcla `PitBossApi.encabezados()` en los headers; si la respuesta es 401 y la ruta no es `/auth/login`, llama a `PitBossApi.alNoAutorizado(r)` antes de lanzar el error; si es 403 sin `mensaje`, lanza `Error('No tienes acceso a este módulo.')`), las utilidades de fechas y documentos (`dos`, `isoLocal`, `fechaApi`, `hora`, `sumarDias`, `corrimiento`, `aFront`, `docApi`, `docFront`, `codigoFicha`, `MESES`, expuestas en `PitBossApi.util`), `conectar()` y la nueva infraestructura de cargas (`registrarCarga`, `sincronizar`, `iniciar`, `pedirSi`). La API es la del contrato C2. |
| `sprint1.js` | `EST_JUG`, `EST_TX`, `TIPO_OF`, `TIPO_API`, `docDe`, `sincJugadores`, `sincMesas`, `sincTransacciones`, `sincConfiguracion`, `sincOfertas` y todas las llamadas `conectar(...)` de Perfiles, Catálogo, Operación de mesas, Caja (`tx`, `txApprove`, `txVoid`, `txReport`, `txEscalate`) y Retención. Registra las cargas de orden 10, 20, 30, 40 y 50 de la tabla de C2. |
| `inventario.js` (pasa a Camilo) | `sincFichas` (carga 60, `GET /inventario/fichas`) y los `conectar` de `vaultOrder` y `restock`, sin cambios. |
| `seguridad.js`, `personal.js`, `compras.js` | Una IIFE vacía con un comentario que dice de quién es el archivo y que apunta a su plan. |
| `arranque.js` | `PitBossApi.iniciar().catch(() => toast('Sin conexión con la API: el ERP funciona en modo demostración y los cambios no se guardan.', true));` |

- `iniciar()`: `GET /api/salud` (se agrega en la Fase 0 en `Program.cs` con `.AllowAnonymous()`) → `estado.online = true` → `if (await PitBossApi.antesDeSincronizar()) await sincronizar()`; si la sincronización falla, `online` vuelve a `false`. Se mantiene `window.PitBossApi.sincronizar` para depuración.
- `sprint1.js` lleva su propia marca de "primera carga" para `sincOfertas`: antes, la primera sincronización corría con `online = false` y las señales bloqueadas del prototipo quedaban al final del registro.
- Cada carga usa `pedirSi([...módulos], ...)` con los mismos módulos que `[AutorizarModulo]` (contrato C5), para no pedir lo que el rol no puede ver cuando llegue la seguridad.
- En `index.html`, reemplazar `<script src="js/conexion-api.js"></script>` por las 7 etiquetas de C2 en ese orden.

- [ ] **J0.8 Probar el front a mano** (regresión del Sprint 1)

Ejecutar: `$env:BaseDatos__RecrearAlIniciar="true"; dotnet run --project src/CasinoErp.Api`, abrir http://localhost:5080 y entrar con cualquier usuario del login de demo. Verificar, con los datos de la API (no los del prototipo):
1. Perfiles: registrar un jugador; aparece después de recargar.
2. Catálogo: registrar la mesa `M-19`; cambiar sus límites.
3. Operación de mesas: abrir una mesa y aprobar el relleno de M-04.
4. Caja: compra de 2 fichas de $50.000 al jugador p1; la transacción aparece y el stock de la bóveda baja 2.
5. Inventario: "Pedir al proveedor" suma 1.500 fichas (comportamiento del Sprint 1).
6. Retención: aceptar una oferta.
7. Con la API apagada aparece el toast de modo demostración y el prototipo sigue funcionando.

Anotar el resultado de cada punto en la bitácora.

- [ ] **J0.9 Limpieza**

Borrar `registro.py` (CER-8). En `docs/index.md`: arreglar las listas mal indentadas de las secciones 2 a 5, cambiar "`/` — Source code (Python)" por "`/src` — Código fuente C# (.NET 8)" y agregar el enlace `docs/arc42/README.md`.

- [ ] **J0.10 Commit, PR y aviso**

```powershell
git add -A; git commit -m "HU-00: Fase 0 - modulos DI, ISemilla, RecrearAlIniciar, front dividido en js/api, CI con cobertura (CER-8)"; git push -u origin HEAD
```

Abrir el PR hacia `develop`. Por excepción, basta la aprobación de **cualquier** integrante para hacer merge: los demás dependen de este PR. Después del merge, crear `desarrollo/decisiones/D-007-jorge-fase-0-lista.md` (estado `Aceptada`) con el texto "Fase 0 en develop: hagan `git pull --rebase origin develop`" y avisar al equipo por el chat del grupo.

---

### Tarea J1 · Sprint 2 en Jira

Jira hoy tiene: épicas CER-1 a CER-4, historias CER-5 a CER-7 (Sprint 1) y la tarea CER-8.

- [ ] **J1.1** Crear el sprint "Sprint 2" en el tablero CER con el Sprint Goal (copiar el de `desarrollo/README.md`).
- [ ] **J1.2** Crear las épicas: "Talento Humano y Nómina", "Inventario y Compras", "Seguridad y Control de Acceso" e "Infraestructura y Arquitectura".
- [ ] **J1.3** Crear las historias con su texto "Como… quiero… para…" (de la guía) y sus puntos: HU-04 Cálculo de nómina (8, Felipe), HU-05 Alerta de stock crítico (5, Camilo), HU-06 Asignación de roles (5, Iván), HU-07 Infraestructura como código (13, Jorge) y HU-08 Documentación de seguridad y despliegue (3, Jorge). Agregar además la historia "CRUD de Proveedores y orden de compra automática" (3, Camilo).
- [ ] **J1.4** Crear una subtarea por cada tarea de cada plan (F1…F8, C1…C9, I1…I8, J0…J9) con su responsable. Mover CER-8 al Sprint 2 bajo J0.
- [ ] **J1.5** Anotar en la bitácora las claves CER-NN creadas y publicarlas en `desarrollo/README.md` (tabla "Jira").

---

### Tarea J2 · Backend remoto del state de Terraform (HU-07)

**Requisito:** acceso a la suscripción de Azure donde están `rg-pitboss-erp` y `pitboss-erp-8szxa`. La creó el autor del commit `b9f4410` (usuario de GitHub Flashtwiice). Hay que pedirle el rol **Owner** o **Contributor + User Access Administrator** sobre la suscripción o sobre `rg-pitboss-erp`. Si no se consigue, queda como bloqueo en la bitácora y se avisa a Jorge de inmediato.

**Archivos:** `infra/terraform/bootstrap/main.tf`, `infra/terraform/bootstrap/outputs.tf`, `.gitignore` (agregar `.terraform/`, `*.tfstate`, `*.tfstate.*`, `backend.hcl`, `*.tfplan`, `.terraform.lock.hcl` **no**: el lock sí se sube).

- [ ] **J2.1** Escribir `bootstrap/main.tf`:
  - `terraform { required_version = ">= 1.6" required_providers { azurerm = { source = "hashicorp/azurerm", version = "~> 4.0" } random = { source = "hashicorp/random", version = "~> 3.6" } } }` y `provider "azurerm" { features {} }`
  - `random_string.sufijo` (6 caracteres, `upper = false`, `special = false`)
  - `azurerm_resource_group.tfstate` (`"rg-pitboss-tfstate"`, `"brazilsouth"`)
  - `azurerm_storage_account.tfstate` (`"stpitbosstf${random_string.sufijo.result}"`, `Standard`/`LRS`, `min_tls_version = "TLS1_2"`, `allow_nested_items_to_be_public = false`, `shared_access_key_enabled = false`)
  - `azurerm_storage_container.tfstate` (`"tfstate"`, `container_access_type = "private"`, `storage_account_id`)
  - Outputs: `resource_group_name` y `storage_account_name`.
- [ ] **J2.2** Aplicar: `az login`, luego `$env:ARM_SUBSCRIPTION_ID="<id>"`, `terraform -chdir=infra/terraform/bootstrap init` y `terraform -chdir=infra/terraform/bootstrap apply`. Esperado: `Apply complete! Resources: 4 added`. El `terraform.tfstate` local del bootstrap **no se sube**; se guarda en un lugar seguro y se anota dónde en la bitácora.
- [ ] **J2.3** Asignarse el rol `Storage Blob Data Contributor` sobre la cuenta de storage (`az role assignment create`).
- [ ] **J2.4** Commit: `HU-07: Bootstrap del backend remoto del state en Azure Storage`.

---

### Tarea J3 · Terraform de la infraestructura existente, con import (HU-07)

**Archivos:** `infra/terraform/versions.tf`, `backend.tf`, `variables.tf`, `main.tf`, `outputs.tf`, `backend.hcl.example`, `README.md`.

**Recursos que se importan** (de la rama de Azure):

| Recurso Terraform | Recurso en Azure |
|---|---|
| `azurerm_resource_group.principal` | `rg-pitboss-erp` (brazilsouth) |
| `azurerm_service_plan.principal` | el plan F1 de la app (nombre con `az appservice plan list -g rg-pitboss-erp --query "[].name"`) |
| `azurerm_linux_web_app.erp` | `pitboss-erp-8szxa` (identidad `SystemAssigned`, .NET 8) |
| `azurerm_mssql_server.principal` | `pitboss-sql-8szxa` (bloque `azuread_administrator` con `azuread_authentication_only = true`) |
| `azurerm_mssql_database.erp` | `CasinoErp` (oferta gratuita serverless) |
| `azurerm_mssql_firewall_rule.*` | las reglas que devuelva `az sql server firewall-rule list -g rg-pitboss-erp -s pitboss-sql-8szxa` |

- [ ] **J3.1** `versions.tf`: las mismas versiones que el bootstrap. `provider "azurerm" { features {} }` (la suscripción llega por `ARM_SUBSCRIPTION_ID`).
- [ ] **J3.2** `backend.tf`: `terraform { backend "azurerm" { key = "pitboss.tfstate" use_azuread_auth = true } }`. `backend.hcl.example` contiene `resource_group_name = "rg-pitboss-tfstate"`, `storage_account_name = "stpitbosstfXXXXXX"` y `container_name = "tfstate"`.
- [ ] **J3.3** `variables.tf`: `ubicacion` (por defecto `"brazilsouth"`), `nombre_app` (`"pitboss-erp-8szxa"`), `nombre_sql` (`"pitboss-sql-8szxa"`), `codigo_totp_demo` (string, por defecto `""`, `sensitive = true`) y `recrear_base_al_iniciar` (bool, por defecto `false`).
- [ ] **J3.4** `main.tf`: los recursos de la tabla, más `random_password.jwt` (48 caracteres, `special = false`). Los `app_settings` de la web app:
  - `BaseDatos__Proveedor = "SqlServer"`
  - `ConnectionStrings__CasinoErp = "Server=tcp:${azurerm_mssql_server.principal.fully_qualified_domain_name},1433;Database=CasinoErp;Authentication=Active Directory Default;Encrypt=True;"`
  - `TZ = "America/Bogota"`
  - `Jwt__Clave = random_password.jwt.result`
  - `Seguridad__CodigoTotpDemo = var.codigo_totp_demo`
  - `BaseDatos__RecrearAlIniciar = tostring(var.recrear_base_al_iniciar)`
  
  `site_config { always_on = false application_stack { dotnet_version = "8.0" } }`, porque F1 no permite `always_on`.
- [ ] **J3.5** Importar (state remoto) y llevar el plan a cero diferencias:

```powershell
Copy-Item infra/terraform/backend.hcl.example infra/terraform/backend.hcl   # completar con los outputs de J2
terraform -chdir=infra/terraform init -backend-config=backend.hcl
terraform -chdir=infra/terraform import azurerm_resource_group.principal /subscriptions/$env:ARM_SUBSCRIPTION_ID/resourceGroups/rg-pitboss-erp
# ... un import por recurso de la tabla (IDs con az resource list -g rg-pitboss-erp --query "[].id")
terraform -chdir=infra/terraform plan
```

Se ajustan los argumentos hasta que el plan **solo** muestre los cambios intencionales: la creación de `random_password.jwt` y el agregado de `Jwt__Clave`, `Seguridad__CodigoTotpDemo` y `BaseDatos__RecrearAlIniciar`. Los atributos que el proveedor no puede representar (por ejemplo, los de la oferta gratuita de SQL) van en `lifecycle { ignore_changes = [...] }`, con un comentario que explique por qué. **No aplicar todavía** (se aplica en J6).
- [ ] **J3.6** `outputs.tf`: `url_app = "https://${azurerm_linux_web_app.erp.default_hostname}"` y `principal_id_app` (la identidad administrada).
- [ ] **J3.7** `infra/terraform/README.md`: requisitos (Azure CLI, Terraform ≥ 1.6), orden (bootstrap → `backend.hcl` → `init` → imports, que ya están hechos en el state remoto → `plan`/`apply`), cómo publicar la app (los comandos de la sección "Despliegue en Azure" del README) y una nota: el usuario SQL de la identidad administrada (`CREATE USER [pitboss-erp-8szxa] FROM EXTERNAL PROVIDER`) se creó a mano y no lo maneja Terraform.
- [ ] **J3.8** Verificar: `terraform -chdir=infra/terraform fmt -check -recursive` y `terraform -chdir=infra/terraform validate` → `Success! The configuration is valid.`
- [ ] **J3.9** Commit y PR: `HU-07: Terraform de la infraestructura existente (App Service F1, Azure SQL) con state remoto`.

---

### Tarea J4 · Workflow `terraform plan` en cada PR a `develop` (HU-07 y Tarea 3 de la guía)

**Archivos:** `.github/workflows/terraform-plan.yml`; `infra/terraform/README.md` (sección OIDC).

- [ ] **J4.1** Crear la identidad del workflow: un App Registration "github-pitboss-terraform" con una credencial federada para `repo:camilo1503/Casino-ERP-Repositorio:pull_request`, el rol `Contributor` sobre `rg-pitboss-erp` (hace falta para leer los app settings) y el rol `Storage Blob Data Contributor` sobre la cuenta del state. Quien administre el repo carga los secretos `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RG` y `TFSTATE_SA`.
- [ ] **J4.2** Escribir el workflow:

```yaml
name: Terraform plan
on:
  pull_request:
    branches: [develop]
    paths: ['infra/terraform/**/*.tf']
permissions: { id-token: write, contents: read }
jobs:
  plan:
    runs-on: ubuntu-latest
    defaults: { run: { working-directory: infra/terraform } }
    env:
      ARM_USE_OIDC: true
      ARM_USE_AZUREAD: true
      ARM_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
      ARM_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
      ARM_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    steps:
      - uses: actions/checkout@v4
      - uses: hashicorp/setup-terraform@v3
        with: { terraform_version: 1.9.8 }
      - run: terraform fmt -check -recursive
      - run: >
          terraform init -input=false
          -backend-config="resource_group_name=${{ secrets.TFSTATE_RG }}"
          -backend-config="storage_account_name=${{ secrets.TFSTATE_SA }}"
          -backend-config="container_name=tfstate"
      - run: terraform validate
      - run: terraform plan -input=false -no-color -lock-timeout=2m | tee plan.txt
      - run: { echo '```'; cat plan.txt; echo '```'; } >> "$GITHUB_STEP_SUMMARY"
```

- [ ] **J4.3** Probar: abrir un PR a `develop` que solo agregue un comentario en `variables.tf`. Esperado: el check "Terraform plan" en verde y el resumen con `No changes.` (o solo los cambios pendientes de J6). Guardar una captura en `docs/sprint-2/evidencias/terraform-plan-pr.png`.
- [ ] **J4.4** Commit: `HU-07: Workflow de terraform plan en PR a develop con OIDC`.

---

### Tarea J5 · Integración continua del equipo (rol permanente durante el sprint)

- [ ] **J5.1** Cada día que haya trabajo, actualizar el **tablero** de `desarrollo/README.md`: el estado de cada tarea de los cuatro planes, leído de las bitácoras y los PR.
- [ ] **J5.2** Revisar los PR abiertos con este checklist: DoD de AGENT.md §7, contratos respetados, que no toque archivos ajenos sin justificar, bitácora actualizada y CI en verde. Comentar en el PR lo que falte.
- [ ] **J5.3** Si `develop` queda en rojo: identificar el commit con `git log` y el CI, avisar al autor y, si en 2 horas no hay arreglo, revertir con `git revert` (nunca reescribir la historia). Registrar el caso en la bitácora.
- [ ] **J5.4** Actualizar la tabla de `desarrollo/decisiones/README.md` con las decisiones nuevas.
- [ ] **J5.5** Hitos que hay que vigilar y anunciar en el tablero:
  - "Fase 0 en develop" (J0)
  - "C4 en develop" (Felipe F1)
  - "Seguridad activa en develop" (Iván I5): a partir de ahí, todos deben hacer `pull --rebase` y anotar sus controladores
  - "Congelamiento": todo en `develop` antes de J6

---

### Tarea J6 · Desplegar el Sprint 2 en Azure

**Requisito:** todos los PR de HU-04, HU-05 y HU-06 en `develop`, con el CI en verde.

- [ ] **J6.1** `terraform -chdir=infra/terraform apply -var="recrear_base_al_iniciar=true" -var="codigo_totp_demo=482913"`. El código de demo solo queda activo para la Sprint Review y queda anotado como decisión `D-0NN-jorge-totp-demo-en-azure`, porque es un riesgo aceptado.
- [ ] **J6.2** Publicar la app: `dotnet publish src/CasinoErp.Api -c Release -o publish`, luego `Compress-Archive publish\* publish.zip -Force` y luego `az webapp deploy -g rg-pitboss-erp -n pitboss-erp-8szxa --src-path publish.zip --type zip`.
- [ ] **J6.3** Prueba de humo en `https://pitboss-erp-8szxa.azurewebsites.net`:
  - `GET /api/salud` responde 200;
  - login como `felipe.cortes@pitboss.co` (contraseña `Casino2026`, código `482913`) y cálculo de la nómina de octubre;
  - login como `laura.gomez@pitboss.co`: solo ve Caja e Inventario.
  
  La primera carga puede tardar hasta un minuto porque la base gratuita se reanuda.
- [ ] **J6.4** Volver a `recrear_base_al_iniciar=false` con un nuevo `apply`. Verificar en `terraform plan` que no quedan cambios.
- [ ] **J6.5** Anotar en la bitácora la URL, la hora y el resultado de cada punto de la prueba de humo.

---

### Tarea J7 · arc42: consolidación (HU-08)

**Estructura de `docs/arc42/` (cada archivo tiene un solo dueño):**

| Archivo | Dueño | Contenido |
|---|---|---|
| `README.md` | Jorge | Índice de las 12 secciones de arc42; las que no se tocan en el Sprint 2 dicen "Pendiente – Sprint 3" |
| `05-vista-bloques.md` | Jorge | Nivel 1 (Front → Api → Dominio ← Infraestructura → SQL) e índice de bloques |
| `05-bloques/nomina.md` | Felipe | Bloque Nómina, SRP/DIP y diagrama de secuencia de `POST /api/nomina/calculate` |
| `05-bloques/inventario-compras.md` | Camilo | Bloque Inventario/Compras y patrón observador |
| `05-bloques/seguridad.md` | Iván | Bloque Seguridad |
| `07-vista-despliegue.md` | Jorge | Despliegue en Azure y en local |
| `09-decisiones/ADR-001…004` | Jorge | Decisiones de arquitectura |
| `09-decisiones/ADR-005-parametros-nomina.md` | Felipe | |
| `09-decisiones/ADR-002-jwt-totp.md` | Iván | |
| `10-conceptos-seguridad.md` | Iván | Sección 10 (seguridad y roles) |

- [ ] **J7.1** `README.md` y `05-vista-bloques.md` (nivel 1 con Mermaid `flowchart LR` y la tabla de bloques con enlaces a `05-bloques/*`).
- [ ] **J7.2** `07-vista-despliegue.md`:
  - diagrama Mermaid: Navegador → App Service Linux F1 `pitboss-erp-8szxa` (.NET 8, identidad administrada) → Azure SQL serverless gratuito `CasinoErp` (solo Entra ID); el state de Terraform en `stpitbosstf…/tfstate`; GitHub Actions (CI .NET + `terraform plan` con OIDC);
  - una tabla de nodo, recurso Terraform, configuración y secretos (dónde vive cada uno: `Jwt__Clave` en un app setting generado por Terraform, sin contraseñas SQL);
  - las diferencias entre local y nube (SQLite frente a Azure SQL; `CodigoTotpDemo` solo en local o en la demo);
  - el procedimiento de despliegue de J6.
- [ ] **J7.3** ADRs, cada uno con Contexto / Decisión / Consecuencias:
  - ADR-001 Monolito modular frente a microservicio de nómina (de D-002)
  - ADR-003 Terraform en Azure con import y state remoto en Blob (de D-005)
  - ADR-004 `EnsureCreated` + `RecrearAlIniciar` en vez de migraciones, como deuda para el Sprint 3 (de D-006)
- [ ] **J7.4** **Revisión cruzada (Tarea 2 de la guía):** verificar que cada control de la sección 10 (Iván) aparece en su bloque de `05-bloques/*` (por ejemplo, `[AutorizarModulo(Modulos.Rrhh)]` en `NominaController` dentro del bloque Nómina de Felipe). Anotar las incoherencias como comentarios en el PR del dueño y el resultado en `docs/sprint-2/guion-video.md`.
- [ ] **J7.5** Commit: `HU-08: arc42 indice, vista de despliegue y ADR 001/003/004`.

---

### Tarea J8 · Artefactos Scrum (`docs/sprint-2/`)

- [ ] **J8.1** `sprint-backlog.md`: Sprint Goal; capacidad (una tabla por integrante con días disponibles y velocidad del Sprint 1, que la llena el equipo); las HU con sus SP y las claves de Jira de J1; las tareas por plan con su responsable; y la DoD de AGENT.md §7.
- [ ] **J8.2** `retrospectiva.md`:
  - las secciones Qué salió bien, Qué se puede mejorar e Impedimentos, una por integrante, con una pregunta guía (no se inventa contenido);
  - **Tarea 1 de la guía:** por qué la HU-04 pudo subestimarse. La evidencia está en la bitácora de Felipe y en los casos de borde: auxilio de transporte en el límite exacto de 2 SMMLV, fondo de solidaridad desde 4 SMMLV, recargo nocturno que saca al crupier del auxilio, prestación de servicios sin aportes y la disputa judicial del decreto 2026;
  - la regla de estimación para el Sprint 3: *"Toda HU con norma legal (impuestos, facturación electrónica DIAN, nómina) suma un spike de 2 SP en el Sprint Planning y se estima después del spike con una lista explícita de casos de borde"*;
  - las acciones de mejora de la guía (spike para HU complejas; CI de `terraform plan`, ya hecho en J4).
- [ ] **J8.3** `investigacion-terraform-actions.md` (Tarea 3 de la guía): cómo se disparó el workflow (`pull_request` + `branches` + `paths`), OIDC frente a secretos de larga duración, qué hacen `fmt`, `validate` y `plan`, por qué el state remoto con lease de Blob evita conflictos, enlace a `.github/workflows/terraform-plan.yml` y la captura de J4.3.
- [ ] **J8.4** `guion-video.md` (Tarea 2 de la guía, máximo 60 s):
  - 0–15 s: sección 5, bloque Nómina, frontera del módulo;
  - 15–40 s: sección 10, matriz de roles y `[AutorizarModulo(Modulos.Rrhh)]`;
  - 40–60 s: conclusión de coherencia (cada control de la 10 tiene su lugar en la 5).
  
  Quién graba y el enlace del video se anotan aquí.
- [ ] **J8.5** Commit: `Sprint 2: backlog, retrospectiva, investigacion de Terraform en Actions y guion del video`.

---

### Tarea J9 · Cierre del sprint

- [ ] **J9.1** `README.md`: tabla de módulos y endpoints nuevos (empleados, nómina, alertas, proveedores, compras, auth, usuarios), cuentas de demo (enlace a `desarrollo/contratos.md` §C6), número de pruebas, cobertura y sección de Terraform.
- [ ] **J9.2** Verificación final: `dotnet test CasinoErp.sln -c Release` con 0 fallos y el comando de cobertura ≥ 80 % (anotar el porcentaje); `terraform validate` en verde; el CI de `develop` en verde.
- [ ] **J9.3** Ensayo de la Sprint Review (guion en `desarrollo/README.md` §Demo), con el equipo y en Azure.
- [ ] **J9.4** PR de `develop` a `main` con el título "Sprint 2 · PitBoss ERP". Después del merge: `git tag v0.2.0` y `git push origin v0.2.0`.
- [ ] **J9.5** Cerrar el sprint en Jira y escribir la entrada final en la bitácora.
