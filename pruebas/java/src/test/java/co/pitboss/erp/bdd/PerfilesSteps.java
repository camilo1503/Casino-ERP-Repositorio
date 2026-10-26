package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.perfiles.Jugador;
import co.pitboss.erp.perfiles.JugadorService;
import io.cucumber.java.es.*;
import java.time.LocalDate;

public class PerfilesSteps {
    private final Contexto ctx;
    private Jugador jugador;

    public PerfilesSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    @Dado("que no existe un jugador con documento {string}")
    public void noExisteJugador(String documento) {
        assertFalse(ctx.repoJugadores.existsByDocument(documento));
    }

    @Dado("que ya existe un jugador con documento {string}")
    public void yaExisteJugador(String documento) {
        Jugador j = new Jugador(documento, "María Fernanda López", "maria.lopez@correo.com", LocalDate.of(1990, 5, 14));
        j.setPuntos(8450);
        ctx.repoJugadores.save(j);
    }

    @Cuando("registro al jugador con documento {string}, nombre {string}, correo {string} y fecha de nacimiento {string}")
    public void registroJugador(String documento, String nombre, String correo, String fecha) {
        jugador = ctx.intentar(() -> ctx.jugadores.registrar(new Jugador(documento, nombre, correo, LocalDate.parse(fecha))), j -> JugadorService.REGISTRADO);
    }

    @Cuando("registro un jugador con correo {string}")
    public void registroJugadorConCorreo(String correo) {
        ctx.intentar(() -> ctx.jugadores.registrar(new Jugador("1030000001", "Jugador de Prueba", correo, LocalDate.of(1995, 1, 1))), j -> "Correo válido");
    }

    @Cuando("desactivo al jugador con documento {string} por el motivo {string}")
    public void desactivo(String documento, String motivo) {
        ctx.intentar(() -> ctx.jugadores.desactivar(documento, motivo), j -> JugadorService.DESACTIVADO);
    }

    @Entonces("el jugador se crea con estado {string}")
    public void seCreaConEstado(String estado) {
        assertNotNull(jugador);
        assertEquals(estado, jugador.getEstado());
    }

    @Entonces("el jugador con documento {string} queda en estado {string}")
    public void quedaEnEstado(String documento, String estado) {
        assertEquals(estado, ctx.repoJugadores.findByDocument(documento).orElseThrow().getEstado());
    }

    @Entonces("el jugador con documento {string} conserva el nombre {string}")
    public void conservaNombre(String documento, String nombre) {
        assertEquals(nombre, ctx.repoJugadores.findByDocument(documento).orElseThrow().getNombre());
    }
}
