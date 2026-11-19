output "url_app" {
  description = "URL pública del ERP."
  value       = "https://${azurerm_linux_web_app.erp.default_hostname}"
}

output "principal_id_app" {
  description = "Identidad administrada de la app (es el usuario que se crea dentro de la base)."
  value       = azurerm_linux_web_app.erp.identity[0].principal_id
}
