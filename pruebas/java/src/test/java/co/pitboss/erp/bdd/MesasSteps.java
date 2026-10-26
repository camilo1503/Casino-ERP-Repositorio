package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.mesas.Mesa;
import co.pitboss.erp.mesas.MesaService;
import io.cucumber.java.es.*;

/** Taller 1 y Ejercicio 1 (pasos del retiro de mesas). Responsable: Daniel Camilo Ruiz Rodríguez */
public class MesasSteps {
    private final Contexto ctx;

    public MesasSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    @Dado("que no existe una mesa con número {string}")
    public void noExisteMesa(String numero) {
        assertFalse(ctx.repoMesas.existsByNumber(numero));
    }

    @Dado("que existe la mesa {string} en estado {string}")
    public void existeMesa(String numero, String estado) {
        Mesa mesa = new Mesa(numero, "Blackjack", 25000, 500000);
        mesa.setEstado(estado);
        ctx.repoMesas.save(mesa);
    }

    @Cuando("registro la mesa {string} de {string} con apuesta mínima {long} y máxima {long}")
    public void registroMesa(String numero, String juego, long minima, long maxima) {
        ctx.intentar(() -> ctx.mesas.registrar(new Mesa(numero, juego, minima, maxima)), m -> MesaService.REGISTRADA);
    }

    @Cuando("actualizo los límites de la mesa {string} a mínima {long} y máxima {long}")
    public void actualizoLimites(String numero, long minima, long maxima) {
        ctx.intentar(() -> ctx.mesas.actualizarLimites(numero, minima, maxima), m -> MesaService.ACTUALIZADA);
    }

    @Cuando("solicito retirar la mesa {string} por el motivo {string}")
    public void solicitoRetirar(String numero, String motivo) {
        ctx.intentar(() -> ctx.mesas.retirar(numero, motivo), m -> MesaService.RETIRADA);
    }

    @Entonces("la mesa {string} queda en estado {string}")
    public void mesaQuedaEnEstado(String numero, String estado) {
        assertEquals(estado, ctx.repoMesas.findByNumber(numero).orElseThrow().getEstado());
    }
}
