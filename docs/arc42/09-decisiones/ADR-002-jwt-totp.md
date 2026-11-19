# ADR-002 · Sesión con JWT y segundo factor TOTP propio

- **Estado:** Aceptada · **Fecha:** 2026-11-18 · **Autor:** Iván · **Historia:** HU-06 · **Detalle:** [§10](../10-conceptos-seguridad.md) y [bloque Seguridad](../05-bloques/seguridad.md)

## Contexto
El mockup simula el login: elige un usuario de una lista y acepta siempre el código `482913`. La HU-06 pide que cada rol solo pueda usar sus módulos, y el mockup exige un segundo factor de 6 dígitos con bloqueo de 15 minutos tras 5 intentos. Hay otras condiciones:

- El front es HTML y JS estático servido por la misma API (`wwwroot`), sin framework. Habla con la API por `fetch` a través de `PitBossApi` (contrato C2).
- Se despliega en un único App Service F1 gratuito, sin Redis ni servicios extra.
- Un administrador debe poder **quitarle el acceso** a una persona (desactivarla o cambiarle el rol) y que esto aplique en su siguiente acción.
- Las pruebas de integración deben poder iniciar sesión sin una app autenticadora real.

## Opciones consideradas
1. **ASP.NET Core Identity completo con cookies.** Trae tablas (`AspNetUsers`, `AspNetRoles`…), páginas de Razor, autenticador TOTP integrado y bloqueo por intentos.
   - A favor: probado y completo.
   - En contra: impone su esquema de tablas y de roles, que no coincide con la matriz rol × módulo del mockup ni con `Usuario`; las cookies requieren protección CSRF para las llamadas `fetch`; esconde las reglas del mockup (mensajes exactos y bloqueo por códigos errados) detrás de su configuración, y es mucho más código del que se puede probar en un sprint.
2. **JWT con TOTP propio (elegida).** Dominio propio (`Usuario`, `AutenticacionService`, `UsuarioService`, `MatrizAcceso`) y solo dos piezas de bibliotecas: el `PasswordHasher` de Identity (PBKDF2) y Otp.NET (RFC 6238). La sesión es un JWT HS256 que el front manda en `Authorization: Bearer`.
   - A favor: las reglas y los mensajes del mockup quedan en el dominio con pruebas unitarias; encaja con el front que ya usa `fetch` y no necesita CSRF; no tiene estado en el servidor, así que sirve en un F1.
   - En contra: un JWT no se puede "cerrar" del lado del servidor por sí solo, y hay que escribir y probar la lógica de bloqueo.
3. **Proveedor externo (Microsoft Entra ID / Azure AD B2C).** Delega todo, incluido el MFA.
   - En contra: el directorio de la universidad no permite registrar aplicaciones a los estudiantes, y las cuentas de demo de la Sprint Review no son personas reales.

## Decisión
Opción 2:
- Contraseñas con `PasswordHasher<T>` de Identity (PBKDF2), TOTP con Otp.NET (ventana de ±1 paso) y bloqueo de 15 minutos tras 5 intentos en `AutenticacionService`.
- JWT HS256 de **8 horas** (un turno) con `sub`, `name`, `email` y el rol. La clave va en `Jwt:Clave` (Terraform la genera en Azure como `Jwt__Clave`).
- Autorización con `[AutorizarModulo(...)]`, que traduce módulos a roles con `MatrizAcceso`.
- **Revocación por consulta:** en cada petición, `OnTokenValidated` carga el usuario y rechaza el token si está inactivo o su rol cambió.
- El código de demo (`Seguridad:CodigoTotpDemo`) solo se acepta si está configurado (Development, pruebas y la Sprint Review).

## Consecuencias
- **Token en `sessionStorage`:** se pierde al cerrar la pestaña y no viaja solo en cada petición (no hay CSRF). Pero es legible por JavaScript: un XSS podría robarlo durante su vigencia. Se mitiga porque el front escapa todo lo que pinta (`esc`) y no carga scripts de terceros. Pasar a una cookie `HttpOnly` queda para el Sprint 3 si se agrega un front separado.
- **Una consulta a la base por petición** para la revocación. Con el volumen del casino es despreciable y evita una lista negra de tokens o un caché distribuido. Si la carga creciera, se puede reemplazar por un "sello de seguridad" en el usuario con caché corto.
- **Sin refresh tokens:** al cumplirse las 8 horas hay que volver a iniciar sesión, lo que coincide con el cambio de turno.
- **Rotar `Jwt:Clave`** invalida todas las sesiones abiertas (es lo esperado ante una filtración).
- Las reglas de seguridad quedan en el dominio con 97 % de cobertura, y las pruebas de integración inician sesión con `factory.ClienteComoAsync(correo)` (contrato C3).
- Desde el merge de la I5, toda prueba de integración y todo controlador nuevo debe usar `ClienteComoAsync` y `[AutorizarModulo]` (decisión de aviso "seguridad activa").
