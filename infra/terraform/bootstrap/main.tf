terraform {
  required_version = ">= 1.6"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }
}

provider "azurerm" {
  features {}
}

# Sufijo para que el nombre de la cuenta de storage sea único en Azure.
resource "random_string" "sufijo" {
  length  = 6
  upper   = false
  special = false
}

resource "azurerm_resource_group" "tfstate" {
  name     = "rg-pitboss-tfstate"
  location = "brazilsouth"
}

resource "azurerm_storage_account" "tfstate" {
  name                            = "stpitbosstf${random_string.sufijo.result}"
  resource_group_name             = azurerm_resource_group.tfstate.name
  location                        = azurerm_resource_group.tfstate.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = false
}

resource "azurerm_storage_container" "tfstate" {
  name                  = "tfstate"
  storage_account_id    = azurerm_storage_account.tfstate.id
  container_access_type = "private"
}
