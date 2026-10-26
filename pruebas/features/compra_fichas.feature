# language: es
# Ejercicio 4, aplicado a Caja / POS (compra de fichas)
# Responsable: Iván Felipe León Pinilla
Característica: Compra de fichas en caja
  Como cajero
  Quiero registrar las compras de fichas de los jugadores
  Para que el efectivo y las fichas entregadas queden cuadrados

  Antecedentes:
    Dado que el usuario "Laura Gómez" con rol "Cajero" ha iniciado sesión

  Escenario: Registrar una compra de fichas válida
    Cuando registro una compra de fichas por 100000 pesos para el jugador "1020456789"
    Entonces la compra queda en estado "APROBADA"
    Y el sistema muestra el mensaje "Compra registrada"

  Escenario: Enviar a revisión una compra sobre el umbral legal
    Cuando registro una compra de fichas por 8750000 pesos para el jugador "79456112"
    Entonces la compra queda en estado "EN_REVISION"
    Y el sistema muestra el mensaje "Compra enviada a revisión de cumplimiento"

  # Ejercicio 4: rechazar la operación con monto cero
  Escenario: Rechazar compra con monto cero
    Cuando registro una compra de fichas por 0 pesos para el jugador "1020456789"
    Entonces el sistema muestra el mensaje "El monto debe ser mayor que cero"
    Y no se registra ninguna compra
