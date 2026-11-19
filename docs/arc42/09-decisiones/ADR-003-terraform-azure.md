# ADR-003 · Terraform en Azure con import y state remoto en Blob Storage

- **Estado:** Aceptada · **Fecha:** 2026-11-18 · **Origen:** decisión D-005 (con el ajuste de D-010)

## Contexto
La infraestructura de Azure (grupo de recursos, App Service F1, Azure SQL gratuito y su identidad administrada) se creó a mano. Si no está descrita en código no se puede revisar en un PR ni reproducir. La guía pide infraestructura como código con `terraform plan` en cada PR.

## Decisión
- Terraform **describe e importa** los recursos que ya existen; no crea una infraestructura paralela.
- Los valores nuevos del Sprint 2 (`Jwt__Clave`, `Seguridad__CodigoTotpDemo`, `BaseDatos__RecrearAlIniciar`) pasan a ser app settings administrados por Terraform.
- El state vive en Azure Blob Storage (`rg-pitboss-tfstate`), con acceso solo por Entra ID y bloqueo por lease del blob.
- Un workflow de GitHub Actions ejecuta `fmt`, `validate` y `plan` en cada PR a `develop`, con OIDC y sin secretos de larga duración. El `apply` lo ejecuta una persona.

## Consecuencias
- Los cambios de infraestructura se revisan como cualquier otro PR.
- El proveedor `azurerm` 4.81 no representa la marca "oferta gratuita" de Azure SQL: la base se importa y lleva `ignore_changes` y `prevent_destroy` para que un `apply` no la recree.
- Hace falta que alguien con rol de Owner en la suscripción asigne los roles a quien opere Terraform (tarea F9, D-010). Se asignó Contributor y un administrador de roles con condición (D-011): solo permite asignar Reader, Contributor y Storage Blob Data Contributor.
- El directorio de la universidad no permite registrar aplicaciones en Entra ID. El workflow usa por eso una identidad administrada asignada por el usuario, con credencial federada para GitHub, en lugar de una App Registration. El efecto es el mismo (OIDC, sin secretos de larga duración).
- El state contiene `Jwt__Clave`; solo debe leerlo el equipo.
