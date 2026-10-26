# language: es
# Taller 3, aplicado a Operación de mesas (aprobación de rellenos de fichas)
# Responsable: Iván Felipe León Pinilla
Característica: Aprobación de rellenos de fichas
  Como jefe de piso
  Quiero aprobar los rellenos de fichas que piden las mesas
  Para que ninguna mesa se quede sin fichas y la bóveda no se descuadre

  Antecedentes:
    Dado que el usuario "Daniela Vargas" con rol "Jefe de piso" ha iniciado sesión

  Escenario: Aprobar relleno dentro del saldo de la bóveda
    Dado que la bóveda tiene un saldo disponible de 10000000 pesos
    Y existe un relleno "RF-100" para la mesa "M-04" por 5000000 pesos en estado "PENDIENTE"
    Cuando el jefe de piso aprueba el relleno "RF-100"
    Entonces el relleno "RF-100" queda en estado "APROBADO"
    Y el saldo de la bóveda queda en 5000000 pesos

  Escenario: Rechazar relleno que excede el saldo de la bóveda
    Dado que la bóveda tiene un saldo disponible de 3000000 pesos
    Y existe un relleno "RF-101" para la mesa "M-12" por 5000000 pesos en estado "PENDIENTE"
    Cuando el jefe de piso aprueba el relleno "RF-101"
    Entonces el sistema muestra el mensaje "Saldo insuficiente en la bóveda"
    Y el saldo de la bóveda queda en 3000000 pesos

  Escenario: No volver a aprobar un relleno aprobado
    Dado que la bóveda tiene un saldo disponible de 10000000 pesos
    Y existe un relleno "RF-102" para la mesa "M-04" por 2000000 pesos en estado "APROBADO"
    Cuando el jefe de piso aprueba el relleno "RF-102"
    Entonces el sistema muestra el mensaje "El relleno ya está aprobado"
    Y el saldo de la bóveda queda en 10000000 pesos
