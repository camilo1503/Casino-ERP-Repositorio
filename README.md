# Casino-ERP-Repositorio
Sistema ERP para la gestión integral de casinos, desarrollado para Ingeniería de Software II (6° semestre). Incluye perfiles de jugador, catálogo de mesas, caja, retención de jugadores, rellenos de fichas e inventario de la bóveda (Sprint 1), y en el Sprint 2 talento humano y nómina, usuarios con roles y autenticación de dos factores, y la infraestructura de Azure como código.

## Incremento: PitBoss ERP conectado a la API (C# · ASP.NET Core · SQL Server)

El front del prototipo (`src/CasinoErp.Api/wwwroot/index.html`) se mantiene igual y ahora guarda en la base de datos a través de la API.
Los archivos de `wwwroot/js/api/` (`nucleo.js`, `sprint1.js`, `inventario.js`, `personal.js`, `seguridad.js`, `compras.js` y `arranque.js`) conectan los módulos sin cambiar las pantallas:

| Módulo del front | Qué se guarda en la API |
| --- | --- |
| Perfiles de jugador | Registro, edición del perfil, desactivar, autoexcluir y reactivar |
| Catálogo y búsqueda | Registrar, editar (límites, ubicación y estado) y retirar mesas |
| Operación de mesas | Abrir, cerrar, mantenimiento, solicitar y aprobar rellenos (descuenta la bóveda) |
| Caja / POS | Compras y pagos, descuento de retención, aprobar y anular; puntos de lealtad |
| Inventario de fichas | Stock de la bóveda por denominación (compras, pagos, pedidos y reposición) |
| Retención de jugadores | Ofertas aceptadas y rechazadas, límite diario y configuración |
| Cumplimiento | Reportar y escalar transacciones sobre el umbral |

Competencias, Turnos y nómina, Usuarios y el inicio de sesión (con 2FA) ya usan la API desde el Sprint 2. Promociones, Reportes y la app del jugador siguen con los datos de ejemplo del prototipo.
Si la API rechaza una operación, el front muestra el mensaje de la regla de negocio y vuelve a cargar los datos reales.

| Categoría | Tecnología |
| --- | --- |
| Front | Prototipo PitBoss ERP (HTML, CSS y JavaScript) servido por la misma API |
| Lenguaje backend | C# (.NET 8) |
| Framework backend | ASP.NET Core Web API |
| Base de datos | SQL Server / Azure SQL Database con Entity Framework Core (SQLite para desarrollo local) |
| Pruebas | xUnit, FluentAssertions, Moq, WebApplicationFactory, Coverlet |
| IDE | Visual Studio 2022 |
| CI | GitHub Actions (`.github/workflows/ci.yml`) |

### Estructura

```
CasinoErp.sln
src/CasinoErp.Dominio/          reglas de negocio (servicios, entidades, interfaces de repositorio)
src/CasinoErp.Infraestructura/  Entity Framework Core, repositorios y datos de demostración
src/CasinoErp.Api/              controladores REST + Swagger
src/CasinoErp.Api/wwwroot/       front del ERP + js/api/*.js (un archivo por módulo)
tests/CasinoErp.PruebasUnitarias/    271 pruebas unitarias del dominio (TDD)
tests/CasinoErp.PruebasIntegracion/  62 pruebas de la API contra una base SQLite real
infra/terraform/                 infraestructura de Azure como código (HU-07)
docs/arc42/                      arquitectura (arc42): bloques, despliegue, seguridad y ADR
docs/sprint-2/                   artefactos Scrum del Sprint 2
desarrollo/                      planes, bitácoras, contratos y decisiones del Sprint 2
```

### Módulos y endpoints

| Módulo | Endpoints principales |
| --- | --- |
| Perfiles de jugador (E-01) | `GET/POST /api/jugadores`, `PUT /api/jugadores/{documento}`, `PUT .../perfil`, `POST .../desactivar`, `.../autoexcluir`, `.../reactivar` |
| Catálogo de mesas (E-02) | `GET/POST /api/mesas`, `PUT /api/mesas/{numero}/limites`, `POST .../abrir`, `.../cerrar`, `.../mantenimiento`, `.../reparada`, `.../retirar` |
| Caja / POS | `GET/POST /api/caja/transacciones`, `POST .../{id}/aprobar`, `.../anular`, `.../cumplimiento`, `GET /api/caja/total` |
| Retención (E-03) | `GET/POST /api/retencion/ofertas`, `POST /api/retencion/descuento`, `GET/PUT /api/retencion/configuracion` |
| Rellenos y bóveda | `GET/POST /api/rellenos`, `POST /api/rellenos/{id}/aprobar`, `.../rechazar`, `GET /api/boveda`, `POST /api/boveda/depositos` |
| Inventario de fichas | `GET/POST /api/inventario/fichas`, `POST .../entradas`, `POST .../retiros`, `GET /api/inventario/movimientos` |
| Alertas de stock crítico (HU-05) | `PUT /api/inventario/fichas/{codigo}/minimo`, `GET /api/inventario/alertas` |
| Tablero | `GET /api/resumen` |
| Empleados (HU-04) | `GET/POST /api/empleados`, `GET/PUT /api/empleados/{documento}`, `POST /api/empleados/{documento}/retirar` |
| Nómina (HU-04) | `POST /api/nomina/calculate`, `GET /api/nomina`, `GET /api/nomina/{anio}/{mes}`, `POST /api/nomina/{id}/aprobar`, `GET/PUT /api/nomina/parametros/{anio}` |
| Autenticación (HU-06) | `POST /api/auth/login` (con código 2FA), `GET /api/auth/yo`, `POST /api/auth/cambiar-contrasena` |
| Usuarios y roles (HU-06) | `GET/POST /api/usuarios`, `PUT /api/usuarios/{id}/rol`, `POST .../restablecer`, `.../desactivar`, `.../reactivar` |
| Estado del servicio | `GET /api/salud` (anónimo) |

