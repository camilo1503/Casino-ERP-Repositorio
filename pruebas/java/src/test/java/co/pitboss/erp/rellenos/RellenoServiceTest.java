package co.pitboss.erp.rellenos;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.ReglaNegocioException;
import org.junit.jupiter.api.*;

/** Taller 3 (JUnit): aprobación de rellenos de fichas. Responsable: Iván Felipe León Pinilla */
@DisplayName("Taller 3 · Aprobación de rellenos")
class RellenoServiceTest {
    private InMemoryRellenoRepository repo;
    private RellenoService service;

    @BeforeEach
    void setUp() {
        repo = new InMemoryRellenoRepository();
        service = new RellenoService(repo);
    }

    @Test
    @DisplayName("Debe aprobar dentro del saldo y descontar la bóveda")
    void deberiaAprobarDentroDelSaldo() {
        repo.updateVaultBalance(10_000_000);
        repo.save(new Relleno("RF-100", "M-04", 5_000_000, "PENDIENTE"));
        assertEquals("APROBADO", service.aprobar("RF-100", "Daniela Vargas").getEstado());
        assertEquals(5_000_000, repo.getVaultBalance());
    }

    @Test
    @DisplayName("No debe aprobar un relleno que excede el saldo")
    void noDeberiaAprobarSobreElSaldo() {
        repo.updateVaultBalance(3_000_000);
        repo.save(new Relleno("RF-101", "M-12", 5_000_000, "PENDIENTE"));
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.aprobar("RF-101", "Daniela Vargas"));
        assertEquals("Saldo insuficiente en la bóveda", e.getMessage());
        assertEquals(3_000_000, repo.getVaultBalance());
        assertEquals("PENDIENTE", repo.findById("RF-101").orElseThrow().getEstado());
    }

    @Test
    @DisplayName("No debe re-aprobar un relleno aprobado")
    void noDeberiaReaprobar() {
        repo.updateVaultBalance(10_000_000);
        repo.save(new Relleno("RF-102", "M-04", 2_000_000, "APROBADO"));
        assertThrows(ReglaNegocioException.class, () -> service.aprobar("RF-102", "Daniela Vargas"));
        assertEquals(10_000_000, repo.getVaultBalance());
    }
}
