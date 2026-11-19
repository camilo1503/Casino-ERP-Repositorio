# Guía de la parte B de J2, J3 y J4 (Terraform con acceso a Azure)

Lista para ejecutar cuando Felipe termine la tarea **F9** (D-010). El código está en la rama `feature/HU-07-bootstrap-state` (parte A). Todo se ejecuta desde la raíz del repositorio, en PowerShell. **Nada de esto se ha probado contra Azure todavía.**

> Los comandos de `az` y de importación no se han corrido; los formatos de ID salen de la documentación del proveedor `azurerm` 4.x. Si un comando falla, se anota en la bitácora y se corrige aquí.

## 0. Antes de empezar

1. Cambiar a la rama: `git switch feature/HU-07-bootstrap-state` (y `git pull`).
2. Comprobar que F9 está hecha. Deben aparecer **Contributor** y **User Access Administrator** (u **Owner**) con alcance `/subscriptions/<id>`, no solo sobre la web app:

```powershell
az account set --subscription 12f1bc21-94a3-4b28-9a90-8224d1e5fcc8
az role assignment list --assignee jalcidesdiaz@ucundinamarca.edu.co --all --query "[].{rol:roleDefinitionName, ambito:scope}" -o table
az group list --query "[].name" -o tsv     # debe listar rg-pitboss-erp
```

Si los roles no aparecen: `az logout` y `az login`. Si siguen sin aparecer, F9 no está completa; no seguir.

3. Variables que se usan en todos los pasos:

```powershell
$env:ARM_SUBSCRIPTION_ID = "12f1bc21-94a3-4b28-9a90-8224d1e5fcc8"
$sub = $env:ARM_SUBSCRIPTION_ID
$rg  = "rg-pitboss-erp"
```

## 1. J2-B: crear el backend del state (bootstrap)

```powershell
terraform -chdir=infra/terraform/bootstrap init
terraform -chdir=infra/terraform/bootstrap plan
terraform -chdir=infra/terraform/bootstrap apply
```

- **Esperado:** `Apply complete! Resources: 4 added` (el grupo, el sufijo aleatorio, la cuenta y el contenedor).
- Anotar los outputs: `terraform -chdir=infra/terraform/bootstrap output`. El nombre de la cuenta es `stpitbosstf` más 6 caracteres.
- **El `terraform.tfstate` del bootstrap queda local** en `infra/terraform/bootstrap/` y no se sube (está en `.gitignore`). Copiarlo a un lugar seguro y anotar dónde en la bitácora. Si se pierde, la cuenta se puede importar de nuevo.

Darse acceso al contenedor del state (J2.3):

```powershell
$yo = az ad signed-in-user show --query id -o tsv
$sa = terraform -chdir=infra/terraform/bootstrap output -raw storage_account_name
$said = az storage account show -n $sa -g rg-pitboss-tfstate --query id -o tsv
az role assignment create --assignee-object-id $yo --assignee-principal-type User --role "Storage Blob Data Contributor" --scope $said
```

Los roles tardan unos minutos en propagarse: si el `init` del paso siguiente da 403, esperar y repetir.

## 2. J3-B: importar la infraestructura existente

### 2.1 Backend y primer `init`

```powershell
Copy-Item infra/terraform/backend.hcl.example infra/terraform/backend.hcl
# editar backend.hcl: poner el nombre real de la cuenta (valor de $sa)
terraform -chdir=infra/terraform init -backend-config=backend.hcl
```

### 2.2 Descubrir los valores reales

```powershell
az appservice plan list -g $rg --query "[].{nombre:name, sku:sku.name}" -o table
az sql server ad-admin list -g $rg -s pitboss-sql-8szxa -o json
az sql server firewall-rule list -g $rg -s pitboss-sql-8szxa --query "[].{nombre:name, desde:startIpAddress, hasta:endIpAddress}" -o table
az webapp config appsettings list -g $rg -n pitboss-erp-8szxa --query "[].name" -o tsv
```

Con eso se completa un archivo **local** `infra/terraform/local.tfvars` (está ignorado por Git, `*.tfvars`):

