package co.pitboss.erp.mesas;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.ReglaNegocioException;
import java.util.Optional;
import org.junit.jupiter.api.*;

/** Taller 1 (JUnit): no se retira una mesa abierta. Responsable: Daniel Camilo Ruiz Rodríguez */
@DisplayName("Taller 1 · Catálogo de mesas")
class MesaServiceTest {

    /** Doble de prueba (mock manual): devuelve siempre una mesa abierta y cuenta las llamadas a save. */
    static class RepoMesaAbierta implements MesaRepository {
        int llamadasSave = 0;
        @Override public boolean existsByNumber(String numero) { return true; }
        @Override public Optional<Mesa> findByNumber(String numero) {
            Mesa m = new Mesa(numero, "Blackjack", 25000, 500000);
            m.setEstado("ABIERTA");
            return Optional.of(m);
        }
        @Override public Mesa save(Mesa mesa) { llamadasSave++; return mesa; }
    }

    @Test
    @DisplayName("No debe retirar una mesa abierta")
    void noDeberiaRetirarMesaAbierta() {
        RepoMesaAbierta repo = new RepoMesaAbierta();
        MesaService service = new MesaService(repo);
        MesaAbiertaException e = assertThrows(MesaAbiertaException.class, () -> service.retirar("M-01", "Cambio de distribución"));
        assertEquals("No se puede retirar una mesa abierta", e.getMessage());
        assertEquals(0, repo.llamadasSave);
    }

    @Test
    @DisplayName("Debe retirar una mesa cerrada")
    void deberiaRetirarMesaCerrada() {
        MesaService service = new MesaService(new InMemoryMesaRepository());
        service.registrar(new Mesa("M-13", "Póker", 50000, 600000));
        Mesa retirada = service.retirar("M-13", "Paño dañado sin reparación");
        assertEquals("RETIRADA", retirada.getEstado());
    }

    @Test
    @DisplayName("Debe registrar una mesa nueva en estado CERRADA y rechazar el número duplicado")
    void deberiaRegistrarYRechazarDuplicado() {
        MesaService service = new MesaService(new InMemoryMesaRepository());
        assertEquals("CERRADA", service.registrar(new Mesa("M-19", "Blackjack", 25000, 500000)).getEstado());
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.registrar(new Mesa("M-19", "Ruleta", 10000, 200000)));
        assertEquals("El número de mesa ya está registrado", e.getMessage());
    }

    @Test
    @DisplayName("Debe exigir apuesta mínima menor que la máxima")
    void noDeberiaAceptarLimitesInvertidos() {
        MesaService service = new MesaService(new InMemoryMesaRepository());
        assertThrows(ReglaNegocioException.class, () -> service.registrar(new Mesa("M-20", "Póker", 500000, 100000)));
    }
}
