# language: es
# Taller 1 (CRUD) y Ejercicio 1, aplicados a E-02 Catálogo de Juegos y Mesas
# Responsable: Daniel Camilo Ruiz Rodríguez
Característica: Catálogo de mesas de juego
  Como jefe de piso
  Quiero registrar, consultar, actualizar y retirar mesas de juego
  Para mantener actualizado el catálogo del piso

  Antecedentes:
    Dado que el usuario "Daniela Vargas" con rol "Jefe de piso" ha iniciado sesión

  Escenario: Registrar una mesa válida
    Dado que no existe una mesa con número "M-19"
    Cuando registro la mesa "M-19" de "Blackjack" con apuesta mínima 25000 y máxima 500000
    Entonces la mesa "M-19" queda en estado "CERRADA"
    Y el sistema muestra el mensaje "Mesa registrada exitosamente"

  Escenario: Rechazar mesa con número duplicado
    Dado que existe la mesa "M-01" en estado "ABIERTA"
    Cuando registro la mesa "M-01" de "Ruleta" con apuesta mínima 10000 y máxima 200000
    Entonces el sistema muestra el mensaje "El número de mesa ya está registrado"

  Escenario: Rechazar límites de apuesta inválidos
    Cuando registro la mesa "M-20" de "Póker" con apuesta mínima 500000 y máxima 100000
    Entonces el sistema muestra el mensaje "La apuesta mínima debe ser menor que la máxima"

  Escenario: Actualizar los límites de una mesa
    Dado que existe la mesa "M-05" en estado "CERRADA"
    Cuando actualizo los límites de la mesa "M-05" a mínima 50000 y máxima 1000000
    Entonces el sistema muestra el mensaje "Mesa actualizada"

  # Ejercicio 1: retirar (eliminar) una mesa que no tiene juego activo
  Escenario: Retirar una mesa que no está abierta
    Dado que existe la mesa "M-13" en estado "CERRADA"
    Cuando solicito retirar la mesa "M-13" por el motivo "Paño dañado sin reparación"
    Entonces la mesa "M-13" queda en estado "RETIRADA"
    Y el sistema confirma "Mesa retirada del catálogo"

  Escenario: No retirar una mesa abierta
    Dado que existe la mesa "M-01" en estado "ABIERTA"
    Cuando solicito retirar la mesa "M-01" por el motivo "Cambio de distribución"
    Entonces el sistema muestra el mensaje "No se puede retirar una mesa abierta"
    Y la mesa "M-01" queda en estado "ABIERTA"
