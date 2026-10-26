package co.pitboss.erp.retencion;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.ReglaNegocioException;
import co.pitboss.erp.perfiles.Jugador;
import java.time.Clock;
import java.time.LocalDate;
import java.time.ZoneId;
import org.junit.jupiter.api.*;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;
import org.junit.jupiter.params.provider.ValueSource;

/** Taller 2 (JUnit): descuento y reglas de juego responsable. Responsable: Jorge Alcides Díaz Lozano */
@DisplayName("Taller 2 · Ofertas de retención")
class RetencionServiceTest {
    private static final ZoneId BOGOTA = ZoneId.of("America/Bogota");
    private static final Clock HOY = Clock.fixed(LocalDate.of(2026, 10, 26).atStartOfDay(BOGOTA).toInstant(), BOGOTA);
    private InMemoryOfertaRepository repo;
    private RetencionService service;

    @BeforeEach
    void setUp() {
        repo = new InMemoryOfertaRepository();
        service = new RetencionService(repo, HOY);
    }

    private static Jugador jugador(int puntos, String estado) {
        Jugador j = new Jugador("1020456789", "María Fernanda López", "maria@correo.com", LocalDate.of(1990, 5, 14));
        j.setPuntos(puntos);
        j.setEstado(estado);
        return j;
    }

    @ParameterizedTest(name = "compra de {0} -> descuento {1}")
    @DisplayName("Debe aplicar el 20 % con tope de $200.000")
    @CsvSource({"500000, 100000", "2500000, 200000", "1000000, 200000"})
    void deberiaAplicarDescuentoConTope(long monto, long descuento) {
        assertEquals(descuento, service.aplicarDescuento("1020456789", monto));
        assertEquals(1, repo.descuentosGuardados());
    }

    @ParameterizedTest(name = "compra de {0} -> Monto inválido")
    @DisplayName("No debe aplicar descuento con monto cero o negativo")
    @ValueSource(longs = {0, -1000})
    void noDeberiaAplicarDescuentoConMontoInvalido(long monto) {
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.aplicarDescuento("1020456789", monto));
        assertEquals("Monto inválido", e.getMessage());
        assertEquals(0, repo.descuentosGuardados());
    }

    @ParameterizedTest(name = "{0} puntos -> {1}")
    @DisplayName("Debe sugerir la oferta según el nivel de lealtad")
    @CsvSource({"900, DESCUENTO_FICHAS", "3200, DESCUENTO_FICHAS", "8450, FICHAS_BONO", "15200, BONO_RECLAMABLE"})
    void deberiaSugerirOfertaPorNivel(int puntos, String tipo) {
        OfertaRetencion oferta = service.evaluarSenal(jugador(puntos, "ACTIVO"), 95, "Quiere retirarse");
        assertEquals(tipo, oferta.tipo());
        assertEquals(LocalDate.of(2026, 11, 25), oferta.vence());
    }

    @Test
    @DisplayName("No debe ofrecer retención a un jugador autoexcluido")
    void noDeberiaOfrecerAJugadorAutoexcluido() {
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.evaluarSenal(jugador(8450, "AUTOEXCLUIDO"), 60, "Quiere retirarse"));
        assertEquals("No se ofrece retención: juego responsable", e.getMessage());
    }

    @Test
    @DisplayName("No debe ofrecer retención con menos de 30 minutos de sesión")
    void noDeberiaOfrecerConSesionCorta() {
        assertThrows(ReglaNegocioException.class, () -> service.evaluarSenal(jugador(3200, "ACTIVO"), 22, "Quiere retirarse"));
    }

    @Test
    @DisplayName("Debe permitir solo una oferta por jugador al día")
    void soloUnaOfertaPorDia() {
        service.evaluarSenal(jugador(8450, "ACTIVO"), 95, "Quiere retirarse");
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.evaluarSenal(jugador(8450, "ACTIVO"), 120, "Cobro en caja"));
        assertEquals("El jugador ya recibió una oferta hoy", e.getMessage());
    }
}
