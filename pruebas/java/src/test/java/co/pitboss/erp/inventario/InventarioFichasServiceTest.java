package co.pitboss.erp.inventario;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.ReglaNegocioException;
import org.junit.jupiter.api.*;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

/** Taller 4 (JUnit @CsvSource): retiro de fichas de la bóveda. Responsable: Felipe Antonio Cortés Hernández */
@DisplayName("Taller 4 · Inventario de fichas")
class InventarioFichasServiceTest {
    private InMemoryFichaRepository repo;
    private InventarioFichasService service;

    @BeforeEach
    void setUp() {
        repo = new InMemoryFichaRepository();
        service = new InventarioFichasService(repo);
    }

    @ParameterizedTest(name = "{0}: stock {1}, retiro {2} -> {3} ({4})")
    @CsvSource({
        "FICHA-5K,  10, 3, 7, Retiro exitoso",
        "FICHA-10K,  5, 5, 0, Retiro exitoso",
        "FICHA-25K,  2, 5, 2, Stock insuficiente"
    })
    @DisplayName("Debe retirar fichas o rechazar el retiro sin cambiar el stock")
    void deberiaRetirarSegunStock(String codigo, int stock, int retiro, int esperado, String mensaje) {
        service.registrar(new Ficha(codigo, 25000, "Verde", stock));
        if ("Stock insuficiente".equals(mensaje)) {
            StockInsuficienteException e = assertThrows(StockInsuficienteException.class, () -> service.retirar(codigo, retiro));
            assertEquals(mensaje, e.getMessage());
        } else {
            service.retirar(codigo, retiro);
        }
        assertEquals(esperado, service.consultar(codigo).getStock());
    }

    @ParameterizedTest(name = "{0}: {1} + {2} = {3}")
    @CsvSource({"FICHA-5K, 10, 5, 15", "FICHA-10K, 0, 20, 20", "FICHA-25K, 50, 10, 60"})
    @DisplayName("Debe registrar entradas y su movimiento")
    void deberiaRegistrarEntrada(String codigo, int actual, int cantidad, int final_) {
        service.registrar(new Ficha(codigo, 25000, "Verde", actual));
        assertEquals(final_, service.registrarEntrada(codigo, cantidad).getStock());
        assertEquals("ENTRADA", repo.movements().get(0).tipo());
    }

    @Test
    @DisplayName("Debe rechazar stock inicial negativo, categoría vacía y código duplicado")
    void deberiaValidarRegistro() {
        assertThrows(ReglaNegocioException.class, () -> service.registrar(new Ficha("FICHA-1M", 1000000, "Morado", -5)));
        assertThrows(ReglaNegocioException.class, () -> service.registrar(new Ficha("FICHA-2K", 2000, "", 100)));
        service.registrar(new Ficha("FICHA-50K", 50000, "Naranja", 2600));
        assertThrows(ReglaNegocioException.class, () -> service.registrar(new Ficha("FICHA-50K", 50000, "Naranja", 1)));
    }
}
