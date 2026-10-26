package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.caja.Compra;
import co.pitboss.erp.caja.CompraFichasService;
import io.cucumber.java.es.*;

/**
 * Ejercicio 4: "Rechazar compra con monto cero".
 * El paso @Cuando captura la excepción (a través de Contexto.intentar) y el @Entonces verifica el mensaje.
 * Responsable: Iván Felipe León Pinilla
 */
public class CajaSteps {
    private final Contexto ctx;
    private Compra compra;

    public CajaSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    @Cuando("registro una compra de fichas por {long} pesos para el jugador {string}")
    public void registroCompra(long monto, String documento) {
        compra = ctx.intentar(() -> ctx.caja.registrarCompra(documento, monto), CompraFichasService::mensajePara);
    }

    @Entonces("la compra queda en estado {string}")
    public void compraQuedaEnEstado(String estado) {
        assertNotNull(compra);
        assertEquals(estado, compra.estado());
    }

    @Entonces("no se registra ninguna compra")
    public void noSeRegistraCompra() {
        assertNull(compra);
        assertTrue(ctx.repoCompras.findAll().isEmpty());
    }
}
