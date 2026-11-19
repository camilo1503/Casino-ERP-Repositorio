# Plan Iván — Usuarios, roles y control de acceso (HU-06)

> **Para agentes:** antes de empezar, lee `AGENT.md`, `desarrollo/contratos.md` y tu bitácora `desarrollo/bitacoras/ivan.md`. Ejecuta las tareas en orden, con TDD estricto, y reporta según AGENT.md §8. Los pasos usan casillas (`- [ ]`).

**Objetivo:** reemplazar el login simulado del mockup por **autenticación real**: contraseña con hash, segundo factor TOTP de 6 dígitos y bloqueo tras 5 intentos durante 15 minutos. La sesión se maneja con un token JWT. La **autorización por módulo** se aplica en la API según el rol, siguiendo la matriz del mockup, y el administrador puede **asignar y cambiar roles** desde la pantalla "Usuarios y roles".

**HU de la guía:** *HU-06 Asignación de Roles (5 SP).* "Como administrador del sistema, quiero poder asignar roles de usuario (ej. 'Admin', 'Compras', 'RR. HH.') para controlar el acceso a funcionalidades y módulos específicos."

**Demo que pide la guía:** *"Se demuestra la asignación de un rol a un usuario y se verifica que este solo pueda acceder a los módulos permitidos."*

**Mockup de referencia** (`src/CasinoErp.Api/wwwroot/index.html`):
- `const ROLES` (9 roles con sus `views` y `home`) y `const CAN`.
- `loginView()` y `FORM.login`: correo, contraseña de mínimo 8 caracteres, código TOTP de 6 dígitos (demo `482913`) y bloqueo de 15 minutos tras 5 códigos errados. Mensajes: `Cuenta bloqueada 15 minutos por 5 intentos fallidos. Un administrador puede restablecerla.`, `Esta cuenta está desactivada.`, `Código incorrecto. Intento {n} de 5.`, `Código incorrecto. Cuenta bloqueada 15 minutos.`
- `V.usuarios` (épica E-06): tabla de cuentas, acciones (Cambiar rol, Restablecer contraseña, Desactivar, Reactivar) y la matriz de acceso por rol.
- `MODALS.userNew` y `FORM.userNew` (HU-ADM-01): `Escribe el nombre.`, `El correo no es válido.`, `Ya existe una cuenta con ese correo.`, `Vincula el usuario de crupier a un empleado para asignarle su mesa.`, `El empleado vinculado no es crupier.`
- `FORM.userRole`: `No puedes quitar el rol al último administrador.`
- `ACT.resetPass`, `ACT.userOff`, `ACT.userOn` y `seed().users` (las 11 cuentas).

**Archivos que son tuyos:**
- `src/CasinoErp.Dominio/Seguridad/**`
- `src/CasinoErp.Dominio/Comun/Excepciones.cs` (solo se agrega `AutenticacionException`)
- En `src/CasinoErp.Infraestructura/`: `Seguridad/HasherContrasena.cs`, `Seguridad/VerificadorTotp.cs`, `Configuraciones/UsuarioConfiguracion.cs`, `Repositorios/SeguridadRepositorios.cs`, `Modulos/ModuloSeguridad.cs`, `Semillas/SemillaSeguridad.cs`; y en `CasinoErp.Infraestructura.csproj`, solo los dos paquetes
- `src/CasinoErp.Api/Seguridad/**`, `Controllers/AuthController.cs`, `Controllers/UsuariosController.cs`, `CasinoErp.Api.csproj` (solo el paquete JwtBearer)
- En la tarea I5, cambios mínimos en: `Program.cs`, `FiltroExcepciones.cs`, `appsettings.json`, `appsettings.Development.json`, el atributo `[AutorizarModulo]` en los controladores que existan en `develop`, `tests/CasinoErp.PruebasIntegracion/Utilidades/CasinoErpFactory.cs` (dos settings) y la creación de clientes en las pruebas de integración que existan en `develop`
- `src/CasinoErp.Api/wwwroot/js/api/seguridad.js`
- En `tests/CasinoErp.PruebasUnitarias/`: `Dobles/SeguridadEnMemoria.cs`, `MatrizAccesoTests.cs`, `UsuarioServiceTests.cs`, `AutenticacionServiceTests.cs`
- En `tests/CasinoErp.PruebasIntegracion/`: `Utilidades/Autenticacion.cs`, `SeguridadApiTests.cs`
- `docs/arc42/10-conceptos-seguridad.md`, `docs/arc42/05-bloques/seguridad.md`, `docs/arc42/09-decisiones/ADR-002-jwt-totp.md`

**Contratos:** eres dueño de **C5** (seguridad) y **C6** (cuentas de demo). Consumes **C1**, **C2** y **C3** (Fase 0 de Jorge) y **C4** (`Empleado` e `IEmpleadoRepositorio` de Felipe, en I3).

**Tu PR de la tarea I5 cambia la forma de trabajar de todos:** desde que llega a `develop`, toda la API exige token. Antes de fusionarlo, avisa con un día de anticipación. Después del merge, publica la decisión de aviso (I5.9).

---

## Orden y dependencias

| Tarea | Necesita en `develop` | Nota |
|---|---|---|
| I0 Arranque | — | |
| I1 Roles, módulos y matriz (C5, parte 1) | — | Día 1; PR temprano |
| I2 Usuario y autenticación (dominio) | — | Día 1–2 |
| I3 Gestión de usuarios (dominio) | **C4 de Felipe (F1)** | Mientras tanto, avanza en I2 |
| I4 Infraestructura y cuentas de demo (C6) | Fase 0 (J0) | |
| I5 JWT y autorización en la API (C5, parte 2) | I1–I4 | **Hito "Seguridad activa"** |
| I6 Front (`seguridad.js`) | I5, Fase 0 (C2 JS) | |
| I7 arc42 sección 10 | I5 | |
| I8 Cierre y demo | I6 | |

---

### Tarea I0 · Arranque

- [ ] **I0.1** `git switch develop; git pull --rebase origin develop; git switch -c feature/HU-06-matriz-acceso`
- [ ] **I0.2** Ejecutar `dotnet test CasinoErp.sln` y anotar el resultado en la bitácora. Mover HU-06 a "En curso" en Jira. Copiar la tabla de tareas I0–I8 en tu bitácora.

---

### Tarea I1 · `Rol`, `Modulos` y `MatrizAcceso` (contrato C5, parte 1)

