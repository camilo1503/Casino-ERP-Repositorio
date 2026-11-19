# 10. Conceptos de seguridad y roles (HU-06)

> arc42 §10 · Dueño: Iván. Historia: HU-06 *Asignación de roles* (5 SP). Bloque: [05-bloques/seguridad.md](05-bloques/seguridad.md). Decisión: [ADR-002](09-decisiones/ADR-002-jwt-totp.md). Contratos: C5 (matriz y atributo) y C6 (cuentas de demo) en `desarrollo/contratos.md`.

La seguridad del ERP tiene tres capas: **autenticación** (quién eres: contraseña y segundo factor), **sesión** (un token JWT por turno) y **autorización** (qué módulos puede usar tu rol). Las tres se aplican en la API; el front solo oculta lo que el rol no puede ver.

## 10.1 Autenticación

| Control | Cómo se implementa | Dónde |
|---|---|---|
| Contraseña con hash | PBKDF2 de ASP.NET Core Identity (`PasswordHasher<T>`, formato v3 con sal aleatoria e iteraciones dentro del propio hash). Nunca se guarda ni se devuelve la contraseña. | `Infraestructura/Seguridad/HasherContrasena.cs` (`IHasherContrasena`) |
| Segundo factor TOTP | RFC 6238 de 6 dígitos y 30 s con **Otp.NET**. Se acepta una ventana de ±1 paso (`VerificationWindow.RfcSpecifiedNetworkDelay`) para tolerar el desfase del reloj del teléfono. Cada cuenta tiene un secreto Base32 de 20 bytes. | `Infraestructura/Seguridad/VerificadorTotp.cs` (`IVerificadorTotp`) |
| Alta en la app autenticadora | Al crear la cuenta se devuelve `otpauth://totp/PitBoss:{correo}?secret=…&issuer=PitBoss` una sola vez, para registrarla en Google Authenticator, Microsoft Authenticator o similar. | `UsuarioService.CrearAsync` |
| Bloqueo por intentos | 5 fallos seguidos (de contraseña o de código) bloquean la cuenta **15 minutos**. El bloqueo vence solo o lo levanta un administrador al restablecer la contraseña. Un ingreso correcto reinicia el contador. | `AutenticacionService.IniciarSesionAsync` |
| Mensajes que no revelan cuentas | Un correo inexistente y una contraseña errada dan el mismo mensaje: `Correo o contraseña incorrectos.` | `AutenticacionService` |
| Contraseñas temporales | 12 caracteres generados con `RandomNumberGenerator` (al menos una mayúscula, una minúscula, un dígito y un carácter de `!@#$%&*?`). La cuenta queda con `DebeCambiarContrasena = true` y el front pide el cambio en el primer ingreso. | `ContrasenaTemporal.Generar`, `UsuarioService.RestablecerContrasenaAsync` |
| Fortaleza de contraseñas | Al menos 8 caracteres, una mayúscula, un número y un carácter especial (las `pwRules` del mockup). | `ContrasenaTemporal.EsSegura`, `UsuarioService.CambiarContrasenaAsync` |
| Código de demo | `482913` se acepta **solo** si `Seguridad:CodigoTotpDemo` está configurado: en `appsettings.Development.json`, en las pruebas de integración y, durante la Sprint Review, en el app setting `Seguridad__CodigoTotpDemo` de Azure (Terraform, `var.codigo_totp_demo`; vacío por defecto). | `VerificadorTotp.Verificar` |

Las fallas de autenticación lanzan `AutenticacionException`, que `FiltroExcepciones` convierte en **401** con `{ "mensaje": "..." }`.

## 10.2 Sesión

