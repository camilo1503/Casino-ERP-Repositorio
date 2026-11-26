# URL del despliegue · PitBoss ERP (entrega final)

Dueño: Felipe (Plan-Felipe-Despliegue, N5.2). Fuente de las cifras: `desarrollo/bitacoras/felipe.md`.

| Nube | URL | Commit | Fecha y hora | `/api/salud` | Login laura.gomez | Base compartida | Notas |
|---|---|---|---|---|---|---|---|
| Azure App Service (Linux F1) | https://pitboss-erp-8szxa.azurewebsites.net | `6caa823` (`develop`) | 2026-11-26 00:00 (Bogotá) | `{"estado":"ok"}` | Token; módulos `caja`, `inventario` | Azure SQL `pitboss-sql-8szxa` / `CasinoErp` (17 tablas, datos de demo) | Publicado como paquete zip con la CLI de Azure (`az webapp deploy`). La publicación desde VS Code para las capturas (D-015) queda pendiente. |
| Vercel (Hobby) | https://pitboss-erp.vercel.app | `6caa823` (`develop`, `wwwroot`) | 2026-11-25 23:50 (Bogotá) | `{"estado":"ok"}` (reenviado a Azure) | Token; módulos `caja`, `inventario` | La misma de Azure, a través de la API de Azure | **Contingencia de D-014 (N4.8):** Vercel sirve el front y reenvía `/api/*` y `/swagger/*` a Azure. El contenedor (N4.1–N4.7) espera el `Dockerfile` (E1) y la opción B (D-015). |

Prueba funcional adicional a través de Vercel: `felipe.cortes@pitboss.co` calcula la nómina de octubre de 2026 → "Nómina calculada", 18 filas, total neto $49.074.720.

## Variables configuradas (solo nombres)

**Azure App Service `pitboss-erp-8szxa`** (*Configuración → Variables de entorno*):
- `ConnectionStrings__CasinoErp` (Azure SQL con `Authentication=Active Directory Default`: identidad administrada de la app, sin contraseña)
- `BaseDatos__Proveedor`
- `BaseDatos__CargarDatosDemo`
- `TZ`
- `Jwt__Clave`
- `Seguridad__CodigoTotpDemo`

El taller (ejercicio 4) usa `DB_CONNECTION_STRING`. ASP.NET Core lee la variable de entorno `ConnectionStrings__CasinoErp` como la clave de configuración `ConnectionStrings:CasinoErp`, así que cumple el mismo papel sin cambiar el código.

**Vercel `pitboss-erp`:** no usa variables de entorno en la contingencia. Solo tiene `vercel.json` con los `rewrites` hacia Azure. Las variables de N4.4 se cargan cuando se despliegue el contenedor.
