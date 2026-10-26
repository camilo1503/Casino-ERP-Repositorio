package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.rellenos.Relleno;
import co.pitboss.erp.rellenos.RellenoService;
import io.cucumber.java.es.*;

/**
 * Taller 3: step definitions de la aprobación de rellenos.
 * El paso @Entonces consulta el saldo actualizado de la bóveda y lo compara con el valor esperado.
 * Responsable: Iván Felipe León Pinilla
 */
public class RellenosSteps {
    private final Contexto ctx;

    public RellenosSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    @Dado("que la bóveda tiene un saldo disponible de {long} pesos")
    public void saldoBoveda(long saldo) {
        ctx.repoRellenos.updateVaultBalance(saldo);
    }

    @Y("existe un relleno {string} para la mesa {string} por {long} pesos en estado {string}")
    public void existeRelleno(String id, String mesa, long monto, String estado) {
        ctx.repoRellenos.save(new Relleno(id, mesa, monto, estado));
    }

    @Cuando("el jefe de piso aprueba el relleno {string}")
    public void apruebaRelleno(String id) {
        ctx.intentar(() -> ctx.rellenos.aprobar(id, ctx.usuario), r -> RellenoService.APROBADO);
    }

    @Entonces("el relleno {string} queda en estado {string}")
    public void rellenoQuedaEnEstado(String id, String estado) {
        assertEquals(estado, ctx.repoRellenos.findById(id).orElseThrow().getEstado());
    }

    @Entonces("el saldo de la bóveda queda en {long} pesos")
    public void saldoQueda(long esperado) {
        assertEquals(esperado, ctx.repoRellenos.getVaultBalance());
    }
}