```hcl
nombre_plan         = "<nombre real del plan>"
admin_sql_login     = "<login del administrador Entra>"
admin_sql_object_id = "<object id del administrador Entra>"
reglas_firewall = {
  "<nombre de la regla>" = { ip_inicial = "<desde>", ip_final = "<hasta>" }
}
```

Importante: la regla "Permitir servicios de Azure" (`0.0.0.0` a `0.0.0.0`) cuenta como una regla más y debe importarse si existe.

### 2.3 Importar cada recurso

```powershell
$base = "/subscriptions/$sub/resourceGroups/$rg"
$tf = "-chdir=infra/terraform"
terraform $tf import -var-file=local.tfvars azurerm_resource_group.principal $base
terraform $tf import -var-file=local.tfvars azurerm_service_plan.principal "$base/providers/Microsoft.Web/serverFarms/<nombre del plan>"
terraform $tf import -var-file=local.tfvars azurerm_linux_web_app.erp "$base/providers/Microsoft.Web/sites/pitboss-erp-8szxa"
terraform $tf import -var-file=local.tfvars azurerm_mssql_server.principal "$base/providers/Microsoft.Sql/servers/pitboss-sql-8szxa"
terraform $tf import -var-file=local.tfvars azurerm_mssql_database.erp "$base/providers/Microsoft.Sql/servers/pitboss-sql-8szxa/databases/CasinoErp"
```

Una línea por cada regla de firewall (la clave entre comillas es el nombre de la regla; en PowerShell se usa comilla simple por fuera):

```powershell
terraform $tf import -var-file=local.tfvars 'azurerm_mssql_firewall_rule.regla["<nombre>"]' "$base/providers/Microsoft.Sql/servers/pitboss-sql-8szxa/firewallRules/<nombre>"
```

### 2.4 Llevar el plan a cero diferencias

```powershell
terraform $tf plan -var-file=local.tfvars
```

**Diferencias que se esperan (y solo estas):**
- Se **crea** `random_password.jwt`.
- Se **agregan** los app settings `Jwt__Clave`, `Seguridad__CodigoTotpDemo` y `BaseDatos__RecrearAlIniciar`.

**Diferencias que NO deben quedar** (se ajustan en `main.tf` hasta que desaparezcan):

| Síntoma en el plan | Causa probable | Qué hacer |
|---|---|---|
| Un app setting existente aparece como `-` (se borraría) | Terraform reemplaza **todos** los app settings; hay uno creado a mano que no está en `main.tf` (por ejemplo `WEBSITE_RUN_FROM_PACKAGE`) | Comparar con la lista del paso 2.2 y agregar el que falte en `app_settings` |
| La base `CasinoErp` aparece para reemplazar (`-/+`) | `sku_name`, `min_capacity` u otro atributo no coincide | **No aplicar.** `prevent_destroy` lo bloqueará; ajustar `ignore_changes` o el valor, y repetir |
| `azuread_administrator` cambia | `login_username` u `object_id` no son los reales | Corregir el `local.tfvars` |
| `site_config` cambia | `ftps_state`, `minimum_tls_version` u otro valor distinto del predeterminado | Copiarlo desde `az webapp show` al bloque `site_config` |
| `https_only` o `identity` cambian | Distinto al estado real | Igualar con `az webapp show -g $rg -n pitboss-erp-8szxa` |

**No ejecutar `apply` en esta etapa**: eso es J6. Cuando el plan solo muestre las diferencias esperadas, guardar el resultado en la bitácora.

### 2.5 Verificación final de J3

```powershell
terraform -chdir=infra/terraform fmt -check -recursive
terraform -chdir=infra/terraform validate
```

Si hubo cambios en `main.tf` o `variables.tf`, hacer commit en la rama. Si se agregaron valores reales que no deben ir al repositorio, ponerlos solo en `local.tfvars`.

## 3. J4-B: identidad OIDC y prueba del workflow

El workflow necesita una identidad en Azure con una credencial federada para `repo:camilo1503/Casino-ERP-Repositorio:pull_request`.

### 3.1 Opción A: App Registration (la del plan)

