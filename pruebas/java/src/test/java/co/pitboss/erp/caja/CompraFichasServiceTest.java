package co.pitboss.erp.caja;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.ReglaNegocioException;
import org.junit.jupiter.api.*;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

/** Ejercicio 2: total de una compra de fichas = denominación x cantidad. Responsable: Iván Felipe León Pinilla */
@DisplayName("Ejercicio 2 · Total de la compra de fichas")
class CompraFichasServiceTest {
    private CompraFichasService service;

    @BeforeEach
    void setUp() {
        service = new CompraFichasService(new InMemoryCompraRepository());
    }

    @ParameterizedTest(name = "{0} x {1} = {2}")
    @CsvSource({
        "25000,  4, 100000",
        "100000, 3, 300000",
        "5000,  20, 100000",
        "25000,  0, ERROR",
        "50000, -1, ERROR"
    })
    @DisplayName("Debe calcular el total o rechazar cantidades no positivas")
    void deberiaCalcularTotal(long denominacion, int cantidad, String esperado) {
        if ("ERROR".equals(esperado)) {
            ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.calcularTotal(denominacion, cantidad));
            assertEquals("La cantidad debe ser mayor que cero", e.getMessage());
        } else {
            assertEquals(Long.parseLong(esperado), service.calcularTotal(denominacion, cantidad));
        }
    }

    @Test
    @DisplayName("Debe rechazar una denominación que no existe")
    void noDeberiaAceptarDenominacionInexistente() {
        assertThrows(ReglaNegocioException.class, () -> service.calcularTotal(7000, 1));
    }

    @ParameterizedTest(name = "compra de {0} queda {1}")
    @CsvSource({"100000, APROBADA", "4999999, APROBADA", "5000000, EN_REVISION", "8750000, EN_REVISION"})
    @DisplayName("Debe enviar a revisión las compras sobre el umbral legal")
    void deberiaClasificarPorUmbral(long monto, String estado) {
        assertEquals(estado, service.registrarCompra("1020456789", monto).estado());
    }

    @Test
    @DisplayName("Debe rechazar una compra con monto cero")
    void noDeberiaRegistrarMontoCero() {
        InMemoryCompraRepository repo = new InMemoryCompraRepository();
        CompraFichasService s = new CompraFichasService(repo);
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> s.registrarCompra("1020456789", 0));
        assertEquals("El monto debe ser mayor que cero", e.getMessage());
        assertTrue(repo.findAll().isEmpty());
    }
}