- **Token:** JWT firmado con **HMAC-SHA256** (HS256), vigente **8 horas** (un turno del casino). Lo emite `JwtEmisor` al hacer `POST /api/auth/login`.
- **Claims:** `sub` = Id del usuario, `name` = nombre, `email` = correo y `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` (`ClaimTypes.Role`) = el rol (`Cajero`, `Gerente`…). `iss` = `CasinoErp` y `aud` = `CasinoErp.Front`.
- **Validación:** emisor, audiencia, firma y vigencia, con 1 minuto de tolerancia de reloj (`ClockSkew`). `MapInboundClaims = false`, así los claims llegan con el mismo nombre con el que se emitieron.
- **Clave:** `Jwt:Clave` (al menos 32 caracteres; la API no arranca sin ella gracias a `ValidateOnStart`). No está en `appsettings.json`. En local sale de `appsettings.Development.json`; en Azure, del app setting `Jwt__Clave` que Terraform genera con `random_password` (`infra/terraform/main.tf`) y guarda solo en el state remoto.
- **Revocación efectiva:** en cada petición, `JwtBearerEvents.OnTokenValidated` (`JwtEmisor.ValidarUsuarioVigente`) consulta el usuario en la base. Si ya no existe, está **desactivado** o su **rol cambió**, el token se rechaza con 401 aunque no haya vencido. Así se cumple "un token de un usuario desactivado o con otro rol deja de servir" sin lista negra de tokens.
- **En el front:** el token se guarda en `sessionStorage` (se pierde al cerrar la pestaña) y viaja en `Authorization: Bearer`. Al recargar, `GET /api/auth/yo` restaura la sesión. Cualquier 401 (salvo en el propio login) cierra la sesión y muestra `Tu sesión terminó. Inicia sesión de nuevo.` (`js/api/seguridad.js`).

## 10.3 Autorización

`[AutorizarModulo(params string[] modulos)]` hereda de `AuthorizeAttribute` y traduce los módulos a roles con `MatrizAcceso.RolesCon(modulos)`: basta con que el rol tenga **uno** de los módulos. Sin token (o con un token no vigente) la respuesta es **401**; con un token de un rol sin el módulo, **403**.

### Matriz rol × módulo

Es la constante `ROLES` del mockup, sin `campana` (vive dentro de promociones), más `proveedores` para Gerente y Jefe de piso. Código: `Dominio/Seguridad/Rol.cs` (`MatrizAcceso`).

| Rol | inicio | perfiles | promociones | retencion | catalogo | mimesa | mesas | caja | inventario | competencias | rrhh | cumplimiento | reportes | usuarios | proveedores |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Gerente | ● | ● | ● | ● | ● | | ● | ● | ● | ● | ● | ● | ● | | ● |
| Administrador | | | | | | | | | | | | | ● | ● | |
| Anfitrión | ● | ● | | ● | ● | | | | | | | | | | |
| Jefe de piso | ● | | | | ● | | ● | | ● | ● | | | | | ● |
| Crupier | | | | | | ● | | | | | | | | | |
| Marketing | ● | ● | ● | ● | ● | | | | | | | | | | |
| Cajero | | | | | | | | ● | ● | | | | | | |
| Talento humano | ● | | | | | | | | | ● | ● | | | | |
| Cumplimiento | ● | | | | | | | ● | | ● | | ● | ● | | |

### Módulos que exige cada controlador

| Controlador | `[AutorizarModulo(...)]` | Roles que entran |
|---|---|---|
| `JugadoresController` | perfiles, retencion, caja | Gerente, Anfitrión, Marketing, Cajero, Cumplimiento |
| `MesasController` | catalogo, mesas, mimesa | Gerente, Anfitrión, Jefe de piso, Crupier, Marketing |
| `CajaController` | caja, cumplimiento | Gerente, Cajero, Cumplimiento |
| `RetencionController` | retencion, caja, mimesa | Gerente, Anfitrión, Crupier, Marketing, Cajero, Cumplimiento |
| `RellenosController` | mesas, inventario | Gerente, Jefe de piso, Cajero |
| `InventarioController` | inventario, caja | Gerente, Jefe de piso, Cajero, Cumplimiento |
| `ResumenController` | inicio | Gerente, Anfitrión, Jefe de piso, Marketing, Talento humano, Cumplimiento |
| `EmpleadosController` | competencias, rrhh, mesas | Gerente, Jefe de piso, Talento humano, Cumplimiento |
| `NominaController` | rrhh | Gerente, Talento humano |
| `ProveedoresController` (Camilo) | proveedores | Gerente, Jefe de piso |
| `ComprasController` (Camilo) | proveedores, inventario | Gerente, Jefe de piso, Cajero |
| `UsuariosController` | usuarios | Administrador |
| `AuthController` | `login` anónimo; `yo` y `cambiar-contrasena` con `[Authorize]` | Cualquiera con sesión |

`GET /api/salud` y el front estático (`/`, `index.html`, `js/`) son anónimos. Una prueba de integración (`Todos_los_controladores_exigen_token`) comprueba que las rutas de la API responden 401 sin token.

