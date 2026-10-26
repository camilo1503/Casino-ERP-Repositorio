# language: es
# Plantilla CRUD en Gherkin aplicada a E-01 Gestión de Perfiles de Jugador
# Responsable: Daniel Camilo Ruiz Rodríguez
Característica: Gestión de perfiles de jugador
  Como anfitrión de jugadores
  Quiero registrar, consultar, actualizar y desactivar perfiles de jugador
  Para atender a cada jugador según su nivel de lealtad

  Antecedentes:
    Dado que el usuario "Sebastián Prada" con rol "Anfitrión de jugadores" ha iniciado sesión

  Escenario: Registrar un jugador válido
    Dado que no existe un jugador con documento "1020456789"
    Cuando registro al jugador con documento "1020456789", nombre "María Fernanda López", correo "maria.lopez@correo.com" y fecha de nacimiento "1990-05-14"
    Entonces el jugador se crea con estado "ACTIVO"
    Y el sistema muestra el mensaje "Jugador registrado exitosamente"

  Escenario: Rechazar jugador con documento duplicado
    Dado que ya existe un jugador con documento "1020456789"
    Cuando registro al jugador con documento "1020456789", nombre "Otra Persona", correo "otra@correo.com" y fecha de nacimiento "1988-01-10"
    Entonces el sistema muestra el mensaje "El documento ya está registrado"
    Y el jugador con documento "1020456789" conserva el nombre "María Fernanda López"

  Escenario: Rechazar jugador menor de edad
    Cuando registro al jugador con documento "1099000111", nombre "Pedro Gil", correo "pedro.gil@correo.com" y fecha de nacimiento "2010-03-01"
    Entonces el sistema muestra el mensaje "El jugador debe ser mayor de 18 años"

  Esquema del escenario: Validar formatos de correo
    Cuando registro un jugador con correo "<correo>"
    Entonces el sistema responde "<resultado>"

    Ejemplos:
      | correo                 | resultado                  |
      | maria.lopez@correo.com | Correo válido              |
      | sin-arroba.com         | Formato de correo inválido |
      | usuario@               | Formato de correo inválido |

  Escenario: Desactivar un jugador con motivo (borrado lógico)
    Dado que ya existe un jugador con documento "1020456789"
    Cuando desactivo al jugador con documento "1020456789" por el motivo "Solicitud del titular"
    Entonces el jugador con documento "1020456789" queda en estado "INACTIVO"
    Y el sistema muestra el mensaje "Jugador desactivado"

  Escenario: No desactivar un jugador sin motivo
    Dado que ya existe un jugador con documento "1020456789"
    Cuando desactivo al jugador con documento "1020456789" por el motivo ""
    Entonces el sistema muestra el mensaje "El motivo es obligatorio"
    Y el jugador con documento "1020456789" queda en estado "ACTIVO"
