# 5.x Bloque Seguridad: usuarios, roles y control de acceso (HU-06)

> arc42 §5 · Vista de bloques, nivel 2. Dueño: Iván. Historia: HU-06 *Asignación de roles* (5 SP). Conceptos: [§10](../10-conceptos-seguridad.md). Decisión: [ADR-002](../09-decisiones/ADR-002-jwt-totp.md). Contratos C5 y C6.

## 1. Propósito y frontera

El bloque autentica al personal (contraseña y TOTP), emite la sesión (JWT) y decide qué módulos puede usar cada rol. También administra las cuentas: crearlas, asignar o cambiar el rol, restablecer la contraseña, desactivarlas y reactivarlas. Los demás bloques solo dependen de él a través de `Modulos` y `[AutorizarModulo]` (contrato C5).

| Capa | Carpeta / archivo | Contenido |
|---|---|---|
| Dominio | `src/CasinoErp.Dominio/Seguridad/` | `Rol`, `Modulos`, `MatrizAcceso` (C5), `Usuario`, `IUsuarioRepositorio`, `IHasherContrasena`, `IVerificadorTotp`, `ContrasenaTemporal`, `AutenticacionService`, `UsuarioService` |
| Dominio | `Comun/Excepciones.cs` | `AutenticacionException` (→ 401) |
| Infraestructura | `Seguridad/HasherContrasena.cs`, `Seguridad/VerificadorTotp.cs` | Adaptadores de ASP.NET Identity (PBKDF2) y Otp.NET |
| Infraestructura | `Configuraciones/UsuarioConfiguracion.cs`, `Repositorios/SeguridadRepositorios.cs` | Tabla `Usuarios` (correo único) y `UsuarioRepositorio` |
| Infraestructura | `Modulos/ModuloSeguridad.cs`, `Semillas/SemillaSeguridad.cs` (orden 40) | Registro en DI y las 11 cuentas de demo (C6) |
| Api | `Seguridad/OpcionesJwt.cs`, `JwtEmisor.cs`, `AutorizarModuloAttribute.cs`, `UsuarioDto.cs` | Sesión JWT, revocación y autorización por módulo |
| Api | `Controllers/AuthController.cs`, `Controllers/UsuariosController.cs` | Endpoints REST |
| Front | `wwwroot/js/api/seguridad.js` | Login con 2FA, sesión y pantalla **Usuarios y roles** |

### Lo que expone

| Verbo y ruta | Autorización | Respuesta |
|---|---|---|
| `POST /api/auth/login` | Anónimo | 200 `{ token, expira, usuario, modulos }`; 401 `{ mensaje }` |
| `GET /api/auth/yo` | Con sesión | `{ usuario, modulos }` |
| `POST /api/auth/cambiar-contrasena` | Con sesión | "Contraseña actualizada"; 400 si la actual no coincide o la nueva no es segura |
| `GET /api/usuarios` | Módulo `usuarios` | `UsuarioDto[]` |
| `POST /api/usuarios` | Módulo `usuarios` | 201 "Usuario creado" con `{ usuario, contrasenaTemporal, uriTotp }` |
| `PUT /api/usuarios/{id}/rol` | Módulo `usuarios` | "Rol actualizado" |
| `POST /api/usuarios/{id}/restablecer` | Módulo `usuarios` | "Contraseña restablecida" con `{ usuario, contrasenaTemporal }` |
| `POST /api/usuarios/{id}/desactivar` · `/reactivar` | Módulo `usuarios` | "Usuario desactivado" · "Usuario reactivado" |

### De qué depende

- `IEmpleadoRepositorio` (contrato C4, bloque Personal): solo para validar que un usuario Crupier esté vinculado a un empleado con cargo Crupier.
- `TimeProvider` (bloqueos, último ingreso y vigencia del token).
- Configuración: `Jwt:*` y, opcionalmente, `Seguridad:CodigoTotpDemo`.

## 2. Diagrama de clases