### El front no es una barrera de seguridad

El menú del mockup solo muestra los módulos de `ROLES[rol].views`, y `PitBossApi.pedirSi` no llama a la API si el rol no tiene el módulo. Esto es **solo de interfaz**: la barrera real es `[AutorizarModulo]` en la API. Un usuario que llame a la API a mano recibe 403.

## 10.4 Protección de datos

- **`UsuarioDto`** es lo único que la API devuelve de una cuenta: Id, nombre, correo, rol, activo, bloqueado, empleado vinculado, último ingreso y si debe cambiar la contraseña. **Nunca** incluye `HashContrasena` ni `SecretoTotp` (lo verifican las pruebas `El_admin_crea_usuario_y_la_respuesta_no_expone_secretos` y `El_listado_de_usuarios_no_expone_secretos`).
- La **contraseña temporal** y la **URI TOTP** se muestran una sola vez, en la respuesta de `POST /api/usuarios` (o la contraseña, en `POST /api/usuarios/{id}/restablecer`). El front las presenta en un modal con el aviso de entregarlas una sola vez.
- Restablecer la contraseña **no** cambia el secreto TOTP: la persona sigue usando la misma entrada en su app autenticadora.
- No hay secretos en el repositorio: la clave JWT de desarrollo solo sirve en local y la de Azure la genera Terraform. Las bases SQLite (`*.db`), el state (`*.tfstate`) y `backend.hcl` están en `.gitignore`, y AGENT.md §9 prohíbe subir secretos. El CI todavía no escanea secretos; agregar un escáner (por ejemplo, gitleaks) queda como deuda para el Sprint 3.
- Reglas de negocio de las cuentas: no se puede quitar el rol ni desactivar al **último administrador activo**; un usuario Crupier debe estar vinculado a un empleado con cargo Crupier.

## 10.5 Coherencia con la sección 5 (control → bloque → clase)

| Control de seguridad | Bloque (§5) | Clase o archivo donde se implementa |
|---|---|---|
| Hash PBKDF2 de contraseñas | Seguridad | `HasherContrasena` (`IHasherContrasena`) |
| Segundo factor TOTP | Seguridad | `VerificadorTotp` (`IVerificadorTotp`), `AutenticacionService` |
| Bloqueo de 15 min tras 5 intentos | Seguridad | `AutenticacionService.IniciarSesionAsync` |
| Emisión del JWT | Seguridad (Api) | `JwtEmisor.Emitir`, `AuthController.Login` |
| Revocación por usuario inactivo o rol cambiado | Seguridad (Api) | `JwtEmisor.ValidarUsuarioVigente` (`OnTokenValidated` en `Program.cs`) |
| Matriz rol × módulo | Seguridad | `MatrizAcceso` (`Dominio/Seguridad/Rol.cs`) |
| `[AutorizarModulo(Modulos.Usuarios)]` → solo el administrador gestiona cuentas | Seguridad | `UsuariosController` |
| `[AutorizarModulo(Modulos.Rrhh)]` → solo Gerente y Talento humano liquidan la nómina | Nómina | `NominaController` |
| `[AutorizarModulo(Modulos.Competencias, Modulos.Rrhh, Modulos.Mesas)]` | Personal | `EmpleadosController` |
| `[AutorizarModulo(Modulos.Caja, Modulos.Cumplimiento)]` | Caja (Sprint 1) | `CajaController` |
| `[AutorizarModulo(Modulos.Proveedores)]` y `(Modulos.Proveedores, Modulos.Inventario)` | Inventario y Compras | `ProveedoresController`, `ComprasController` (Camilo) |
| Último administrador protegido | Seguridad | `UsuarioService.CambiarRolAsync`, `UsuarioService.DesactivarAsync` |
| Sin hash ni secreto en las respuestas | Seguridad (Api) | `UsuarioDto.De` |
| 401 con mensaje para el usuario | Api (transversal) | `FiltroExcepciones` (`AutenticacionException` → 401) |
| Clave JWT fuera del repo | Despliegue (§7) | `infra/terraform/main.tf` (`Jwt__Clave`), `OpcionesJwt` con `ValidateOnStart` |
| Sesión en el navegador y cierre ante 401 | Front | `wwwroot/js/api/seguridad.js` |

Esta tabla es la base del video de la Tarea 2 de la guía: cada fila se puede mostrar en el código y en una prueba.
