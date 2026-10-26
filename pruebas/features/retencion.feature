# language: es
# Taller 2, aplicado a E-03 Retención de jugadores (oferta cuando el jugador quiere irse)
# Responsable: Jorge Alcides Díaz Lozano
Característica: Ofertas de retención de jugadores
  Como crupier o cajero
  Quiero que el sistema proponga una oferta cuando un jugador quiere dejar de jugar
  Para mantenerlo en el casino sin dejar de lado el juego responsable

  Antecedentes:
    Dado que el usuario "Ana Torres" con rol "Crupier" ha iniciado sesión

  Esquema del escenario: Validar el monto de la compra con descuento de retención
    Dado que el jugador "1020456789" tiene una sesión activa de 95 minutos
    Cuando aplico el descuento de retención a una compra de <monto> pesos
    Entonces el sistema responde "<resultado>"

    Ejemplos:
      | monto   | resultado             |
      | 500000  | Descuento de $100.000 |
      | 2500000 | Descuento de $200.000 |
      | 0       | Monto inválido        |
      | -1000   | Monto inválido        |

  Escenario: Proponer la oferta sugerida para un jugador Oro
    Dado que el jugador "1020456789" tiene una sesión activa de 95 minutos
    Cuando el crupier registra la señal "Quiere retirarse"
    Entonces se emite una oferta de tipo "FICHAS_BONO"
    Y el sistema muestra el mensaje "Oferta de retención emitida"

  Escenario: Bloquear la oferta a un jugador autoexcluido
    Dado que el jugador "80554219" está en estado "AUTOEXCLUIDO"
    Y tiene una sesión activa de 60 minutos
    Cuando el crupier registra la señal "Quiere retirarse"
    Entonces el sistema muestra el mensaje "No se ofrece retención: juego responsable"
    Y no se emite ninguna oferta

  Escenario: Solo una oferta por jugador al día
    Dado que el jugador "1020456789" tiene una sesión activa de 95 minutos
    Y ya recibió una oferta de retención hoy
    Cuando el crupier registra la señal "Quiere retirarse"
    Entonces el sistema muestra el mensaje "El jugador ya recibió una oferta hoy"

  Escenario: No ofrecer retención con menos de 30 minutos de sesión
    Dado que el jugador "1018334560" tiene una sesión activa de 22 minutos
    Cuando el crupier registra la señal "Quiere retirarse"
    Entonces el sistema muestra el mensaje "La sesión debe durar al menos 30 minutos"
    Y no se emite ninguna oferta