**Archivos:** `src/CasinoErp.Dominio/Seguridad/Rol.cs` (con `Rol`, `Modulos` y `MatrizAcceso`); `src/CasinoErp.Dominio/Comun/Excepciones.cs` (agregar `public class AutenticacionException(string mensaje) : Exception(mensaje);`); pruebas en `tests/CasinoErp.PruebasUnitarias/MatrizAccesoTests.cs`.

**Interfaces que produce:** exactamente las de `desarrollo/contratos.md` §C5. La matriz es la tabla de C5: un `Dictionary<Rol, HashSet<string>>` estático con `StringComparer.Ordinal`. `RolesCon(modulos)` devuelve los roles con al menos uno de esos módulos, en el orden del enum.

- [ ] **I1.1 Escribir las pruebas que fallan**

```csharp
[Fact] public void El_crupier_solo_ve_Mi_mesa() =>
    MatrizAcceso.ModulosDe(Rol.Crupier).Should().BeEquivalentTo(["mimesa"]);
[Fact] public void El_cajero_ve_caja_e_inventario() =>
    MatrizAcceso.ModulosDe(Rol.Cajero).Should().BeEquivalentTo(["caja", "inventario"]);
[Fact] public void El_administrador_ve_usuarios_y_reportes() =>
    MatrizAcceso.ModulosDe(Rol.Administrador).Should().BeEquivalentTo(["usuarios", "reportes"]);
[Fact] public void El_gerente_ve_13_modulos_y_no_administra_usuarios()
{
    MatrizAcceso.ModulosDe(Rol.Gerente).Should().HaveCount(13).And.Contain("proveedores").And.NotContain("usuarios").And.NotContain("mimesa");
}
[Fact] public void Talento_humano_ve_inicio_competencias_y_rrhh() =>
    MatrizAcceso.ModulosDe(Rol.TalentoHumano).Should().BeEquivalentTo(["inicio", "competencias", "rrhh"]);
[Theory]
[InlineData(Rol.Anfitrion, new[] { "inicio", "perfiles", "retencion", "catalogo" })]
[InlineData(Rol.JefePiso, new[] { "inicio", "catalogo", "mesas", "inventario", "competencias", "proveedores" })]
[InlineData(Rol.Marketing, new[] { "inicio", "perfiles", "promociones", "retencion", "catalogo" })]
[InlineData(Rol.Cumplimiento, new[] { "inicio", "cumplimiento", "reportes", "competencias", "caja" })]
public void La_matriz_es_la_del_mockup(Rol rol, string[] modulos) => MatrizAcceso.ModulosDe(rol).Should().BeEquivalentTo(modulos);
[Fact] public void PuedeAcceder_responde_segun_la_matriz()          // (Cajero,"caja") true; (Cajero,"rrhh") false
[Fact] public void RolesCon_devuelve_quien_tiene_algun_modulo() =>
    MatrizAcceso.RolesCon("rrhh").Should().Equal(Rol.Gerente, Rol.TalentoHumano);
[Fact] public void RolesCon_varios_modulos_une_los_roles() =>
    MatrizAcceso.RolesCon("usuarios", "mimesa").Should().Equal(Rol.Administrador, Rol.Crupier);
[Fact] public void Todos_los_roles_tienen_al_menos_un_modulo()      // Enum.GetValues<Rol>() -> ModulosDe(r) no vacío
```

- [ ] **I1.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter MatrizAccesoTests`. Esperado: falla.
- [ ] **I1.3** Implementar.
- [ ] **I1.4** Ejecutar el mismo comando. Esperado: verde.
- [ ] **I1.5** Commit, push y PR temprano: `HU-06: Roles, modulos y matriz de acceso (contrato C5 parte 1)`.

---

### Tarea I2 · `Usuario` y `AutenticacionService` (dominio)

**Rama:** `feature/HU-06-autenticacion`.

**Archivos:** `src/CasinoErp.Dominio/Seguridad/Usuario.cs`, `ContrasenaTemporal.cs`, `AutenticacionService.cs`; pruebas en `Dobles/SeguridadEnMemoria.cs` (`UsuariosEnMemoria`, `HasherFalso`, `TotpFalso`) y `AutenticacionServiceTests.cs`.

**Interfaces que produce:**

```csharp
namespace CasinoErp.Dominio.Seguridad;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";              // minúsculas
    public Rol Rol { get; set; }
    public bool Activo { get; set; } = true;
    public string HashContrasena { get; set; } = "";
    public string SecretoTotp { get; set; } = "";
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public bool DebeCambiarContrasena { get; set; }
    public string? EmpleadoDocumento { get; set; }
    public DateTime? UltimoIngreso { get; set; }
}
public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerAsync(int id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);   // compara en minúsculas
    Task<bool> ExisteCorreoAsync(string correo);
    Task<IReadOnlyList<Usuario>> ListarAsync();             // por Id
    Task AgregarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
public interface IHasherContrasena { string Hash(string contrasena); bool Verificar(string hash, string contrasena); }
public interface IVerificadorTotp
{
    string NuevoSecreto();                                  // Base32
    bool Verificar(string secreto, string codigo);
    string UriConfiguracion(string secreto, string correo); // otpauth://totp/PitBoss:{correo}?secret={secreto}&issuer=PitBoss
}
public static class ContrasenaTemporal
{
    public static string Generar();                         // 12 caracteres: ≥1 mayúscula, ≥1 minúscula, ≥1 dígito, ≥1 de "!@#$%&*?"; RandomNumberGenerator
    public static bool EsSegura(string contrasena);         // ≥8 caracteres, ≥1 mayúscula, ≥1 dígito, ≥1 no alfanumérico (pwRules del mockup)
}
public class AutenticacionService(IUsuarioRepositorio usuarios, IHasherContrasena hasher, IVerificadorTotp totp, TimeProvider reloj)
{
    public const int MaximoIntentos = 5;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);
    public Task<Usuario> IniciarSesionAsync(string correo, string contrasena, string codigo);
}
```

**Algoritmo de `IniciarSesionAsync`.** Todas las fallas lanzan `AutenticacionException`; `ahora = reloj.GetUtcNow().UtcDateTime`.

```
u = ObtenerPorCorreoAsync(correo.Trim().ToLowerInvariant())
si u es null                                   -> "Correo o contraseña incorrectos."
si u.BloqueadoHasta > ahora                    -> "Cuenta bloqueada 15 minutos por 5 intentos fallidos. Un administrador puede restablecerla."
si u.BloqueadoHasta <= ahora (venció)          -> u.BloqueadoHasta = null; u.IntentosFallidos = 0
si !u.Activo                                   -> "Esta cuenta está desactivada."
si !hasher.Verificar(u.HashContrasena, contrasena):
     Fallar(u); si quedó bloqueada -> "Cuenta bloqueada 15 minutos por 5 intentos fallidos. Un administrador puede restablecerla."
                si no              -> "Correo o contraseña incorrectos."
