# D-015 · Vercel entra a Azure SQL con un usuario contenido (opción B de D-014) y Azure se publica desde Visual Studio Code

- **Estado:** Aceptada por Felipe (dueño de la suscripción y de la base); falta que Jorge aplique la parte de Terraform
- **Autor:** Felipe (agente de Felipe)
- **Fecha:** 2026-11-25
- **Afecta a:** Jorge (Terraform, E5 y E5.4), Felipe (Plan-Felipe-Despliegue N1, N3 y N4)
- **Plan / tarea:** Plan-Felipe-Despliegue N0.2, N1 y N3; D-014

## Contexto
D-014 elige que Vercel entre a Azure SQL con un *service principal* (opción A). El 2026-11-25 se comprobó que el inquilino de la universidad tiene `allowedToCreateApps: false` en su política de autorización, y la cuenta de Felipe no tiene un rol de directorio que lo permita. No se puede crear el registro `pitboss-erp-vercel` (N3.1).

Además, N1 pide publicar desde **Visual Studio 2022**, que solo existe para Windows. Felipe trabaja en macOS, y Visual Studio para Mac ya no tiene soporte.

## Opciones consideradas
Para la base:
1. **Opción B de D-014 (elegida):** habilitar la autenticación SQL en `pitboss-sql-8szxa` (`azuread_authentication_only = false` y un login administrador en una variable sensible de Terraform) y crear en `CasinoErp` un usuario contenido para Vercel, con contraseña.
2. **Solo la contingencia de D-014:** Vercel sirve el front y reenvía `/api/*` a Azure. No cambia la seguridad del servidor, pero el backend queda en una sola nube.

Para publicar en Azure:
1. **Visual Studio Code en macOS con la extensión Azure App Service (elegida):** publica sobre la app existente, sin crear recursos.
2. Que otro integrante con Windows publique desde Visual Studio 2022.
3. JetBrains Rider con Azure Toolkit.

## Decisión
- **Base:** opción B. Usuario contenido `pitboss-erp-vercel` con `db_datareader` y `db_datawriter`, sin `db_ddladmin`. Contraseña aleatoria larga, guardada solo en el gestor de contraseñas de Felipe y en las variables de Vercel. El login administrador SQL de Terraform lo define y guarda Jorge, fuera del repo.
- **Riesgo aceptado por Felipe:** mientras dure la opción B, el servidor acepta usuario y contraseña de SQL y, con la regla `vercel-todas-las-ip`, cualquiera en internet puede intentar conectarse. Se mitiga con contraseñas largas y permisos mínimos, y se revierte al terminar la calificación (N6.3): `DROP USER`, `azuread_authentication_only = true` y fuera la regla de firewall.
- **Publicación en Azure:** Visual Studio Code con la extensión Azure App Service, sobre `pitboss-erp-8szxa` existente (nunca "Create new"), sin tocar los app settings ni las cadenas de conexión. Se guardan capturas de cada paso, con los mismos nombres de N1, como evidencia para la rúbrica.

## Consecuencias
- Jorge hace E5.4 (opción B en Terraform) junto con E5 (la regla de firewall) y avisa a Felipe (S3).
- N3.1 y N3.2 no aplican. N3.3 pasa a `CREATE USER [pitboss-erp-vercel] WITH PASSWORD = '…'` (usuario contenido) más los dos roles. N3.6 se verifica con `sqlcmd` en autenticación SQL.
- En N4.4, `ConnectionStrings__CasinoErp` de Vercel usa `User Id=pitboss-erp-vercel;Password=…` sin `Authentication=Active Directory Service Principal`.
- El documento final (E7.3) debe decir que Azure se publicó desde Visual Studio Code y por qué.

## Aprobación
- Dueño de la suscripción y de la base: Felipe · 2026-11-25 · Aprobada
- Dueño de `infra/`: Jorge · pendiente