```mermaid
classDiagram
    direction LR
    class AuthController {
        +Login(LoginRequest) LoginResponse
        +Yo() object
        +CambiarContrasena(CambiarContrasenaRequest) IActionResult
    }
    class UsuariosController {
        +Listar() UsuarioDto[]
        +Crear(CrearUsuarioRequest) IActionResult
        +CambiarRol(int id, CambiarRolRequest)
        +Restablecer(int id)
        +Desactivar(int id)
        +Reactivar(int id)
    }
    class AutorizarModuloAttribute {
        +AutorizarModuloAttribute(params string[] modulos)
        +string Roles
    }
    class JwtEmisor {
        +Emitir(Usuario) TokenYExpira
        +ValidarUsuarioVigente(TokenValidatedContext)$
    }
    class AutenticacionService {
        +MaximoIntentos = 5
        +DuracionBloqueo = 15 min
        +IniciarSesionAsync(correo, contrasena, codigo) Task~Usuario~
    }
    class UsuarioService {
        +CrearAsync(nombre, correo, Rol, empleadoDocumento) Task~UsuarioCreado~
        +CambiarRolAsync(int id, Rol) Task~Usuario~
        +RestablecerContrasenaAsync(int id) Task~ContrasenaRestablecida~
        +DesactivarAsync(int id) Task~Usuario~
        +ReactivarAsync(int id) Task~Usuario~
        +CambiarContrasenaAsync(int id, actual, nueva) Task
    }
    class MatrizAcceso {
        <<static>>
        +ModulosDe(Rol) IReadOnlySet~string~
        +PuedeAcceder(Rol, string) bool
        +RolesCon(params string[]) IReadOnlyList~Rol~
    }
    class Usuario {
        +int Id
        +string Correo
        +Rol Rol
        +bool Activo
        +string HashContrasena
        +string SecretoTotp
        +int IntentosFallidos
        +DateTime? BloqueadoHasta
        +bool DebeCambiarContrasena
        +string? EmpleadoDocumento
    }
    class IUsuarioRepositorio {
        <<interface>>
    }
    class IHasherContrasena {
        <<interface>>
        +Hash(string) string
        +Verificar(string hash, string) bool
    }
    class IVerificadorTotp {
        <<interface>>
        +NuevoSecreto() string
        +Verificar(string secreto, string codigo) bool
        +UriConfiguracion(string secreto, string correo) string
    }
    class HasherContrasena
    class VerificadorTotp

    AuthController --> AutenticacionService
    AuthController --> UsuarioService
    AuthController --> JwtEmisor
    UsuariosController --> UsuarioService
    UsuariosController ..> AutorizarModuloAttribute : anotado con
    AutorizarModuloAttribute --> MatrizAcceso
    JwtEmisor --> IUsuarioRepositorio : revocación
    AutenticacionService --> IUsuarioRepositorio
    AutenticacionService --> IHasherContrasena
    AutenticacionService --> IVerificadorTotp
    UsuarioService --> IUsuarioRepositorio
    UsuarioService --> IHasherContrasena
    UsuarioService --> IVerificadorTotp
    IUsuarioRepositorio --> Usuario
    HasherContrasena ..|> IHasherContrasena
    VerificadorTotp ..|> IVerificadorTotp
```

## 3. Login con 2FA

```mermaid
sequenceDiagram
    autonumber
    actor P as Persona (navegador)
    participant F as seguridad.js
    participant A as AuthController
    participant S as AutenticacionService
    participant R as IUsuarioRepositorio
    participant H as IHasherContrasena
    participant T as IVerificadorTotp
    participant J as JwtEmisor

    P->>F: correo, contraseña, código TOTP
    F->>A: POST /api/auth/login
    A->>S: IniciarSesionAsync(correo, contraseña, código)
    S->>R: ObtenerPorCorreoAsync(correo en minúsculas)
    alt cuenta bloqueada o desactivada
        S-->>A: AutenticacionException
        A-->>F: 401 { mensaje }
    else contraseña o código errado
        S->>R: IntentosFallidos++ (al 5.º: BloqueadoHasta = ahora + 15 min)
        S-->>A: AutenticacionException ("Código incorrecto. Intento n de 5.")
        A-->>F: 401 { mensaje }
    else datos correctos
        S->>H: Verificar(hash, contraseña) ✓
        S->>T: Verificar(secreto, código) ✓
        S->>R: IntentosFallidos = 0, UltimoIngreso = ahora
        A->>J: Emitir(usuario)
        J-->>A: token HS256 (8 h)
        A-->>F: 200 { token, expira, usuario, modulos }
        F->>F: sessionStorage["pitboss.token"] = token
        F-->>P: menú con los módulos del rol
    end
```

## 4. Petición autorizada o denegada

```mermaid
sequenceDiagram
    autonumber
    participant F as Front (Bearer)
    participant JB as JwtBearer
    participant V as JwtEmisor.ValidarUsuarioVigente
    participant R as IUsuarioRepositorio
    participant AZ as Autorización ([AutorizarModulo])
    participant C as Controlador

    F->>JB: GET /api/nomina · Authorization: Bearer …
    JB->>JB: firma, emisor, audiencia y vigencia
    alt token inválido o vencido
        JB-->>F: 401
    else token válido
        JB->>V: OnTokenValidated
        V->>R: ObtenerAsync(sub)
        alt usuario inexistente, desactivado o con otro rol
            V-->>F: 401 (Sesión no vigente) → el front vuelve al login
        else vigente
            JB->>AZ: rol del token ∈ MatrizAcceso.RolesCon("rrhh")?
            alt el rol no tiene el módulo (p. ej. Cajero)
                AZ-->>F: 403 → "No tienes acceso a este módulo."
            else Gerente o Talento humano
                AZ->>C: NominaController
                C-->>F: 200
            end
        end
    end
```

## 5. Pruebas que cubren el bloque

| Nivel | Archivo | Qué verifica |
|---|---|---|
| Unitaria | `MatrizAccesoTests` | La matriz del mockup, `PuedeAcceder` y `RolesCon` |
| Unitaria | `AutenticacionServiceTests` | Login, mensajes, bloqueo y vencimiento, contraseñas temporales seguras |
| Unitaria | `UsuarioServiceTests` | Alta, validaciones, crupier vinculado, último administrador, restablecer, cambiar la contraseña |
| Integración | `SeguridadApiTests` | Semilla de 11 cuentas, TOTP real, 401/403 por rol, revocación por desactivación o cambio de rol, sin secretos en las respuestas |
| Manual (I6.2) | `js/api/seguridad.js` | Los 9 escenarios del plan en el navegador |