si !totp.Verificar(u.SecretoTotp, codigo):
     Fallar(u); si quedó bloqueada -> "Código incorrecto. Cuenta bloqueada 15 minutos."
                si no              -> $"Código incorrecto. Intento {u.IntentosFallidos} de 5."
éxito: u.IntentosFallidos = 0; u.UltimoIngreso = ahora; GuardarCambiosAsync(); return u

Fallar(u): u.IntentosFallidos++; si u.IntentosFallidos >= 5 -> u.BloqueadoHasta = ahora + 15 min, u.IntentosFallidos = 0; GuardarCambiosAsync()
```

- [ ] **I2.1 Escribir las pruebas que fallan** (`HasherFalso`: `Hash(x) = "H:" + x`, `Verificar(h, x) = h == "H:" + x`; `TotpFalso`: solo acepta `"482913"`; usuario `laura.gomez@pitboss.co` con `HashContrasena = "H:Casino2026"`; `RelojMovible` es un `TimeProvider` cuyo `Ahora` se puede avanzar)

```csharp
[Fact] public async Task Ingresa_con_contrasena_y_codigo_correctos()        // devuelve el usuario; UltimoIngreso == ahora; IntentosFallidos 0
[Fact] public async Task El_correo_no_distingue_mayusculas()                // "LAURA.GOMEZ@PITBOSS.CO " -> ingresa
[Fact] public async Task Correo_inexistente_da_mensaje_generico()           // "Correo o contraseña incorrectos."
[Fact] public async Task Contrasena_errada_suma_un_intento()                // mensaje genérico; IntentosFallidos 1
[Fact] public async Task Codigo_errado_informa_el_intento()                 // "Código incorrecto. Intento 1 de 5."
[Fact] public async Task Cinco_codigos_errados_bloquean_15_minutos()
{
    for (var i = 1; i <= 4; i++) await Fallar(codigo: "000000");            // intentos 1..4
    var accion = () => _auth.IniciarSesionAsync("laura.gomez@pitboss.co", "Casino2026", "000000");
    await accion.Should().ThrowAsync<AutenticacionException>().WithMessage("Código incorrecto. Cuenta bloqueada 15 minutos.");
    _laura.BloqueadoHasta.Should().Be(Ahora.AddMinutes(15));
}
[Fact] public async Task Cinco_contrasenas_erradas_tambien_bloquean()      // 5.º -> mensaje de cuenta bloqueada
[Fact] public async Task Bloqueada_no_ingresa_ni_con_datos_correctos()
[Fact] public async Task El_bloqueo_vence_a_los_15_minutos()               // reloj + 15 min 1 s -> ingresa; BloqueadoHasta null
[Fact] public async Task Cuenta_desactivada_no_ingresa()                    // "Esta cuenta está desactivada."
[Fact] public async Task Un_ingreso_correcto_reinicia_el_contador()        // 2 fallos + éxito -> IntentosFallidos 0
[Fact] public void La_contrasena_temporal_es_segura()                      // 100 generadas: longitud 12, EsSegura true, todas distintas
[Theory, InlineData("corta1!"), InlineData("sinmayuscula1!"), InlineData("SinNumero!!"), InlineData("SinEspecial12")]
public void EsSegura_rechaza_contrasenas_debiles(string c)                  // false
```

- [ ] **I2.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter AutenticacionServiceTests`. Esperado: falla.
- [ ] **I2.3** Implementar.
- [ ] **I2.4** Ejecutar el mismo comando. Esperado: verde.
- [ ] **I2.5** Commit: `HU-06: Usuario y autenticacion con TOTP y bloqueo por intentos`.

---

### Tarea I3 · `UsuarioService`: gestión de cuentas y asignación de roles (requiere C4)

**Archivos:** `src/CasinoErp.Dominio/Seguridad/UsuarioService.cs`; pruebas en `UsuarioServiceTests.cs`. Se usa `EmpleadosEnMemoria`, que Felipe crea en `Dobles/PersonalEnMemoria.cs` (F1). Si todavía no está en `develop`, escribe primero las pruebas que no lo necesitan y anota el bloqueo.

**Interfaces que produce:**

```csharp
public record UsuarioCreado(Usuario Usuario, string ContrasenaTemporal, string UriTotp);
public record ContrasenaRestablecida(Usuario Usuario, string ContrasenaTemporal);

public class UsuarioService(IUsuarioRepositorio usuarios, IEmpleadoRepositorio empleados, IHasherContrasena hasher, IVerificadorTotp totp)
{
    public const string MensajeCreado = "Usuario creado", MensajeRol = "Rol actualizado", MensajeRestablecida = "Contraseña restablecida",
                        MensajeDesactivado = "Usuario desactivado", MensajeReactivado = "Usuario reactivado",
                        MensajeContrasena = "Contraseña actualizada";
    public Task<UsuarioCreado> CrearAsync(string nombre, string correo, Rol rol, string? empleadoDocumento);
    public Task<Usuario> CambiarRolAsync(int id, Rol rol);
    public Task<ContrasenaRestablecida> RestablecerContrasenaAsync(int id);
    public Task<Usuario> DesactivarAsync(int id);
    public Task<Usuario> ReactivarAsync(int id);
    public Task<Usuario> ConsultarAsync(int id);
    public Task<IReadOnlyList<Usuario>> ListarAsync();
    public Task CambiarContrasenaAsync(int id, string actual, string nueva);
}
```

**Reglas** (en el orden en que se evalúan en `CrearAsync`):

