output "resource_group_name" {
  description = "Grupo de recursos que guarda el state remoto."
  value       = azurerm_resource_group.tfstate.name
}

output "storage_account_name" {
  description = "Cuenta de storage del state (va en backend.hcl)."
  value       = azurerm_storage_account.tfstate.name
}
