# Casino-ERP-Repositorio
Sistema ERP para la gestión integral de casinos, desarrollado para Ingeniería de Software II (6° semestre). Incluye perfiles de jugador, catálogo de mesas, caja, retención de jugadores, rellenos de fichas e inventario de la bóveda.

## Incremento: PitBoss ERP conectado a la API (C# · ASP.NET Core · SQL Server)

El front del prototipo (`src/CasinoErp.Api/wwwroot/index.html`) se mantiene igual y ahora guarda en la base de datos a través de la API.
El archivo `wwwroot/js/conexion-api.js` conecta los módulos del incremento sin cambiar las pantallas:

| Módulo del front | Qué se guarda en la API |
| --- | --- |
| Perfiles de jugador | Registro, edición del perfil, desactivar, autoexcluir y reactivar |
| Catálogo y búsqueda | Registrar, editar (límites, ubicación y estado) y retirar mesas |
| Operación de mesas | Abrir, cerrar, mantenimiento, solicitar y aprobar rellenos (descuenta la bóveda) |
| Caja / POS | Compras y pagos, descuento de retención, aprobar y anular; puntos de lealtad |
| Inventario de fichas | Stock de la bóveda por denominación (compras, pagos, pedidos y reposición) |
| Retención de jugadores | Ofertas aceptadas y rechazadas, límite diario y configuración |
| Cumplimiento | Reportar y escalar transacciones sobre el umbral |

Promociones, Competencias, Turnos y nómina, Reportes, Usuarios, el inicio de sesión y la app del jugador siguen con los datos de ejemplo del prototipo (próximo incremento).
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
src/CasinoErp.Api/wwwroot/       front del ERP + conexion-api.js
tests/CasinoErp.PruebasUnitarias/    132 pruebas unitarias del dominio (TDD)
tests/CasinoErp.PruebasIntegracion/  15 pruebas de la API contra una base SQLite real
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
| Tablero | `GET /api/resumen` |

Los errores de negocio responden `400` y los registros inexistentes `404`, ambos con `{ "mensaje": "..." }` (los mismos mensajes de los escenarios Gherkin).

### Cómo ejecutar

Requiere el SDK de .NET 8. En Visual Studio 2022 basta con abrir `CasinoErp.sln` y ejecutar `CasinoErp.Api`.

```bash
dotnet test                                   # 147 pruebas
dotnet run --project src/CasinoErp.Api        # front: http://localhost:5080 · API: http://localhost:5080/swagger
```

Por defecto usa SQLite (`casino-erp.db`) y carga los mismos datos de ejemplo del prototipo. Para empezar de cero, borra `src/CasinoErp.Api/casino-erp.db`. Para SQL Server / Azure SQL, completar el servidor en `src/CasinoErp.Api/appsettings.SqlServer.json` y ejecutar con `ASPNETCORE_ENVIRONMENT=SqlServer`.

Cobertura del dominio con las pruebas unitarias: 97 % de líneas y 88 % de ramas.

## Pruebas (Ingeniería de Software II · Taller de herramientas de prueba para CRUD)

La carpeta [`pruebas/`](pruebas/) contiene el taller aplicado a PitBoss ERP: escenarios Gherkin en español, pruebas unitarias con JUnit 5 y Jest, BDD con Cucumber-JS, Cucumber-JVM y Reqnroll (sucesor de SpecFlow) y un reporte HTML con Font Awesome.

| Herramienta | Resultado |
| --- | --- |
| Jest | 63 de 63 pruebas aprobadas, cobertura de líneas 97,9 % |
| Cucumber-JS | 37 de 37 escenarios aprobados |
| JUnit 5 + Cucumber-JVM (`mvn test`) | 83 pruebas, 0 fallos; JaCoCo 95,4 % de líneas |
| Reqnroll (`dotnet test`) | 2 de 2 escenarios aprobados |

Instrucciones de ejecución y evidencias en [`pruebas/README.md`](pruebas/README.md).

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
