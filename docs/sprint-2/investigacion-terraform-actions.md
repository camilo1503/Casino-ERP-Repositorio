# Investigación: Terraform en GitHub Actions (Tarea 3 de la guía)

Workflow: [`.github/workflows/terraform-plan.yml`](../../.github/workflows/terraform-plan.yml).

## Cómo se dispara

```yaml
on:
  pull_request:
    branches: [develop]
    paths: ['infra/terraform/**/*.tf']
```

- `pull_request`: corre cuando se abre o se actualiza un PR.
- `branches: [develop]`: solo para PR cuyo destino es `develop`.
- `paths`: solo si el PR cambia algún `.tf` bajo `infra/terraform/`. Un PR de código .NET no gasta tiempo de Terraform.

## OIDC en vez de secretos de larga duración

Con un secreto de larga duración (`ARM_CLIENT_SECRET`), quien lo vea puede usar la cuenta de Azure hasta que alguien lo rote. Con **OIDC** (`permissions: id-token: write`), GitHub emite un token firmado y de vida corta para cada ejecución. Azure confía en él porque la App Registration `github-pitboss-terraform` tiene una **credencial federada** que solo acepta tokens de `repo:camilo1503/Casino-ERP-Repositorio:pull_request`. No hay contraseña que robar ni rotar. Los secretos del repositorio (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RG`, `TFSTATE_SA`) son identificadores, no credenciales.

## Qué hace cada paso

| Paso | Qué comprueba |
|---|---|
| `terraform fmt -check -recursive` | El código tiene el formato canónico; falla si alguien no corrió `fmt`. |
| `terraform init` | Descarga los proveedores y se conecta al state remoto en Blob Storage. |
| `terraform validate` | La sintaxis y las referencias entre recursos son válidas, sin tocar Azure. |
| `terraform plan` | Compara el código con el state y con Azure, y muestra qué cambiaría. No modifica nada; el `apply` lo hace una persona. |

El plan se guarda en `plan.txt` y se copia al resumen del job (`$GITHUB_STEP_SUMMARY`) para que el revisor lo lea sin abrir los logs.

## Por qué el state remoto evita conflictos

Con un `terraform.tfstate` local, dos personas (o un PR y una persona) pueden aplicar a la vez sobre estados distintos y duplicar o destruir recursos. El backend `azurerm` guarda un solo state en un blob y, mientras alguien ejecuta un `plan` o `apply`, **toma un lease (bloqueo) sobre el blob**. Una segunda ejecución espera (`-lock-timeout=2m`) o falla en vez de pisar la primera.

## Evidencia

Pendiente: la captura de un PR con el check "Terraform plan" en verde, que se guardará en `docs/sprint-2/evidencias/terraform-plan-pr.png`. Necesita la parte B de J4 (OIDC y secretos), que espera el acceso a Azure (D-010).