| Regla | Mensaje |
|---|---|
| Nombre vacío | `Escribe el nombre.` |
| Correo (con `Trim().ToLowerInvariant()`) que no cumple `^[^@\s]+@[^@\s]+\.[^@\s]+$` | `El correo no es válido.` |
| Correo existente | `Ya existe una cuenta con ese correo.` |
| Rol Crupier sin `empleadoDocumento` | `Vincula el usuario de crupier a un empleado para asignarle su mesa.` |
| `empleadoDocumento` que no existe | `NoEncontradoException("Empleado no encontrado")` |
| Rol Crupier y el empleado no es `Cargo.Crupier` | `El empleado vinculado no es crupier.` |
| Crear | Se genera `ContrasenaTemporal.Generar()`, `HashContrasena = hasher.Hash(temp)`, `SecretoTotp = totp.NuevoSecreto()`, `DebeCambiarContrasena = true`; devuelve `UsuarioCreado(u, temp, totp.UriConfiguracion(secreto, correo))` |
| `CambiarRolAsync` a un rol distinto de Administrador cuando el usuario es el **único Administrador activo** | `No puedes quitar el rol al último administrador.` |
| `CambiarRolAsync` a Crupier sin empleado crupier vinculado | `El empleado vinculado no es crupier.` (o el mensaje de "Vincula…" si no tiene empleado) |
| `DesactivarAsync` del único Administrador activo | `No puedes desactivar al último administrador.` |
| `DesactivarAsync` de uno inactivo / `ReactivarAsync` de uno activo | `El usuario ya está inactivo.` / `El usuario ya está activo.` |
| `RestablecerContrasenaAsync` | Contraseña temporal nueva, `DebeCambiarContrasena = true`, `IntentosFallidos = 0`, `BloqueadoHasta = null`. **El secreto TOTP no cambia.** |
| `CambiarContrasenaAsync`: la actual no coincide | `AutenticacionException("La contraseña actual no es correcta.")` |
| `CambiarContrasenaAsync`: la nueva no es segura | `La contraseña necesita 8 caracteres, una mayúscula, un número y un carácter especial.` |
| `CambiarContrasenaAsync`: éxito | Guarda el hash y `DebeCambiarContrasena = false` |
| Id inexistente | `NoEncontradoException("Usuario no encontrado")` |

- [ ] **I3.1 Escribir las pruebas que fallan**

```csharp
[Fact] public async Task Crea_usuario_con_contrasena_temporal_y_secreto_totp()
{
    var r = await _servicio.CrearAsync("Marta Gil", "  Marta.Gil@PitBoss.co ", Rol.Cajero, null);
    r.Usuario.Correo.Should().Be("marta.gil@pitboss.co");
    r.Usuario.DebeCambiarContrasena.Should().BeTrue();
    ContrasenaTemporal.EsSegura(r.ContrasenaTemporal).Should().BeTrue();
    r.Usuario.HashContrasena.Should().Be("H:" + r.ContrasenaTemporal);
    r.UriTotp.Should().StartWith("otpauth://totp/PitBoss:marta.gil@pitboss.co?secret=");
}
[Fact] public async Task Exige_nombre()                                    // "Escribe el nombre."
[Fact] public async Task Rechaza_correo_invalido()                         // "marta@" -> "El correo no es válido."
[Fact] public async Task Rechaza_correo_duplicado_sin_importar_mayusculas()
[Fact] public async Task Crupier_debe_vincular_un_empleado()               // "Vincula el usuario de crupier a un empleado para asignarle su mesa."
[Fact] public async Task Crupier_vinculado_a_un_cajero_se_rechaza()        // empleado Cargo.Cajero -> "El empleado vinculado no es crupier."
[Fact] public async Task Empleado_inexistente_lanza_no_encontrado()
[Fact] public async Task Crupier_vinculado_correctamente()                 // EmpleadoDocumento guardado
[Fact] public async Task Cambia_el_rol()                                   // Anfitrion -> Cajero
[Fact] public async Task No_quita_el_rol_al_ultimo_administrador()
[Fact] public async Task Si_hay_dos_administradores_se_puede_cambiar_uno()
[Fact] public async Task No_desactiva_al_ultimo_administrador()
[Fact] public async Task Desactiva_y_reactiva()
[Fact] public async Task Restablecer_levanta_el_bloqueo_y_conserva_el_totp()
[Fact] public async Task Cambiar_contrasena_exige_la_actual()              // AutenticacionException "La contraseña actual no es correcta."
[Fact] public async Task Cambiar_contrasena_valida_la_fortaleza()
[Fact] public async Task Cambiar_contrasena_quita_la_marca_de_temporal()
[Fact] public async Task Usuario_inexistente_lanza_no_encontrado()
```

