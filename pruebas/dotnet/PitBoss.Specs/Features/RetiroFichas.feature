# language: es
# Ejercicio 5 (SpecFlow / Reqnroll en C#). Responsable: Felipe Antonio Cortés Hernández
Característica: Retiro de fichas de la bóveda
  Como cajero de bóveda
  Quiero que el sistema impida retirar más fichas de las disponibles
  Para que el inventario nunca quede negativo

  Escenario: Retiro dentro del stock disponible
    Dado que existe la ficha "FICHA-5K" con stock 10
    Cuando intento retirar 3 fichas de "FICHA-5K"
    Entonces el stock de "FICHA-5K" es 7
    Y no se lanza ninguna excepción

  Escenario: Retirar más fichas de las disponibles
    Dado que existe la ficha "FICHA-25K" con stock 2
    Cuando intento retirar 5 fichas de "FICHA-25K"
    Entonces se lanza InsufficientStockException con el mensaje "Stock insuficiente"
    Y el stock de "FICHA-25K" es 2
