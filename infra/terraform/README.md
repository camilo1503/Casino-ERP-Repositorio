# Infraestructura como código · PitBoss ERP (HU-07)

Terraform describe la infraestructura de Azure que ya existía (D-005): App Service Linux F1, Azure SQL (oferta gratuita, solo Entra ID) y los valores nuevos del Sprint 2. El state vive en Azure Blob Storage.

## Requisitos

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) y Terraform ≥ 1.6.
- Una cuenta con los roles **Contributor** y **User Access Administrator** (o Owner) sobre la suscripción.
- `az login` y la suscripción: `$env:ARM_SUBSCRIPTION_ID="<id>"`.

## Orden

1. **Bootstrap** (una sola vez): `terraform -chdir=infra/terraform/bootstrap init` y `apply`. Crea `rg-pitboss-tfstate`, la cuenta de storage y el contenedor `tfstate`. Anota los outputs.
2. **backend.hcl:** copia `backend.hcl.example` a `backend.hcl` y pon el nombre real de la cuenta. No se sube a Git.
3. **init:** `terraform -chdir=infra/terraform init -backend-config=backend.hcl`.
4. **Imports:** ya hechos en el state remoto (ver tarea J3.5 del plan). Solo se repiten si se pierde el state.
5. **plan / apply:** `terraform -chdir=infra/terraform plan` y `apply`.

Para revisar el código sin Azure: `init -backend=false`, `validate` y `fmt -check -recursive`.

## Variables que se usan en el despliegue

| Variable | Para qué | Valor habitual |
|---|---|---|
| `codigo_totp_demo` | Código TOTP de la Sprint Review | vacío (activo solo en la demo) |
| `recrear_base_al_iniciar` | Recrea la base al arrancar (D-006) | `false` |
| `admin_sql_login`, `admin_sql_object_id` | Administrador Entra ID del servidor SQL | se confirman al importar |
| `reglas_firewall` | Reglas de firewall del servidor SQL | se copian de `az sql server firewall-rule list` |

## Publicar la app

```powershell
dotnet publish src/CasinoErp.Api -c Release -o publish
Compress-Archive publish\* publish.zip -Force
az webapp deploy -g rg-pitboss-erp -n pitboss-erp-8szxa --src-path publish.zip --type zip
```

## Notas

- El usuario SQL de la identidad administrada (`CREATE USER [pitboss-erp-8szxa] FROM EXTERNAL PROVIDER`) se creó a mano y **no** lo maneja Terraform.
- El proveedor `azurerm` 4.81 no tiene argumentos para la marca "oferta gratuita" de Azure SQL. La base solo se importa; `prevent_destroy` evita que un `apply` la recree y se pierda la oferta.
- `Jwt__Clave` la genera `random_password.jwt` y queda en el state y en el app setting. El state no se comparte fuera del equipo.

## `terraform plan` en cada PR (workflow `terraform-plan.yml`)

Cada PR a `develop` que cambie un `.tf` ejecuta `fmt`, `init`, `validate` y `plan`, y deja el plan en el resumen del job. Entra a Azure con **OIDC** (sin secretos de larga duración): una App Registration `github-pitboss-terraform` con credencial federada para `repo:camilo1503/Casino-ERP-Repositorio:pull_request`.

| Rol de la identidad | Alcance | Para qué |
|---|---|---|
| Contributor | `rg-pitboss-erp` | leer los recursos y sus app settings |
| Storage Blob Data Contributor | la cuenta del state | leer y bloquear el state |

Secretos del repositorio (los carga quien administre el repo): `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RG` y `TFSTATE_SA`. El workflow solo hace `plan`; el `apply` lo ejecuta una persona (J6).
