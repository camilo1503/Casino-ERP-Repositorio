package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.Dinero;
import co.pitboss.erp.perfiles.Jugador;
import co.pitboss.erp.retencion.OfertaRetencion;
import co.pitboss.erp.retencion.RetencionService;
import io.cucumber.java.es.*;
import java.time.LocalDate;
import java.util.Map;

/** Taller 2: ofertas de retención. Responsable: Jorge Alcides Díaz Lozano */
public class RetencionSteps {
    private static final Map<String, Integer> PUNTOS = Map.of("1020456789", 8450, "1018334560", 3200, "80554219", 2700);
    private final Contexto ctx;
    private String documento;
    private int minutosSesion;
    private OfertaRetencion oferta;

    public RetencionSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    private void asegurarJugador(String doc, String estado) {
        Jugador j = new Jugador(doc, "Jugador " + doc, doc + "@correo.com", LocalDate.of(1990, 1, 1));
        j.setPuntos(PUNTOS.getOrDefault(doc, 1000));
        j.setEstado(estado);
        ctx.repoJugadores.save(j);
        documento = doc;
    }

    @Dado("que el jugador {string} tiene una sesión activa de {int} minutos")
    public void sesionActiva(String doc, int minutos) {
        asegurarJugador(doc, "ACTIVO");
        minutosSesion = minutos;
    }

    @Dado("que el jugador {string} está en estado {string}")
    public void jugadorEnEstado(String doc, String estado) {
        asegurarJugador(doc, estado);
    }

    @Y("tiene una sesión activa de {int} minutos")
    public void tieneSesion(int minutos) {
        minutosSesion = minutos;
    }

    @Y("ya recibió una oferta de retención hoy")
    public void yaRecibioOferta() {
        LocalDate hoy = LocalDate.now(ctx.reloj);
        ctx.repoOfertas.save(new OfertaRetencion("RET-1021", documento, "Quiere retirarse", "FICHAS_BONO", 50000, hoy, hoy.plusDays(30), "EMITIDA"));
    }

    @Cuando("aplico el descuento de retención a una compra de {long} pesos")
    public void aplicoDescuento(long monto) {
        ctx.intentar(() -> ctx.retencion.aplicarDescuento(documento, monto), d -> "Descuento de " + Dinero.pesos(d));
    }

    @Cuando("el crupier registra la señal {string}")
    public void registraSenal(String senal) {
        Jugador jugador = ctx.repoJugadores.findByDocument(documento).orElse(null);
        oferta = ctx.intentar(() -> ctx.retencion.evaluarSenal(jugador, minutosSesion, senal), o -> RetencionService.EMITIDA);
    }

    @Entonces("se emite una oferta de tipo {string}")
    public void seEmiteOferta(String tipo) {
        assertNotNull(oferta);
        assertEquals(tipo, oferta.tipo());
    }

    @Entonces("no se emite ninguna oferta")
    public void noSeEmiteOferta() {
        assertNull(oferta);
        assertTrue(ctx.repoOfertas.findByDocument(documento).isEmpty());
    }
}