```powershell
$app = az ad app create --display-name "github-pitboss-terraform" --query appId -o tsv
az ad sp create --id $app
$cred = @{ name="github-pull-request"; issuer="https://token.actions.githubusercontent.com"; subject="repo:camilo1503/Casino-ERP-Repositorio:pull_request"; audiences=@("api://AzureADTokenExchange") } | ConvertTo-Json -Compress
$cred | Set-Content cred.json -Encoding utf8
az ad app federated-credential create --id $app --parameters "@cred.json"
Remove-Item cred.json
```

**Riesgo:** estás en el tenant de la Universidad de Cundinamarca; si el tenant no permite a los estudiantes registrar aplicaciones, `az ad app create` responderá `Insufficient privileges`. En ese caso, usar la opción B (no necesita permisos de Entra ID) o pedirle a Felipe que cree la App Registration.

### 3.2 Opción B: identidad administrada asignada por el usuario (no requiere permisos de Entra ID)

```powershell
az identity create -g $rg -n id-github-pitboss-terraform --query "{cliente:clientId, principal:principalId}" -o json
az identity federated-credential create -g $rg --identity-name id-github-pitboss-terraform -n github-pull-request `
  --issuer "https://token.actions.githubusercontent.com" `
  --subject "repo:camilo1503/Casino-ERP-Repositorio:pull_request" `
  --audiences "api://AzureADTokenExchange"
```

En ambos casos, `AZURE_CLIENT_ID` es el `clientId` de la identidad (el `appId` en la opción A).

### 3.3 Roles de la identidad

```powershell
$principal = "<principalId de la identidad>"   # en la opción A: az ad sp show --id $app --query id -o tsv
az role assignment create --assignee-object-id $principal --assignee-principal-type ServicePrincipal --role Contributor --scope "/subscriptions/$sub/resourceGroups/$rg"
az role assignment create --assignee-object-id $principal --assignee-principal-type ServicePrincipal --role "Storage Blob Data Contributor" --scope $said
```

### 3.4 Secretos del repositorio

Los carga quien administre el repositorio (F9.6 de Felipe, o Jorge si tiene permisos en GitHub). Son identificadores, no contraseñas:

| Secreto | Valor |
|---|---|
| `AZURE_CLIENT_ID` | `clientId` de la identidad |
| `AZURE_TENANT_ID` | `az account show --query tenantId -o tsv` |
| `AZURE_SUBSCRIPTION_ID` | `12f1bc21-94a3-4b28-9a90-8224d1e5fcc8` |
| `TFSTATE_RG` | `rg-pitboss-tfstate` |
| `TFSTATE_SA` | el valor de `$sa` |

Con `gh`: `gh secret set AZURE_CLIENT_ID --body "<valor>"` (y así con cada uno), si la cuenta tiene permiso de administración del repositorio.

### 3.5 Probar el workflow (J4.3)

1. Hay que fusionar antes `feature/HU-07-bootstrap-state` en `develop`, para que el workflow exista en la rama base.
2. Crear una rama desde `develop` que cambie un comentario en `infra/terraform/variables.tf` y abrir un PR hacia `develop`.
3. **Esperado:** el check **Terraform plan** en verde y el resumen con las diferencias que quedaron pendientes (solo las de J6) o `No changes.`.
4. Guardar una captura en `docs/sprint-2/evidencias/terraform-plan-pr.png` y completar `investigacion-terraform-actions.md`.

Si el job falla al iniciar sesión (`AADSTS70021`, `No matching federated identity record`), comprobar que el `subject` de la credencial es exactamente `repo:camilo1503/Casino-ERP-Repositorio:pull_request`.

## 4. Al terminar

- Anotar en la bitácora, en una entrada nueva: la hora, qué se importó, el resultado del `plan` y dónde está el `tfstate` del bootstrap.
- Actualizar `docs/arc42/07-vista-despliegue.md` y `ADR-003`: ya no es "escrito, no aplicado".
- Pasar J2, J3 y J4 a "Hecha" en la tabla de la bitácora y en el tablero, y mover CER-49, CER-50 y CER-51 en Jira.
- Fusionar `feature/HU-07-bootstrap-state` con el CI en verde (D-009).
- Dejar J6 (despliegue) para cuando estén en `develop` los PR de Iván y Camilo.
