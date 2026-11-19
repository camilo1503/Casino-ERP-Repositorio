# D-005 · Terraform importa la infraestructura de Azure que ya existe

- **Estado:** Aceptada · **Autor:** Equipo (plan inicial) · **Fecha:** 2026-11-18
- **Afecta a:** Jorge (HU-07), Iván (secreto JWT en la nube)

## Contexto
La rama `feature/CER-6-despliegue-azure` (commit `b9f4410`) documenta un despliegue manual ya funcionando: `rg-pitboss-erp` en Brazil South, App Service Linux F1 `pitboss-erp-8szxa`, Azure SQL gratuito serverless `CasinoErp` en `pitboss-sql-8szxa`, autenticación solo con Microsoft Entra ID e identidad administrada de la app.

## Decisión
Terraform **describe e importa** esos recursos con bloques `import`. No crea una infraestructura paralela. Los valores nuevos del Sprint 2 (`Jwt__Clave` y, si se decide, `Seguridad__CodigoTotpDemo`) se agregan como app settings administrados por Terraform. El state vive en Azure Blob Storage (backend remoto).

## Consecuencias
Hay que coordinar con quien tiene la suscripción de Azure (autor del commit `b9f4410`) para que Jorge tenga el rol Contributor y se puedan crear las credenciales OIDC del workflow.