- [ ] **I3.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter UsuarioServiceTests`. Esperado: falla.
- [ ] **I3.3** Implementar.
- [ ] **I3.4** Ejecutar la suite unitaria y el comando de cobertura. Esperado: verde y ≥ 80 %.
- [ ] **I3.5** Commit, push y PR (I2 + I3): `HU-06: Gestion de usuarios y asignacion de roles (dominio)`.

---

### Tarea I4 · Infraestructura de seguridad y cuentas de demo (contrato C6; requiere la Fase 0)

**Rama:** `feature/HU-06-infraestructura`.

**Archivos:**
- En `CasinoErp.Infraestructura.csproj`: `<PackageReference Include="Microsoft.Extensions.Identity.Core" Version="8.0.11" />` y `<PackageReference Include="Otp.NET" Version="1.4.0" />`
- Crear: `src/CasinoErp.Infraestructura/Seguridad/HasherContrasena.cs`, `VerificadorTotp.cs`; `Configuraciones/UsuarioConfiguracion.cs`; `Repositorios/SeguridadRepositorios.cs`; `Semillas/SemillaSeguridad.cs`
- Modificar: `Modulos/ModuloSeguridad.cs`

**Decisiones de implementación:**
- `HasherContrasena : IHasherContrasena` usa `new PasswordHasher<object>()`: `Hash` = `HashPassword(null!, c)`; `Verificar` = `VerifyHashedPassword(null!, h, c) != PasswordVerificationResult.Failed`.
- `VerificadorTotp(IConfiguration configuracion) : IVerificadorTotp`:
  - `NuevoSecreto()` = `Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20))`.
  - `Verificar(s, c)`: si `configuracion["Seguridad:CodigoTotpDemo"]` no está vacío y es igual a `c`, devuelve `true`. Si no, `new Totp(Base32Encoding.ToBytes(s)).VerifyTotp(c, out _, VerificationWindow.RfcSpecifiedNetworkDelay)`.
  - `UriConfiguracion` con el formato de la interfaz (`Uri.EscapeDataString` en el correo).
- `UsuarioConfiguracion`: tabla `Usuarios`; índice único en `Correo` (máx. 160); `Nombre` máx. 120; `Rol` como `string` (máx. 20); `HashContrasena` máx. 200; `SecretoTotp` máx. 64; `EmpleadoDocumento` máx. 10.
- `UsuarioRepositorio` usa `db.Set<Usuario>()`.
- `ModuloSeguridad.AgregarSeguridad()`: `AddSingleton<IHasherContrasena, HasherContrasena>()`, `AddSingleton<IVerificadorTotp, VerificadorTotp>()`, `AddScoped<IUsuarioRepositorio, UsuarioRepositorio>()`, `AddScoped<UsuarioService>()`, `AddScoped<AutenticacionService>()` y `AddScoped<ISemilla, SemillaSeguridad>()`.
- `SemillaSeguridad(IHasherContrasena hasher, IVerificadorTotp totp) : ISemilla` con `Orden = 40`: si `db.Set<Usuario>().Any()`, no hace nada. Si no, crea **las 11 cuentas de `contratos.md` §C6**, con el nombre del mockup (`seed().users`), el rol y el estado de la tabla, `HashContrasena = hasher.Hash("Casino2026")`, `SecretoTotp = totp.NuevoSecreto()`, `DebeCambiarContrasena = false` y el `EmpleadoDocumento` de la tabla.

- [ ] **I4.1 Escribir la prueba de integración que falla** (en `SeguridadApiTests.cs`, todavía sin JWT; consulta la base directamente con un scope de la factory)

```csharp
[Fact] public void La_semilla_crea_las_11_cuentas_de_demo()
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CasinoErpDbContext>();
    var usuarios = db.Set<Usuario>().ToList();
    usuarios.Should().HaveCount(11);
    usuarios.Single(u => u.Correo == "carolina.rios@pitboss.co").Activo.Should().BeFalse();
    usuarios.Single(u => u.Correo == "ana.torres@pitboss.co").EmpleadoDocumento.Should().Be("1010200300");
    var hasher = scope.ServiceProvider.GetRequiredService<IHasherContrasena>();
    hasher.Verificar(usuarios[0].HashContrasena, "Casino2026").Should().BeTrue();
}
```

- [ ] **I4.2** Ejecutar `dotnet test tests/CasinoErp.PruebasIntegracion --filter SeguridadApiTests`. Esperado: falla.
- [ ] **I4.3** Implementar.
- [ ] **I4.4** Ejecutar `dotnet test CasinoErp.sln`. Esperado: verde.
- [ ] **I4.5** Commit, push y PR: `HU-06: Hash de contrasenas, TOTP y cuentas de demo`.

---

### Tarea I5 · JWT, login y autorización por módulo en la API (contrato C5, parte 2) — HITO "SEGURIDAD ACTIVA"

**Rama:** `feature/HU-06-jwt`, tomada de `develop` **actualizado el mismo día** que abres el PR.

**Archivos:**
- En `CasinoErp.Api.csproj`: `<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />`
- Crear: `src/CasinoErp.Api/Seguridad/OpcionesJwt.cs`, `JwtEmisor.cs`, `AutorizarModuloAttribute.cs`, `UsuarioDto.cs`
- Crear: `src/CasinoErp.Api/Controllers/AuthController.cs`, `UsuariosController.cs`
- Modificar: `Program.cs`, `FiltroExcepciones.cs` (`AutenticacionException` → 401), `appsettings.json` (`"Jwt": { "Emisor": "CasinoErp", "Audiencia": "CasinoErp.Front", "HorasVigencia": 8 }`, sin clave), `appsettings.Development.json` (`"Jwt": { "Clave": "desarrollo-local-pitboss-erp-clave-de-48-caracteres" }` y `"Seguridad": { "CodigoTotpDemo": "482913" }`)
- Modificar: todos los controladores que haya en `develop` (tabla de C5)
- Crear: `tests/CasinoErp.PruebasIntegracion/Utilidades/Autenticacion.cs` (C3)
- Modificar: `Utilidades/CasinoErpFactory.cs` (+ `Jwt:Clave` de prueba y `Seguridad:CodigoTotpDemo`), y la creación de clientes en todas las pruebas de integración que haya en `develop`
- Pruebas: `SeguridadApiTests.cs`

**Interfaces que produce:**

```csharp
public class OpcionesJwt { public string Emisor { get; set; } = "CasinoErp"; public string Audiencia { get; set; } = "CasinoErp.Front";
                           public string Clave { get; set; } = ""; public int HorasVigencia { get; set; } = 8; }
public class JwtEmisor(IOptions<OpcionesJwt> opciones, TimeProvider reloj)
{
    public (string Token, DateTime Expira) Emitir(Usuario usuario);   // claims: sub=Id, name=Nombre, email=Correo, ClaimTypes.Role=Rol.ToString(); HMAC-SHA256
}
public record UsuarioDto(int Id, string Nombre, string Correo, Rol Rol, bool Activo, bool Bloqueado, string? EmpleadoDocumento,
                         DateTime? UltimoIngreso, bool DebeCambiarContrasena)
{
    public static UsuarioDto De(Usuario u, DateTime ahora);   // Bloqueado = u.BloqueadoHasta > ahora
}
public record LoginRequest(string Correo, string Contrasena, string Codigo);
public record LoginResponse(string Token, DateTime Expira, UsuarioDto Usuario, IReadOnlyCollection<string> Modulos);
public record CrearUsuarioRequest(string Nombre, string Correo, Rol Rol, string? EmpleadoDocumento);
public record CambiarRolRequest(Rol Rol);
public record CambiarContrasenaRequest(string Actual, string Nueva);
```

**Endpoints:**

| Verbo y ruta | Autorización | Respuesta |
|---|---|---|
| `POST /api/auth/login` | `[AllowAnonymous]` | 200 `LoginResponse`; 401 `{ mensaje }` |
| `GET /api/auth/yo` | `[Authorize]` | `{ usuario: UsuarioDto, modulos }` |
| `POST /api/auth/cambiar-contrasena` | `[Authorize]` (usa el `sub` del token) | `{ mensaje: "Contraseña actualizada" }` |
| `GET /api/salud` | Anónimo (`app.MapGet("/api/salud", () => Results.Ok(new { estado = "ok" })).AllowAnonymous()`) | `{ estado: "ok" }` |
| `GET /api/usuarios` | `[AutorizarModulo(Modulos.Usuarios)]` | `IReadOnlyList<UsuarioDto>` |
| `POST /api/usuarios` | idem | 201 `{ mensaje: "Usuario creado", datos: { usuario, contrasenaTemporal, uriTotp } }` |
| `PUT /api/usuarios/{id}/rol` | idem | `Respuesta<UsuarioDto>("Rol actualizado", …)` |
| `POST /api/usuarios/{id}/restablecer` | idem | `{ mensaje: "Contraseña restablecida", datos: { usuario, contrasenaTemporal } }` |
| `POST /api/usuarios/{id}/desactivar` | idem | `"Usuario desactivado"` |
| `POST /api/usuarios/{id}/reactivar` | idem | `"Usuario reactivado"` |

**Configuración en `Program.cs`:**

```csharp
builder.Services.AddOptions<OpcionesJwt>().Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => o.Clave.Length >= 32, "Jwt:Clave debe tener al menos 32 caracteres").ValidateOnStart();
builder.Services.AddSingleton<JwtEmisor>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<OpcionesJwt>>((o, jwt) => { /* TokenValidationParameters: ValidIssuer, ValidAudience, IssuerSigningKey,
        ValidateLifetime = true, ClockSkew = 1 min, RoleClaimType = ClaimTypes.Role; MapInboundClaims = false o NameClaimType = "name" */
        o.Events = new JwtBearerEvents { OnTokenValidated = ValidarUsuarioVigente }; });
