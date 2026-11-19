data "azurerm_client_config" "actual" {}

resource "azurerm_resource_group" "principal" {
  name     = "rg-pitboss-erp"
  location = var.ubicacion
}

resource "azurerm_service_plan" "principal" {
  name                = var.nombre_plan
  resource_group_name = azurerm_resource_group.principal.name
  location            = azurerm_resource_group.principal.location
  os_type             = "Linux"
  sku_name            = "F1"
}

# Clave de firma del JWT (HU-06). Terraform la genera y la guarda solo en el state y en el app setting.
resource "random_password" "jwt" {
  length  = 48
  special = false
}

resource "azurerm_linux_web_app" "erp" {
  name                = var.nombre_app
  resource_group_name = azurerm_resource_group.principal.name
  location            = azurerm_resource_group.principal.location
  service_plan_id     = azurerm_service_plan.principal.id
  https_only          = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = false # F1 no permite always_on.

    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    BaseDatos__Proveedor         = "SqlServer"
    ConnectionStrings__CasinoErp = "Server=tcp:${azurerm_mssql_server.principal.fully_qualified_domain_name},1433;Database=CasinoErp;Authentication=Active Directory Default;Encrypt=True;"
    TZ                           = "America/Bogota"
    Jwt__Clave                   = random_password.jwt.result
    Seguridad__CodigoTotpDemo    = var.codigo_totp_demo
    BaseDatos__RecrearAlIniciar  = tostring(var.recrear_base_al_iniciar)
  }
}

resource "azurerm_mssql_server" "principal" {
  name                = var.nombre_sql
  resource_group_name = azurerm_resource_group.principal.name
  location            = azurerm_resource_group.principal.location
  version             = "12.0"

  # Sin contraseña SQL: solo Microsoft Entra ID.
  azuread_administrator {
    login_username              = var.admin_sql_login
    object_id                   = var.admin_sql_object_id
    tenant_id                   = data.azurerm_client_config.actual.tenant_id
    azuread_authentication_only = true
  }
}

resource "azurerm_mssql_database" "erp" {
  name      = "CasinoErp"
  server_id = azurerm_mssql_server.principal.id

  # Oferta gratuita de Azure SQL (serverless). El proveedor azurerm 4.81 no tiene
  # argumentos para la marca "oferta gratuita": la base ya existe con ella y solo se
  # importa. NO se debe recrear con Terraform (se perdería la oferta gratuita).
  sku_name                    = "GP_S_Gen5_2"
  min_capacity                = 0.5
  auto_pause_delay_in_minutes = 60

  lifecycle {
    # La oferta gratuita fija estos valores en Azure y el proveedor no puede
    # representarlos; sin esto cada plan propondría cambios falsos. Ajustar con el
    # plan real en J3.5.
    prevent_destroy = true
    ignore_changes = [
      sku_name,
      min_capacity,
      auto_pause_delay_in_minutes,
      max_size_gb,
      storage_account_type,
    ]
  }
}

resource "azurerm_mssql_firewall_rule" "regla" {
  for_each = var.reglas_firewall

  name             = each.key
  server_id        = azurerm_mssql_server.principal.id
  start_ip_address = each.value.ip_inicial
  end_ip_address   = each.value.ip_final
}
