# Bitácora · Ivan · Plan-Ivan

> Solo la edita Ivan (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| I0 | Arranque | Hecha | [#4](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/4) |
| I1 | Roles, módulos y matriz (C5) | En revisión | [#4](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/4) |
| I2 | Usuario y autenticación | En revisión | [#5](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/5) (depende de #4) |
| I3 | UsuarioService (necesita C4) | En revisión | [#6](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/6) (depende de #4 y #5) |
| I4 | Infraestructura y cuentas de demo (C6) | En revisión | [#7](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/7) (depende de #4–#6) |
| I5 | JWT y autorización (hito Seguridad activa) | Pendiente | — |
| I6 | Front seguridad.js | Pendiente | — |
| I7 | arc42 sección 10 | Pendiente | — |
| I8 | Cierre y demo | Pendiente | — |

## Pendientes abiertos
- Que Iván confirme la regla de I3: cambiar a Crupier con un documento vinculado que no existe → "Empleado no encontrado".
- Fusión en orden de #4 → #5 → #6 → #7 (la hace Jorge).

## Bloqueos abiertos
- (ninguno)

## Entradas

## 2026-11-18 23:15 · Tarea I4 · Terminada (en revisión)
**Hecho:**
- Paquetes `Microsoft.Extensions.Identity.Core` 8.0.11 y `Otp.NET` 1.4.0 en `CasinoErp.Infraestructura.csproj` (solo esos dos, como dice el plan).
- `Seguridad/HasherContrasena.cs` (PBKDF2 de Identity) y `Seguridad/VerificadorTotp.cs` (Otp.NET, ventana RFC, código demo solo si `Seguridad:CodigoTotpDemo` está configurado).
- `Configuraciones/UsuarioConfiguracion.cs`, `Repositorios/SeguridadRepositorios.cs`, `Semillas/SemillaSeguridad.cs` (11 cuentas de C6, `Orden = 40`, idempotente) y `Modulos/ModuloSeguridad.cs`.
- `tests/CasinoErp.PruebasIntegracion/SeguridadApiTests.cs`: `La_semilla_crea_las_11_cuentas_de_demo` (I4.1) y `El_verificador_acepta_un_codigo_TOTP_real_y_rechaza_uno_errado` (extra: el código demo no debe tapar el TOTP real).
**Evidencia:**
- TDD: `dotnet test tests/CasinoErp.PruebasIntegracion --filter SeguridadApiTests` → falla con `Cannot create a DbSet for 'Usuario' because this type is not included in the model`; tras implementar → verde.
- `dotnet build CasinoErp.sln` → 0 advertencias.
- `dotnet test CasinoErp.sln` → 296 aprobadas (260 unitarias + 36 de integración), 0 fallos.
- Cobertura de líneas de `CasinoErp.Dominio` → 97,22 %.
**Decisiones:** ninguna fuera del plan.
**Pendientes:** las bases locales viejas no tienen la tabla `Usuarios`; hay que arrancar una vez con `BaseDatos__RecrearAlIniciar=true` (D-006).
**Bloqueos / necesito de otros:** fusión en orden #4 → #5 → #6 → #7.
**Siguiente paso:** I5 (JWT, login y `[AutorizarModulo]` en toda la API) en `feature/HU-06-jwt`.
**Commits/PR:** `91a0263` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/7

## 2026-11-18 23:00 · Tarea I3 · Terminada (en revisión)
**Hecho:**
- `src/CasinoErp.Dominio/Seguridad/UsuarioService.cs`: `UsuarioService`, `UsuarioCreado` y `ContrasenaRestablecida`, con las reglas en el orden de la tabla del plan y los mensajes copiados literalmente.
- `tests/CasinoErp.PruebasUnitarias/UsuarioServiceTests.cs`: las 18 pruebas de I3.1 y 3 más (`Un_administrador_inactivo_no_cuenta_como_otro_administrador`, `Cambiar_a_crupier_exige_un_empleado_crupier`, `Consulta_y_lista_por_id`). Usa `EmpleadosEnMemoria` (C4).
**Evidencia:**
- TDD: `dotnet test tests/CasinoErp.PruebasUnitarias --filter UsuarioServiceTests` → falla al compilar (CS0246, no existe `UsuarioService`); tras implementar → 21 aprobadas.
- `dotnet build CasinoErp.sln` → 0 advertencias.
- `dotnet test CasinoErp.sln` → 294 aprobadas (260 unitarias + 34 de integración), 0 fallos.
- Cobertura de líneas de `CasinoErp.Dominio` → 97,22 %.
**Decisiones (solo afectan a mi módulo):**
- **Por confirmar con Iván:** `CambiarRolAsync` a Crupier cuando el documento vinculado ya no existe → `NoEncontradoException("Empleado no encontrado")`, igual que en `CrearAsync`. El plan no define este caso.
- "Único administrador activo" cuenta solo a los administradores activos.
- `DesactivarAsync` revisa primero "ya está inactivo" y después la regla del último administrador.
- La rama sale de `feature/HU-06-autenticacion` porque el #4 y el #5 aún no están fusionados.
**Pendientes:** la confirmación de la regla de arriba.
**Bloqueos / necesito de otros:** que Jorge fusione en orden #4 → #5 → #6.
**Siguiente paso:** I4 (hash con Identity, TOTP con Otp.NET, configuración EF, repositorio y semilla de las 11 cuentas) en `feature/HU-06-infraestructura`.
**Commits/PR:** `9061c6b` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/6

## 2026-11-18 22:45 · Tarea I2 · Terminada (en revisión)
**Hecho:**
- `src/CasinoErp.Dominio/Seguridad/Usuario.cs`: `Usuario`, `IUsuarioRepositorio`, `IHasherContrasena` e `IVerificadorTotp`, con las firmas del plan.
- `src/CasinoErp.Dominio/Seguridad/ContrasenaTemporal.cs`: `Generar()` y `EsSegura()`.
- `src/CasinoErp.Dominio/Seguridad/AutenticacionService.cs`: `IniciarSesionAsync` con el algoritmo del plan y los mensajes copiados literalmente.
- `tests/CasinoErp.PruebasUnitarias/Dobles/SeguridadEnMemoria.cs` (`UsuariosEnMemoria`, `HasherFalso`, `TotpFalso`, `RelojMovible`) y `AutenticacionServiceTests.cs` (13 pruebas, 16 casos).
**Evidencia:**
- TDD: `dotnet test tests/CasinoErp.PruebasUnitarias --filter AutenticacionServiceTests` → falla al compilar (CS0246, no existen `Usuario` ni `AutenticacionService`); tras implementar → 16 aprobadas.
- `dotnet build CasinoErp.sln` → 0 advertencias.
- `dotnet test CasinoErp.sln` → 273 aprobadas (239 unitarias + 34 de integración), 0 fallos.
- Cobertura de líneas de `CasinoErp.Dominio` → 97,35 %.
**Decisiones (solo afectan a mi módulo):**
- La rama `feature/HU-06-autenticacion` sale de `feature/HU-06-matriz-acceso` y no de `develop`, porque el PR #4 todavía no está fusionado y la I2 necesita `Rol` y `AutenticacionException`. Hay que fusionar el #4 antes del #5.
- Un PR por tarea (instrucción de Iván en el chat): I2 tiene su PR propio y la I3 va en otra rama. El plan (I3.5) proponía un solo PR para I2 + I3.
- `ContrasenaTemporal.Generar()` no usa caracteres ambiguos (`I`, `O`, `l`, `0`, `1`) porque la contraseña temporal se dicta o se copia a mano. Cumple igual la regla del plan (12 caracteres con al menos una mayúscula, una minúscula, un dígito y un carácter de `!@#$%&*?`).
**Pendientes:** ninguno de la I2.
**Bloqueos / necesito de otros:** que Jorge fusione el #4 y luego el #5.
**Siguiente paso:** I3 (`UsuarioService`) en `feature/HU-06-usuarios`. El C4 de Felipe ya está en `develop`.
**Commits/PR:** `2665919` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/5

## 2026-11-18 22:30 · Tarea I1 · Terminada (en revisión)
**Hecho:**
- `src/CasinoErp.Dominio/Seguridad/Rol.cs`: `Rol`, `Modulos` y `MatrizAcceso` exactamente como `contratos.md` §C5 (matriz estática con `StringComparer.Ordinal`; `RolesCon` en el orden del enum).
- `src/CasinoErp.Dominio/Comun/Excepciones.cs`: se agrega solo `AutenticacionException`.
- `tests/CasinoErp.PruebasUnitarias/MatrizAccesoTests.cs`: las 10 pruebas de I1.1 (13 casos). Se completaron los cuerpos de `PuedeAcceder_responde_segun_la_matriz` y `Todos_los_roles_tienen_al_menos_un_modulo`, que el plan solo describía.
**Evidencia:**
- TDD: `dotnet test tests/CasinoErp.PruebasUnitarias --filter MatrizAccesoTests` → falla al compilar (CS0234, no existe `CasinoErp.Dominio.Seguridad`); tras implementar → 13 aprobadas.
- `dotnet build CasinoErp.sln` → 0 advertencias.
- `dotnet test CasinoErp.sln` → 257 aprobadas (223 unitarias + 34 de integración), 0 fallos.
- Cobertura de líneas de `CasinoErp.Dominio` → 97,28 % (umbral 80 %).
**Decisiones:** ninguna fuera del plan.
**Pendientes:** mover CER-39 a Listo en Jira cuando se fusione el PR.
**Bloqueos / necesito de otros:** ninguno. Necesito que el integrador (Jorge) revise y fusione el PR #4.
**Siguiente paso:** I2 (`Usuario` y `AutenticacionService`) en `feature/HU-06-autenticacion`.
**Commits/PR:** `1e0db46` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/4

## 2026-11-18 22:10 · Tarea I0 · Terminada
**Hecho:** clonado el repo y creada la rama `feature/HU-06-matriz-acceso` desde `develop` (`241b185`). Se leyeron AGENT.md, desarrollo/README.md, Plan-Ivan, contratos y las decisiones D-001 a D-011.
**Evidencia:** `dotnet test CasinoErp.sln` en `develop` → 244 aprobadas (210 unitarias + 34 de integración), 0 fallos (SDK 8.0.428).
**Decisiones:** los commits se firman con la identidad de Git de Iván (`Ivanfelipe14 <ivanfelipeleonpinilla@gmail.com>`), configurada solo para el repo.
**Pendientes:** mover HU-06 (CER-16) a "En curso" en Jira.
**Bloqueos / necesito de otros:** ninguno. La Fase 0 (`8fff1ce`) y el C4 de Felipe (`f083b68`) ya están en `develop`, así que I3 e I4 no tienen bloqueo.
**Siguiente paso:** I1.
**Commits/PR:** —

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
