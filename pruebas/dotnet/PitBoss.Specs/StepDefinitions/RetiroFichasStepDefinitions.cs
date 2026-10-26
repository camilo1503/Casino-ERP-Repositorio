using FluentAssertions;
using PitBoss.Specs.Dominio;
using Reqnroll;

namespace PitBoss.Specs.StepDefinitions;

/// <summary>
/// Ejercicio 5: al retirar más unidades de las disponibles se lanza InsufficientStockException
/// y el stock no cambia. Responsable: Felipe Antonio Cortés Hernández.
/// </summary>
[Binding]
public class RetiroFichasStepDefinitions
{
    private readonly InventarioFichasService _service = new(new InMemoryFichaRepository());
    private Exception? _excepcion;

    [Given("que existe la ficha {string} con stock {int}")]
    public void DadoQueExisteLaFicha(string codigo, int stock)
    {
        _service.Registrar(new Ficha(codigo, 25000, "Verde", stock));
    }

    [When("intento retirar {int} fichas de {string}")]
    public void CuandoIntentoRetirar(int cantidad, string codigo)
    {
        try
        {
            _service.Retirar(codigo, cantidad);
        }
        catch (Exception e)
        {
            _excepcion = e;
        }
    }

    [Then("se lanza InsufficientStockException con el mensaje {string}")]
    public void EntoncesSeLanzaInsufficientStockException(string mensaje)
    {
        _excepcion.Should().BeOfType<InsufficientStockException>();
        _excepcion!.Message.Should().Be(mensaje);
    }

    [Then("no se lanza ninguna excepción")]
    public void EntoncesNoSeLanzaExcepcion()
    {
        _excepcion.Should().BeNull();
    }

    [Then("el stock de {string} es {int}")]
    public void EntoncesElStockEs(string codigo, int esperado)
    {
        _service.Consultar(codigo).Stock.Should().Be(esperado);
    }
}
