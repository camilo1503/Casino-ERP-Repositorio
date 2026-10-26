package co.pitboss.erp.perfiles;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.comun.ReglaNegocioException;
import java.time.Clock;
import java.time.LocalDate;
import java.time.ZoneId;
import org.junit.jupiter.api.*;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

/** Plantilla de la sección 2 (validación de proveedor) aplicada a perfiles de jugador. Responsable: Daniel Camilo Ruiz Rodríguez */
@DisplayName("Pruebas del servicio de perfiles de jugador")
class JugadorServiceTest {
    private static final Clock HOY = Clock.fixed(LocalDate.of(2026, 10, 26).atStartOfDay(ZoneId.of("America/Bogota")).toInstant(), ZoneId.of("America/Bogota"));
    private JugadorService service;

    @BeforeEach
    void setUp() {
        service = new JugadorService(new InMemoryJugadorRepository(), HOY);
    }

    private static Jugador jugador(String documento, String correo, LocalDate nacimiento) {
        return new Jugador(documento, "María Fernanda López", correo, nacimiento);
    }

    @Test
    @DisplayName("Debe crear un jugador con documento válido en estado ACTIVO")
    void deberiaCrearJugadorConDocumentoValido() {
        Jugador creado = service.registrar(jugador("1020456789", "Maria.Lopez@correo.com", LocalDate.of(1990, 5, 14)));
        assertEquals("ACTIVO", creado.getEstado());
        assertEquals("maria.lopez@correo.com", creado.getCorreo());
    }

    @Test
    @DisplayName("Debe rechazar documento duplicado")
    void noDeberiaCrearJugadorConDocumentoDuplicado() {
        service.registrar(jugador("1020456789", "a@correo.com", LocalDate.of(1990, 5, 14)));
        Jugador duplicado = jugador("1020456789", "b@correo.com", LocalDate.of(1988, 1, 10));
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.registrar(duplicado));
        assertEquals("El documento ya está registrado", e.getMessage());
    }

    @Test
    @DisplayName("Debe rechazar a un menor de 18 años")
    void noDeberiaRegistrarMenorDeEdad() {
        Jugador menor = jugador("1099000111", "pedro@correo.com", LocalDate.of(2010, 3, 1));
        assertThrows(ReglaNegocioException.class, () -> service.registrar(menor));
    }

    @ParameterizedTest
    @ValueSource(strings = {"", " ", "sin-arroba.com", "usuario@"})
    @DisplayName("Debe rechazar correos inválidos")
    void noDeberiaAceptarCorreosInvalidos(String correo) {
        Jugador j = jugador("1030000001", correo, LocalDate.of(1995, 1, 1));
        ReglaNegocioException e = assertThrows(ReglaNegocioException.class, () -> service.registrar(j));
        assertEquals("Formato de correo inválido", e.getMessage());
    }

    @Test
    @DisplayName("Debe desactivar con motivo sin borrar el registro")
    void deberiaDesactivarConMotivo() {
        service.registrar(jugador("1020456789", "a@correo.com", LocalDate.of(1990, 5, 14)));
        Jugador j = service.desactivar("1020456789", "Solicitud del titular");
        assertEquals("INACTIVO", j.getEstado());
        assertNotNull(service.consultar("1020456789"));
    }

    @Test
    @DisplayName("Debe exigir motivo para desactivar")
    void noDeberiaDesactivarSinMotivo() {
        assertThrows(ReglaNegocioException.class, () -> service.desactivar("1020456789", " "));
    }
}