Todos los endpoints, salvo `login` y `salud`, exigen el token JWT y el rol con acceso al módulo (`[AutorizarModulo]`); sin permiso responden `403`. Los proveedores y las órdenes de compra se agregarán a esta tabla cuando entren en `develop`.

Los errores de negocio responden `400` y los registros inexistentes `404`, ambos con `{ "mensaje": "..." }` (los mismos mensajes de los escenarios Gherkin).

### Cómo ejecutar

Requiere el SDK de .NET 8. En Visual Studio 2022 basta con abrir `CasinoErp.sln` y ejecutar `CasinoErp.Api`.

```bash
dotnet test CasinoErp.sln                     # 333 pruebas (271 unitarias + 62 de integración)
dotnet run --project src/CasinoErp.Api        # front: http://localhost:5080 · API: http://localhost:5080/swagger
```

Por defecto usa SQLite (`casino-erp.db`) y carga los mismos datos de ejemplo del prototipo. Para empezar de cero, borra `src/CasinoErp.Api/casino-erp.db`. Para SQL Server / Azure SQL, completar el servidor en `src/CasinoErp.Api/appsettings.SqlServer.json` y ejecutar con `ASPNETCORE_ENVIRONMENT=SqlServer`.

Cobertura del dominio con las pruebas unitarias: 97,36 % de líneas, 90,24 % de ramas y 94,88 % de métodos (medido el 2026-11-19 con el comando de AGENT.md §10; el CI exige al menos 80 % de líneas). Se vuelve a medir al cierre del sprint.

## Pruebas (Ingeniería de Software II · Taller de herramientas de prueba para CRUD)

La carpeta [`pruebas/`](pruebas/) contiene el taller aplicado a PitBoss ERP: escenarios Gherkin en español, pruebas unitarias con JUnit 5 y Jest, BDD con Cucumber-JS, Cucumber-JVM y Reqnroll (sucesor de SpecFlow) y un reporte HTML con Font Awesome.

| Herramienta | Resultado |
| --- | --- |
| Jest | 63 de 63 pruebas aprobadas, cobertura de líneas 97,9 % |
| Cucumber-JS | 37 de 37 escenarios aprobados |
| JUnit 5 + Cucumber-JVM (`mvn test`) | 83 pruebas, 0 fallos; JaCoCo 95,4 % de líneas |
| Reqnroll (`dotnet test`) | 2 de 2 escenarios aprobados |

Instrucciones de ejecución y evidencias en [`pruebas/README.md`](pruebas/README.md).

### Cuentas de demostración (Sprint 2)

Todas usan la contraseña `Casino2026`. El código de doble factor de demostración es `482913` y solo se acepta cuando `Seguridad:CodigoTotpDemo` está configurado (Development y pruebas). La lista completa de cuentas y roles está en [`desarrollo/contratos.md`](desarrollo/contratos.md) (C6). Ejemplos: `felipe.cortes@pitboss.co` (Talento humano), `laura.gomez@pitboss.co` (Cajero) e `ivan.leon@pitboss.co` (Administrador).

### Infraestructura como código (Terraform)

La infraestructura de Azure está descrita en [`infra/terraform/`](infra/terraform/README.md): el state vive en Azure Blob Storage (`bootstrap/` lo crea) y los recursos existentes se importaron. Cada PR a `develop` que cambia un `.tf` ejecuta `terraform plan` con el workflow [`terraform-plan.yml`](.github/workflows/terraform-plan.yml) (OIDC, sin secretos de larga duración). Más detalle en [`docs/arc42/07-vista-despliegue.md`](docs/arc42/07-vista-despliegue.md).

### Despliegue en Azure

La aplicación está publicada en **https://pitboss-erp-8szxa.azurewebsites.net** (API en `/swagger`).

| Recurso | Nombre | Plan |
| --- | --- | --- |
| Grupo de recursos | `rg-pitboss-erp` (Brazil South) | — |
| Azure App Service (Linux, .NET 8) | `pitboss-erp-8szxa` | F1 gratuito |
| Azure SQL Database | `CasinoErp` en el servidor `pitboss-sql-8szxa` | Oferta gratuita (serverless, se pausa si se agota) |

- **Sin contraseñas:** el servidor SQL solo acepta Microsoft Entra ID y la aplicación se conecta con su identidad administrada (`Authentication=Active Directory Default`). No hay claves en el código ni en GitHub.
- **Configuración del App Service:** `BaseDatos__Proveedor=SqlServer`, `ConnectionStrings__CasinoErp=Server=tcp:pitboss-sql-8szxa.database.windows.net,1433;Database=CasinoErp;Authentication=Active Directory Default;Encrypt=True;`, `TZ=America/Bogota`.
- Al arrancar, la API crea las tablas en Azure SQL y carga los datos de ejemplo si la base está vacía.
- La base gratuita se pausa cuando no se usa: la primera carga después de un rato puede tardar hasta un minuto (la API reintenta la conexión).

Para publicar una nueva versión (con Azure CLI y la sesión iniciada con `az login`):

```bash
dotnet publish src/CasinoErp.Api -c Release -o publish
cd publish && zip -r ../publish.zip . && cd ..
az webapp deploy -g rg-pitboss-erp -n pitboss-erp-8szxa --src-path publish.zip --type zip
```

### Integrantes

- Daniel Camilo Ruiz Rodríguez
- Jorge Alcides Díaz Lozano
- Iván Felipe León Pinilla
- Felipe Antonio Cortés Hernández
