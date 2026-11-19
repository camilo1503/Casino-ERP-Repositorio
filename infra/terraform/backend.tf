# El resto de la configuración (grupo de recursos, cuenta y contenedor) se pasa con
# `terraform init -backend-config=backend.hcl`. Ver backend.hcl.example.
terraform {
  backend "azurerm" {
    key              = "pitboss.tfstate"
    use_azuread_auth = true
  }
}