builder.Services.AddAuthorization();
// Swagger: AddSecurityDefinition("Bearer", ...) + AddSecurityRequirement
...
app.UseCors(); app.UseDefaultFiles(); app.UseStaticFiles();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
```

`ValidarUsuarioVigente(TokenValidatedContext ctx)` cumple el punto de revisión "un token de un usuario desactivado o con otro rol deja de servir":
1. Toma `sub` del principal.
2. Resuelve `IUsuarioRepositorio` desde `ctx.HttpContext.RequestServices` y carga el usuario.
3. Si es `null`, `!Activo` o `Rol.ToString()` no coincide con el claim de rol → `ctx.Fail("Sesión no vigente")`.

- [ ] **I5.1 Escribir las pruebas de integración que fallan** (`SeguridadApiTests`; usan `factory.ClienteComoAsync(correo)` de C3, que se implementa en esta misma tarea)

```csharp
[Fact] public async Task Sin_token_la_api_responde_401() =>
    (await _factory.CreateClient().GetAsync("/api/jugadores")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
[Fact] public async Task Salud_y_front_son_anonimos()                // GET /api/salud -> 200; GET / -> 200 (index.html)
[Fact] public async Task El_login_devuelve_token_y_los_modulos_del_rol()
{
    var r = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
        new { correo = "laura.gomez@pitboss.co", contrasena = "Casino2026", codigo = "482913" });
    r.StatusCode.Should().Be(HttpStatusCode.OK);
    var j = await r.LeerJsonAsync();
    j.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    j.GetProperty("usuario").GetProperty("rol").GetString().Should().Be("CAJERO");
    j.GetProperty("modulos").EnumerateArray().Select(x => x.GetString()).Should().BeEquivalentTo(["caja", "inventario"]);
}
[Fact] public async Task Codigo_errado_responde_401_con_mensaje()   // { mensaje: "Código incorrecto. Intento 1 de 5." }
[Fact] public async Task Cuenta_inactiva_responde_401()             // carolina.rios -> "Esta cuenta está desactivada."
[Fact] public async Task El_cajero_entra_a_caja_pero_no_a_usuarios_ni_al_resumen()
{   // laura: GET /api/caja/transacciones 200; GET /api/usuarios 403; GET /api/resumen 403 (el cajero no tiene "inicio")
}
[Fact] public async Task El_crupier_solo_entra_a_mesas_y_retencion() // ana.torres: GET /api/mesas 200; GET /api/caja/transacciones 403
[Fact] public async Task El_admin_crea_usuario_y_la_respuesta_no_expone_secretos()
{
    var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
    var r = await admin.PostAsJsonAsync("/api/usuarios", new { nombre = "Marta Gil", correo = "marta.gil@pitboss.co", rol = "CAJERO" });
    r.StatusCode.Should().Be(HttpStatusCode.Created);
    var texto = await r.Content.ReadAsStringAsync();
    texto.Should().Contain("contrasenaTemporal").And.Contain("otpauth://")
         .And.NotContain("hashContrasena").And.NotContain("secretoTotp");
}
[Fact] public async Task El_listado_de_usuarios_no_expone_secretos()   // GET /api/usuarios: sin "hashContrasena" ni "secretoTotp"
[Fact] public async Task El_gerente_no_administra_usuarios()           // daniel.ruiz: GET /api/usuarios 403
[Fact] public async Task Un_token_de_usuario_desactivado_deja_de_servir()
{
    var cajero = await _factory.ClienteComoAsync("laura.gomez@pitboss.co");
    var admin = await _factory.ClienteComoAsync("ivan.leon@pitboss.co");
    var id = await IdDe(admin, "laura.gomez@pitboss.co");
    await admin.PostAsync($"/api/usuarios/{id}/desactivar", null);
    (await cajero.GetAsync("/api/caja/transacciones")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
[Fact] public async Task Cambiar_el_rol_invalida_el_token_anterior()   // sebastian.prada: rol a CAJERO; su token viejo -> 401; nuevo login -> modulos ["caja","inventario"]
[Fact] public async Task Asignar_un_rol_cambia_los_modulos_accesibles() // la demo de la guía, con andrea.mendez (Cumplimiento): antes GET /api/resumen 200;
                                                                        // el admin le asigna ANFITRION; nuevo login -> GET /api/resumen 200, GET /api/caja/transacciones 403
[Fact] public async Task No_se_puede_quitar_el_rol_al_ultimo_admin()    // PUT /api/usuarios/{idIvan}/rol {rol:"GERENTE"} -> 400 { mensaje: "No puedes quitar el rol al último administrador." }
[Fact] public async Task Cambiar_contrasena()                           // con el token propio: actual "Casino2026", nueva "Nueva#2026x" -> 200; login con la nueva -> 200
```

Las pruebas que desactivan o cambian roles deben usar cuentas que ninguna otra prueba de la misma clase necesite (por ejemplo, `sebastian.prada`, `andrea.mendez`, o usuarios creados en la propia prueba). Así el orden de ejecución no importa.

- [ ] **I5.2** Ejecutar `dotnet test tests/CasinoErp.PruebasIntegracion --filter SeguridadApiTests`. Esperado: falla.
- [ ] **I5.3** Implementar `OpcionesJwt`, `JwtEmisor`, `UsuarioDto`, `AutorizarModuloAttribute`, los dos controladores, el filtro (`AutenticacionException` → 401 con `{ mensaje }`), `Program.cs` y la configuración.
- [ ] **I5.4** `git pull --rebase origin develop`. Poner `[AutorizarModulo(...)]` (tabla de C5) en **cada controlador que haya en `develop`**, incluidos los de Felipe y Camilo si ya se fusionaron. Para comprobarlo: `grep -L "AutorizarModulo\|AllowAnonymous" src/CasinoErp.Api/Controllers/*.cs` no debe listar nada.
- [ ] **I5.5** Migrar **todas** las pruebas de integración que haya en `develop`:
  - `ApiTests` → `ClienteComoAsync("daniel.ruiz@pitboss.co")` (gerente);
  - `BaseDatosTests` → gerente;
  - `PersonalNominaApiTests` → `felipe.cortes@pitboss.co`;
  - `InventarioComprasApiTests` → `daniel.ruiz@pitboss.co`.
  
  Si una prueba del Sprint 1 necesita un módulo que el gerente no tiene (por ejemplo, `mimesa`), usar la cuenta del rol adecuado. **No se borran pruebas.**
- [ ] **I5.6** Ejecutar `dotnet test CasinoErp.sln` y el comando de cobertura. Esperado: **todo** verde (el Sprint 1, lo de los demás y lo tuyo) y ≥ 80 %.
- [ ] **I5.7** Probar a mano en Swagger (http://localhost:5080/swagger): `POST /api/auth/login` → copiar el token → **Authorize** → `GET /api/usuarios` con `ivan.leon` (200) y con `laura.gomez` (403).
- [ ] **I5.8** Commit, push y PR: `HU-06: JWT, login con 2FA y autorizacion por modulo en toda la API`. Avisar al equipo antes de hacer merge.
- [ ] **I5.9** Después del merge, crear `desarrollo/decisiones/D-0NN-ivan-seguridad-activa.md` (estado `Aceptada`) con:
  - "desde el commit `<hash>` toda la API exige token";
  - qué deben hacer los demás: `git pull --rebase origin develop`; crear los clientes de prueba con `factory.ClienteComoAsync(correo)`; anotar sus controladores nuevos con `[AutorizarModulo]` según C5;
  - la clave de desarrollo ya está en `appsettings.Development.json`.
  
  Avisar al equipo por el chat del grupo.

---

### Tarea I6 · Front: sesión real y gestión de usuarios (`js/api/seguridad.js`)

**Rama:** `feature/HU-06-front`. **Solo se edita `src/CasinoErp.Api/wwwroot/js/api/seguridad.js`** (contrato C2).

**Comportamiento que se implementa:**

1. **Token:** `const CLAVE = 'pitboss.token'`. Las funciones `leer()`, `guardar(t)` y `borrar()` usan `sessionStorage` dentro de `try/catch`; si el almacenamiento falla, el token vive en memoria en una variable local.
2. **Ganchos del núcleo:**
   - `PitBossApi.encabezados = () => leer() ? { Authorization: 'Bearer ' + leer() } : {}`
   - `PitBossApi.alNoAutorizado = r => { if (!S.user) return; borrar(); S.user = null; S.modal = null; render(); toast('Tu sesión terminó. Inicia sesión de nuevo.', true); }`. El núcleo no lo llama en `/auth/login` y ya convierte los 403 en `No tienes acceso a este módulo.` (contrato C2).
   - `PitBossApi.antesDeSincronizar = async () => { if (!leer()) return false; try { const yo = await pedir('GET', '/auth/yo'); entrar(yo.usuario); return true; } catch { borrar(); return false; } }`
3. **`entrar(u)`:** busca en `S.users` por `u.correo`. Si no existe, lo agrega con `id: 'ua' + u.id`. Le asigna:
   - `name`, `email`, `role = ROL_FRONT[u.rol]` (tabla de C7), `st` (`Activo`/`Inactivo`), `apiId = u.id`, `last`;
   - `sid` = el `id` de `S.staff` cuyo `apiDoc` o `docApi(doc)` coincide con `u.empleadoDocumento`.
   
   Luego: `S.user = { uid: <id front> }`, `S.view = ROLES[rol].home`, `S.login.err = ''`, `S.mode = 'erp'`.
4. **Login:** reemplazar `loginView` (es una función global) con una versión que muestre, en vez del `<select>` de usuarios:
   - `<input class="in" id="lg-mail" list="lg-users" data-bind="login.correo" value="${esc(S.login.correo ?? userById(S.login.uid)?.email ?? '')}">` con un `<datalist id="lg-users">` que tiene los correos de `S.users` (así se pueden escribir correos nuevos);
   - el resto del formulario igual que en el mockup (contraseña, código TOTP, mensaje de error);
   - el texto de demo cambia a: `Demo local: contraseña Casino2026 y código 482913. Con 5 intentos errados la cuenta se bloquea 15 minutos. Cada rol solo ve sus módulos.`
   
   Sin API (`!PitBossApi.estado.online`), se usa el `loginView` original.
5. **`FORM.login`:** se guarda `const loginLocal = FORM.login`. Con la API en línea, el formulario hace `pedir('POST', '/auth/login', { correo: S.login.correo || userById(S.login.uid).email, contrasena: S.login.pass, codigo: S.login.code })`. Si sale bien: `guardar(r.token)`, `entrar(r.usuario)`, `await PitBossApi.sincronizar()`, y si `r.usuario.debeCambiarContrasena`, `S.modal = { type: 'cambiarContrasena' }`. Si falla: `S.login.err = e.message; render()`. Sin API se usa `loginLocal(fd)`.
6. **`ACT.logout`:** se guarda el original; se llama a `borrar()` y luego al original.
7. **Carga `usuarios` (orden 70):** si `allowed('usuarios')`, `GET /usuarios`. Cada usuario se integra en `S.users` con `entrar`-like (sin cambiar la sesión): `role`, `st`, `last` (`dd/MM · HH:mm` de `ultimoIngreso` con `util.fechaApi` y `util.hora`) y `apiId`.
8. **Acciones de usuarios** con `PitBossApi.conectar` (el prototipo valida primero y la API confirma):
   - `FORM.userNew`: `antes(fd)` captura `{ n: S.users.length, nombre, correo, rol: ROL_API[fd.get('role')], empleadoDocumento: (staff(fd.get('sid'))?.apiDoc) || null }`; `exito: ctx => S.users.length > ctx.n`; `guardar` hace `POST /usuarios` y luego `S.modal = { type: 'csv', title: 'Usuario creado', sub: 'Entrega estos datos una sola vez a la persona. Debe cambiar la contraseña en su primer ingreso.', csv: 'Correo: …\nContraseña temporal: …\nConfigurar app autenticadora: …uriTotp' }`. Reutiliza `MODALS.csv` del mockup, que tiene botón de copiar.
   - `FORM.userRole` → `PUT /usuarios/{apiId}/rol { rol }`
   - `ACT.resetPass` → `POST /usuarios/{apiId}/restablecer`, luego el modal `csv` con la contraseña temporal
   - `ACT.userOff` → `POST /usuarios/{apiId}/desactivar`
   - `ACT.userOn` → `POST /usuarios/{apiId}/reactivar`
9. **`MODALS.cambiarContrasena` y `FORM.cambiarContrasena`:** campos `actual`, `nueva` y `nueva2`. Si `nueva !== nueva2` → `Las contraseñas no coinciden.` Si no, `POST /auth/cambiar-contrasena`, toast `Contraseña actualizada.` y `S.modal = null`.

- [ ] **I6.1** Implementar `seguridad.js` como IIFE.
- [ ] **I6.2 Probar a mano** (`$env:BaseDatos__RecrearAlIniciar="true"; dotnet run --project src/CasinoErp.Api`). Anotar cada resultado en la bitácora:
  1. `laura.gomez@pitboss.co` / `Casino2026` / `482913` → entra y solo ve **Caja / POS** e **Inventario de fichas**.
  2. Código `000000` → `Código incorrecto. Intento 1 de 5.`; cinco veces → `Código incorrecto. Cuenta bloqueada 15 minutos.`
  3. `carolina.rios@pitboss.co` → `Esta cuenta está desactivada.`
  4. Como `ivan.leon@pitboss.co` (admin), en **Usuarios y roles**: crear "Marta Gil" (`marta.gil@pitboss.co`, Cajero) → modal con la contraseña temporal y la URI TOTP; al recargar, sigue en la tabla.
  5. Iván restablece la contraseña de Laura → se levanta el bloqueo del paso 2.
  6. **Demo de la guía:** Iván cambia el rol de **Sebastián Prada** de Anfitrión a **Cajero** → en otra ventana privada, Sebastián (que ya tenía sesión) recibe `Tu sesión terminó…` en su siguiente acción; al entrar de nuevo, solo ve Caja e Inventario.
  7. Marta entra con su contraseña temporal y el código demo → aparece el modal para cambiar la contraseña; con `Nueva#2026x` → toast; sale y entra con la nueva.
  8. Al cerrar sesión y recargar (F5), aparece el login; con una sesión abierta y F5, se restaura sin pedir login (`/auth/yo`).
  9. Con la API apagada, funciona el login local del mockup.
- [ ] **I6.3** Commit y PR: `HU-06: Front con sesion JWT, login con 2FA y gestion de usuarios`.

---

### Tarea I7 · arc42: sección 10 (seguridad y roles), bloque Seguridad y ADR-002

- [ ] **I7.1** `docs/arc42/10-conceptos-seguridad.md`:
  - **10.1 Autenticación:** hash PBKDF2 de ASP.NET Identity, TOTP RFC 6238 (Otp.NET, ventana de ±1 paso), bloqueo de 5 intentos durante 15 minutos, contraseñas temporales y cambio obligatorio, código demo solo con `Seguridad:CodigoTotpDemo`.
  - **10.2 Sesión:** JWT HS256 de 8 horas (un turno), sus claims, la clave en `Jwt:Clave` (en Azure, el app setting que genera Terraform) y la revocación efectiva en `OnTokenValidated` (usuario inactivo o rol cambiado).
  - **10.3 Autorización:** `AutorizarModuloAttribute` → roles; la **matriz rol × módulo** completa (tabla de C5); la tabla de módulos por controlador (C5); y el front, que oculta los módulos sin permiso (no es una barrera de seguridad, solo de interfaz).
  - **10.4 Protección de datos:** `UsuarioDto` sin hash ni secreto; la contraseña temporal se muestra una sola vez; sin secretos en el repo.
  - **10.5 Coherencia con la sección 5:** una tabla de control de seguridad → bloque y clase donde se implementa (por ejemplo, `[AutorizarModulo(Modulos.Rrhh)]` → bloque Nómina → `NominaController`). Es la base del video de la Tarea 2 de la guía.
- [ ] **I7.2** `docs/arc42/05-bloques/seguridad.md`: un `classDiagram` (`AutenticacionService`, `UsuarioService`, `MatrizAcceso`, `Usuario`, `IHasherContrasena`, `IVerificadorTotp`, `JwtEmisor`, `AutorizarModuloAttribute`) y un `sequenceDiagram` del login con 2FA y de una petición autorizada o denegada (403).
- [ ] **I7.3** `docs/arc42/09-decisiones/ADR-002-jwt-totp.md`: contexto, opciones (cookies + Identity completo frente a JWT + TOTP propio), decisión y consecuencias (token en `sessionStorage`; revocación por consulta en cada petición).
- [ ] **I7.4** Commit y PR: `HU-08: arc42 seccion 10 seguridad y roles, bloque Seguridad y ADR-002`.

---

### Tarea I8 · Cierre y demo

- [ ] **I8.1** **Aporte a la retrospectiva:** entrada "Retrospectiva HU-06" en tu bitácora con horas reales contra lo estimado, el impacto de I5 en el equipo, qué salió bien, qué mejorar y los impedimentos.
- [ ] **I8.2** **Demo de la Sprint Review** (máximo 2 minutos, pasos 1, 4 y 6 de I6.2): login con 2FA → el admin asigna el rol Cajero a Sebastián → Sebastián solo ve Caja e Inventario → en Swagger, su token responde 403 en `/api/nomina`. Ensayarla y anotar el tiempo.
- [ ] **I8.3** Mover HU-06 a "Listo" en Jira. Escribir la entrada final de la bitácora (estado, pendientes y decisiones).
