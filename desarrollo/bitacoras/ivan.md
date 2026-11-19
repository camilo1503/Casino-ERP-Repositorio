# Bitácora · Ivan · Plan-Ivan

> Solo la edita Ivan (o su agente). Las entradas nuevas van **arriba** de la sección *Entradas*. Formato: AGENT.md §8.1.

## Estado de mi plan

| Tarea | Descripción | Estado | PR |
|---|---|---|---|
| I0 | Arranque | Hecha | [#4](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/4) |
| I1 | Roles, módulos y matriz (C5) | Hecha (`cce0aa1`) | [#4](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/4) |
| I2 | Usuario y autenticación | Hecha (`cce0aa1`) | [#5](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/5) |
| I3 | UsuarioService (necesita C4) | Hecha (`cce0aa1`) | [#6](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/6) |
| I4 | Infraestructura y cuentas de demo (C6) | Hecha (`cce0aa1`) | [#7](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/7) |
| I5 | JWT y autorización (hito Seguridad activa) | Hecha (`cce0aa1`; D-012) | [#9](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/9) |
| I6 | Front seguridad.js | Hecha (`cce0aa1`) | [#10](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/10) |
| I7 | arc42 sección 10 | Hecha (`2932d93`) | [#11](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/11) |
| I8 | Cierre y demo | Hecha (ensayo automático; sin ensayo con persona) | [#14](https://github.com/camilo1503/Casino-ERP-Repositorio/pull/14) |

## Pendientes abiertos
- ~~Confirmar la regla de I3~~ → confirmada el 2026-11-18 23:04.
- ~~Avisar al grupo y pasar #9 y #10 a listos~~ → hecho el 2026-11-18 23:04.
- ~~Merge de #4 → #11~~ → fusionados el 2026-11-18 23:23 (`cce0aa1`, `2932d93`).
- ~~I5.9: publicar la decisión de seguridad activa~~ → D-012.
- ~~I8.1 y I8.3: retrospectiva y HU-06 a Listo~~ → hecho el 2026-11-18 23:30, sin las horas personales de Iván (no se registraron).
- Ensayar la demo con persona y cronómetro antes de la Sprint Review (solo se ensayó de forma automática).
- Seguimiento: Camilo debe migrar `InventarioComprasApiTests` del PR #8 a `ClienteComoAsync` (D-012).
- Deuda para el Sprint 3: escáner de secretos en el CI (arc42 §10.4).

## Bloqueos abiertos
- (ninguno)

## Entradas

## 2026-11-18 23:30 · Retrospectiva HU-06 (I8.1)
**Horas reales contra lo estimado:**
- Estimado: HU-06 = **5 SP**, en 9 tareas (I0–I8).
- Horas personales de Iván: **no se registraron**. Iván dirigió la ejecución desde el chat y pidió cerrar la retrospectiva con los datos disponibles, sin dar horas.
- Lo medible: la sesión de trabajo con el agente duró **≈ 1 h 25 min** (22:04, clonación del repo → 23:30). Los commits de código fueron de 22:19 (I1) a 22:58 (I7), y la I5.9 a las 23:26. Hubo 16 commits de HU-06/HU-08 y 9 PR (#4–#7, #9–#11 y #14). La I5 fue la tarea más larga (≈ 15 min entre el commit de I4 y el de I5, más la corrección del bloqueo de las pruebas). La HU quedó dentro del día 1 del sprint, muy por debajo de lo que suponen 5 SP.
**Impacto de I5 en el equipo:**
- Desde `cce0aa1`, toda prueba de integración tiene que iniciar sesión y todo controlador nuevo necesita `[AutorizarModulo]`. Se comunicó con un aviso previo al grupo y con D-012.
- Felipe: sus 2 controladores y sus 2 clases de pruebas se migraron dentro de la I5, así que su F7 queda en solo revisarlo.
- Camilo: su PR #8 (abierto) tiene 4 pruebas que fallan con 401 al combinarse con la I5. Se detectó antes del merge y se le indicó cómo migrarlas (D-012).
**Qué salió bien:**
- TDD estricto en cada tarea (rojo → verde → commit); cobertura del dominio estable en ~97 %.
- Contratos C5/C6 implementados al pie de la letra; los mensajes, copiados literalmente del plan.
- Prueba real en el navegador de los 9 escenarios de I6.2 (14/14) y de la demo.
- Un PR por tarea con el CI en verde; el borrador del #9/#10 respetó el aviso de un día.
**Qué mejorar:**
- Los PR quedaron apilados (#4 → #5 → #6 → #7 → #9 → #10) porque nada se fusionaba entre tareas. Conviene que el integrador fusione el PR temprano de I1 antes de seguir, o abrir la cadena desde el principio como una sola historia.
- Las horas de los encabezados de esta bitácora se escribieron estimadas y quedaron adelantadas; se corrigieron con las horas de los commits. En adelante, tomar la hora del reloj al escribir cada entrada.
- Registrar las horas personales para la próxima retrospectiva.
- Falta un escáner de secretos en el CI (deuda del Sprint 3, arc42 §10.4).
**Impedimentos (y cómo se resolvieron):**
- El entorno del agente no tenía el SDK de .NET → se instaló el 8.0.428.
- Sin permiso de escritura al repo al principio → Iván instaló la app de GitHub; no hizo falta ningún token.
- La identidad de Git del entorno era la del asistente (AGENT.md §4.1 lo prohíbe) → se configuró la de Iván (`Ivanfelipe14`), tomada de sus commits anteriores.
- Las pruebas de integración se colgaban al iniciar sesión en el constructor (contexto de sincronización de xUnit) → `IAsyncLifetime`.
- Cruce con el #8 de Camilo → simulado en local y documentado en el #9 y en D-012.

## 2026-11-18 23:30 · Tarea I8 · Terminada
**Hecho:**
- I8.1: retrospectiva (entrada de arriba).
- I8.2: demo ensayada de forma automática (Chrome + Playwright contra la API real): login con 2FA → el admin asigna Cajero a Sebastián → Sebastián solo ve Caja / POS e Inventario de fichas → su token: `GET /api/nomina` 403. La máquina tardó 6 s; con persona, el guion (entrada de las 23:06) está pensado para ≈ 2 min. **No se ensayó con persona ni con cronómetro**: Iván no tuvo tiempo.
- I8.3: CER-46 (I8) y CER-16 (HU-06) a Listo en Jira.
**Evidencia:** en `develop` (`d88fc73` al momento de esta entrada, con `cce0aa1` y `2932d93` incluidos): `dotnet test CasinoErp.sln` → 314 aprobadas, 0 fallos; cobertura del dominio 97,22 %.
**Estado final de HU-06:** I0–I7 fusionadas; I8 cerrada en el PR #14 (D-012 y esta bitácora).
**Decisiones:** D-012 (seguridad activa). Las locales están en cada entrada.
**Pendientes:** ensayo humano de la demo; Camilo migra sus pruebas del #8; deuda del escáner de secretos.
**Bloqueos / necesito de otros:** ninguno para HU-06.
**Siguiente paso:** ninguno en Plan-Ivan; atender comentarios del integrador sobre el #14 si los hay.
**Commits/PR:** https://github.com/camilo1503/Casino-ERP-Repositorio/pull/14

## 2026-11-18 23:26 · Tarea I5.9 · Terminada
**Hecho:** Jorge fusionó #4 a #10 en `cce0aa1` y #11 en `2932d93` (2026-11-18 23:23). Se creó `desarrollo/decisiones/D-012-ivan-seguridad-activa.md` (Aceptada): desde `cce0aa1` toda la API exige token; qué hacer (pull/rebase, recrear la base, `ClienteComoAsync` con `IAsyncLifetime`, `[AutorizarModulo]` en los controladores nuevos, la clave de desarrollo ya está en `appsettings.Development.json`) y las consecuencias para Felipe (sus controladores y pruebas ya quedaron migrados en la I5) y Camilo (migrar las pruebas del #8).
**Evidencia:** en `develop` (`355c1e9`): `dotnet test CasinoErp.sln` → 314 aprobadas (260 + 54), 0 fallos; cobertura del dominio 97,22 %; CI del merge en verde; `grep -L "AutorizarModulo\|AllowAnonymous"` sobre los controladores no lista nada.
**Decisiones:** D-012 (afecta a todos).
**Pendientes:** ver *Pendientes abiertos*.
**Bloqueos / necesito de otros:** Camilo: migrar sus pruebas del #8.
**Siguiente paso:** retrospectiva con las horas de Iván y cierre de HU-06 en Jira.
**Commits/PR:** ver el PR de esta rama (`feature/HU-06-seguridad-activa`).

## 2026-11-18 23:06 · Tarea I8 · En curso
**Hecho:**
- Iván confirmó la regla de I3 (cambiar a Crupier con un documento vinculado inexistente → "Empleado no encontrado") y avisó al grupo de la activación de la seguridad. Los PR #9 y #10 pasaron de borrador a listos y se actualizaron sus descripciones.
- Se revisó el PR #8 de Camilo (HU-05): al combinarlo en local con #9/#10 no hay conflicto de texto en `InventarioController`, pero sus 4 pruebas de `InventarioComprasApiTests` fallan con 401 porque crean clientes sin token. Quedó anotado en el #9 quién las migra según el orden de los merges.
- **I8.2 · Ensayo de la demo** (automatizado, API en Development con la base recreada): (1) Iván entra con contraseña + TOTP y llega a Usuarios y roles; (2) le asigna Cajero a Sebastián → toast "Sebastián Prada ahora es cajero."; (3) Sebastián entra y solo ve Caja / POS e Inventario de fichas; (4) con el token de Sebastián, `GET /api/nomina` → **403** y `GET /api/caja/transacciones` → 200; (5) Swagger muestra el botón Authorize. Todo funcionó; la máquina tardó 6 s.
**Guion para la Sprint Review (≈ 2 min, cuentas ivan.leon y sebastian.prada, contraseña Casino2026, código 482913):**
1. (0:00–0:25) Login de ivan.leon con el código TOTP. Mencionar el bloqueo de 15 min tras 5 intentos.
2. (0:25–0:50) Usuarios y roles → Cambiar rol de Sebastián Prada → Cajero → Guardar. Mostrar la matriz de acceso por rol.
3. (0:50–1:20) Ventana privada: login de sebastian.prada → solo Caja / POS e Inventario de fichas.
4. (1:20–2:00) Swagger: `POST /api/auth/login` con Sebastián → copiar el token → Authorize → `GET /api/nomina` → 403. Cierre: "la barrera está en la API, no solo en el menú".
- Preparar antes: arrancar con `$env:BaseDatos__RecrearAlIniciar="true"` para tener los roles originales (Sebastián debe empezar como Anfitrión).
**Evidencia:** script de Playwright contra la API real (fuera del repo); las salidas están arriba.
**Decisiones:** ninguna nueva.
**Pendientes:** ensayo con persona y cronómetro (Iván); horas reales para la retrospectiva (I8.1); merge de la cadena #4 → #5 → #6 → #7 → #9 + #10; después, D-012 (I5.9), CER-43 y CER-16 a Listo (I8.3).
**Bloqueos / necesito de otros:** merges de Jorge.
**Siguiente paso:** en cuanto Jorge fusione #9, crear `D-012-ivan-seguridad-activa.md` con el hash del merge.
**Commits/PR:** —

## 2026-11-18 22:58 · Cierre de la sesión · En curso
**Hecho en la sesión:** I0 a I7 implementadas con TDD, cada una con su PR hacia `develop`: #4 (I0+I1), #5 (I2), #6 (I3), #7 (I4), #9 (I5, borrador), #10 (I6, borrador) y #11 (I7). Jira: CER-38 a CER-42, CER-44 y CER-45 en Listo; CER-43 (I5) sigue en curso hasta el merge.
**Evidencia:** en la última rama de la cadena (`feature/HU-06-front`), `dotnet test CasinoErp.sln` → 314 aprobadas (260 + 54), 0 fallos; cobertura del dominio 97,22 %. I6.2 en el navegador: 14/14. Todos los PR tienen el CI en verde al abrirse.
**Decisiones:** las de cada tarea (entradas de abajo). Ninguna afecta a otros módulos, así que no hubo archivos nuevos en `desarrollo/decisiones/`.
**Pendientes:** ver *Pendientes abiertos*.
**Bloqueos / necesito de otros:** Jorge debe fusionar en orden #4 → #5 → #6 → #7 (y #11 cuando quiera); #9 y #10 esperan el aviso de Iván.
**Siguiente paso:** cuando Jorge fusione #4–#7, hacer `pull --rebase` de #9 y #10; después del aviso, pasarlos a listos; tras el merge, I5.9 e I8.
**Commits/PR:** #4, #5, #6, #7, #9, #10, #11.

## 2026-11-18 22:58 · Tarea I7 · Terminada (en revisión)
**Hecho:** `docs/arc42/10-conceptos-seguridad.md` (10.1 a 10.5), `docs/arc42/05-bloques/seguridad.md` (frontera, endpoints, `classDiagram` y dos `sequenceDiagram`) y `docs/arc42/09-decisiones/ADR-002-jwt-totp.md`. Rama `feature/HU-06-arc42-seguridad`, tomada de `develop` (no depende de los PR de código).
**Evidencia:** `dotnet test CasinoErp.sln` en esa rama → 244 aprobadas, 0 fallos. Los 3 diagramas Mermaid se renderizan sin errores con mermaid-cli 11. La tabla controlador → roles se calculó a partir de la matriz C5.
**Decisiones:** esta entrada va en `feature/HU-06-front` y no en la rama de la I7, para que la bitácora no tenga conflictos entre ramas paralelas. Se corrigió el borrador: el CI no escanea secretos (solo marcas de IA); queda como deuda del Sprint 3.
**Pendientes:** que Jorge enlace la sección 10 y ADR-002 en el índice de arc42 si lo ve necesario (su archivo).
**Bloqueos / necesito de otros:** ninguno.
**Siguiente paso:** I8 después del merge de #9 y #10.
**Commits/PR:** `e792ae5` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/11

## 2026-11-18 22:55 · Tarea I6 · Terminada (en revisión, borrador)
**Hecho:** `src/CasinoErp.Api/wwwroot/js/api/seguridad.js` con los 9 puntos del plan: token en sessionStorage (con respaldo en memoria), los tres ganchos del núcleo, `entrar`, `loginView` con correo y datalist, `FORM.login` contra la API, `ACT.logout`, la carga `usuarios` (70), crear/rol/restablecer/desactivar/reactivar con `conectar`, y el modal de cambio de contraseña.
**Evidencia:**
- `dotnet test CasinoErp.sln` → 314 aprobadas (260 + 54), 0 fallos.
- I6.2 en el navegador: Chrome sin interfaz manejado con Playwright (scripts fuera del repo), contra la API en Development con la base recreada. 14/14 comprobaciones bien: (1) laura solo ve Caja / POS e Inventario de fichas; (2) "Código incorrecto. Intento 1 de 5." y al quinto "Código incorrecto. Cuenta bloqueada 15 minutos."; (3) carolina → "Esta cuenta está desactivada."; (4) Iván crea a Marta → modal con la contraseña temporal y otpauth://, y después de F5 sigue en la tabla; (5) restablecer levanta el bloqueo de laura; (6) cambiar el rol de Sebastián → "Tu sesión terminó. Inicia sesión de nuevo." en su siguiente petición, y al volver a entrar solo ve Caja e Inventario; (7) Marta cambia la temporal por Nueva#2026x → "Contraseña actualizada." y entra con la nueva; (8) cerrar sesión + F5 → login, sesión + F5 → se restaura; (9) sin API (file://) funciona el login local.
- En el paso 6, la "siguiente acción" de Sebastián se simuló con `PitBossApi.sincronizar()` (cualquier petición a la API produce el 401).
**Decisiones (solo afectan a mi módulo):**
- La carga `usuarios` deja `S.users` igual a la base (quita las filas locales que la API rechazó).
- `antesDeSincronizar` llama a `render()` cuando no hay sesión, para cambiar al login con correo apenas la API responde.
- El modal de cambio obligatorio tiene un botón "Después"; reaparece en cada ingreso mientras la contraseña sea temporal (el plan no dice si es bloqueante).
- PR #10 en borrador: contiene los commits de #9; si se fusionara antes, se saltaría el aviso al equipo.
**Pendientes:** el aviso al equipo y el merge conjunto de #9 y #10.
**Bloqueos / necesito de otros:** fusión de #4–#7.
**Siguiente paso:** I7 (arc42 sección 10, bloque Seguridad y ADR-002).
**Commits/PR:** `2c27f41` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/10

## 2026-11-18 22:51 · Tarea I5 · En curso (PR en borrador; falta I5.9 tras el merge)
**Hecho:**
- `Api/Seguridad/` (`OpcionesJwt`, `JwtEmisor` con `ValidarUsuarioVigente`, `AutorizarModuloAttribute` exacto a C5, `UsuarioDto` y records), `AuthController` y `UsuariosController`.
- Cambios mínimos del plan: paquete JwtBearer 8.0.11, `Program.cs`, `FiltroExcepciones.cs` (401), `appsettings*.json`, `[AutorizarModulo]` en los 9 controladores de `develop` (incluidos `EmpleadosController` y `NominaController` de Felipe: 2 `using` y 1 atributo cada uno), `CasinoErpFactory` (2 settings).
- `Utilidades/Autenticacion.cs` (C3). Pruebas existentes migradas con `IAsyncLifetime`, sin borrar ninguna: `ApiTests`/`BaseDatosTests` → gerente; `PersonalNominaApiTests`/`NominaApiTests` → felipe.cortes.
- `SeguridadApiTests`: las 15 pruebas de I5.1 y 3 extra (`Yo_devuelve...`, `Cambiar_contrasena_con_la_actual_errada...`, `Todos_los_controladores_exigen_token`).
**Evidencia:**
- TDD: `dotnet test tests/CasinoErp.PruebasIntegracion --filter SeguridadApiTests` → 17 fallos (sin endpoints de auth); tras implementar → verde.
- Primer intento colgado: el login síncrono (`GetAwaiter().GetResult()`) en los constructores se bloqueaba con el contexto de xUnit; se corrigió con `IAsyncLifetime`. Una prueba de I4 dependía del orden (contaba 13 usuarios); ahora cuenta solo las 11 cuentas de C6.
- `dotnet build CasinoErp.sln` → 0 advertencias. `dotnet test CasinoErp.sln` → 314 aprobadas (260 + 54), 0 fallos. Cobertura del dominio 97,22 %.
- I5.7 manual (API en Development, curl en vez de la UI de Swagger): login → token; `GET /api/usuarios` ivan.leon 200, laura.gomez 403, sin token 401; `GET /` 200; código 000000 → "Código incorrecto. Intento 1 de 5."; `swagger.json` con esquema Bearer.
**Decisiones (solo afectan a mi módulo):**
- `POST /api/auth/cambiar-contrasena` con la contraseña actual errada → **400**, no 401: el núcleo JS cierra la sesión ante cualquier 401. El dominio sigue lanzando `AutenticacionException`, como dice el plan.
- Las pruebas que desactivan o cambian contraseñas usan usuarios creados en la propia prueba, no laura.gomez (lo pide la nota de I5.1). El código errado se prueba con daniela.vargas.
- PR en borrador: el plan exige avisar con un día de anticipación y, según D-009, el integrador fusiona apenas el CI pasa.
**Pendientes:** aviso al equipo (Iván); I5.9 después del merge; mover CER-43 a Listo.
**Bloqueos / necesito de otros:** fusión de #4–#7. Camilo deberá anotar `ProveedoresController`/`ComprasController` cuando existan.
**Siguiente paso:** I6 (`seguridad.js`) en `feature/HU-06-front`, apilada sobre esta rama.
**Commits/PR:** `2321950` · https://github.com/camilo1503/Casino-ERP-Repositorio/pull/9

## 2026-11-18 22:35 · Tarea I4 · Terminada (en revisión)
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

## 2026-11-18 22:29 · Tarea I3 · Terminada (en revisión)
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

## 2026-11-18 22:24 · Tarea I2 · Terminada (en revisión)
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

## 2026-11-18 22:19 · Tarea I1 · Terminada (en revisión)
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
