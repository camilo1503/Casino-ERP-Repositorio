# language: es
# Taller 4, aplicado a Inventario de fichas de la bóveda
# Responsable: Felipe Antonio Cortés Hernández
Característica: Inventario de fichas de la bóveda
  Como cajero de bóveda
  Quiero registrar las fichas y sus movimientos
  Para conocer el stock real por denominación

  Antecedentes:
    Dado que el usuario "Laura Gómez" con rol "Cajero" ha iniciado sesión

  Escenario: Registrar una denominación de ficha
    Dado que no existe una ficha con código "FICHA-50K"
    Cuando registro la ficha "FICHA-50K" de 50000 pesos, categoría "Naranja" y stock inicial 2600
    Entonces el stock final es 2600
    Y el sistema muestra el mensaje "Ficha registrada"

  Escenario: Rechazar stock inicial negativo
    Cuando registro la ficha "FICHA-1M" de 1000000 pesos, categoría "Morado" y stock inicial -5
    Entonces el sistema muestra el mensaje "El stock inicial no puede ser negativo"

  Escenario: Rechazar ficha sin categoría
    Cuando registro la ficha "FICHA-2K" de 2000 pesos, categoría "" y stock inicial 100
    Entonces el sistema muestra el mensaje "La categoría es obligatoria"

  Esquema del escenario: Entrada de fichas a la bóveda
    Dado que la ficha "<codigo>" tiene stock <stock_actual>
    Cuando se registra una entrada de <cantidad> fichas
    Entonces el stock final es <stock_final>
    Y el movimiento queda registrado en el historial

    Ejemplos:
      | codigo    | stock_actual | cantidad | stock_final |
      | FICHA-5K  | 10           | 5        | 15          |
      | FICHA-10K | 0            | 20       | 20          |
      | FICHA-25K | 50           | 10       | 60          |

  Esquema del escenario: Retiro de fichas de la bóveda
    Dado que la ficha "<codigo>" tiene stock <stock>
    Cuando se retiran <retiro> fichas
    Entonces el stock final es <final>
    Y el sistema responde "<mensaje>"

    Ejemplos:
      | codigo    | stock | retiro | final | mensaje            |
      | FICHA-5K  | 10    | 3      | 7     | Retiro exitoso     |
      | FICHA-10K | 5     | 5      | 0     | Retiro exitoso     |
      | FICHA-25K | 2     | 5      | 2     | Stock insuficiente |
