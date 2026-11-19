variable "ubicacion" {
  description = "Región de Azure de todos los recursos."
  type        = string
  default     = "brazilsouth"
}

variable "nombre_app" {
  description = "Nombre de la App Service (también define el subdominio)."
  type        = string
  default     = "pitboss-erp-8szxa"
}

variable "nombre_plan" {
  description = "Nombre del plan de App Service F1."
  type        = string
  default     = "plan-pitboss-erp"
}

variable "nombre_sql" {
  description = "Nombre del servidor Azure SQL."
  type        = string
  default     = "pitboss-sql-8szxa"
}

variable "admin_sql_login" {
  description = "Cuenta de Microsoft Entra ID que administra el servidor SQL (correo o nombre del grupo). Se confirma al importar (J3.5)."
  type        = string
  default     = ""
}

variable "admin_sql_object_id" {
  description = "Object ID de esa cuenta o grupo en Entra ID. Se confirma al importar (J3.5)."
  type        = string
  default     = ""
}

variable "reglas_firewall" {
  description = "Reglas de firewall del servidor SQL: nombre => rango de IP. Se completan con `az sql server firewall-rule list` (J3.5)."
  type = map(object({
    ip_inicial = string
    ip_final   = string
  }))
  default = {}
}

variable "codigo_totp_demo" {
  description = "Código TOTP de demostración. Vacío = desactivado. Solo para la Sprint Review."
  type        = string
  default     = ""
  sensitive   = true
}

variable "recrear_base_al_iniciar" {
  description = "Si es true, la API borra y vuelve a crear la base al arrancar (D-006). Solo para el despliegue del Sprint 2."
  type        = bool
  default     = false
}
