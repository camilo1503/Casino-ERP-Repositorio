package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.assertEquals;

import io.cucumber.java.es.Dado;
import io.cucumber.java.es.Entonces;

public class ComunesSteps {
    private final Contexto ctx;

    public ComunesSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    @Dado("que el usuario {string} con rol {string} ha iniciado sesión")
    public void usuarioAutenticado(String nombre, String rol) {
        ctx.usuario = nombre + " (" + rol + ")";
    }

    @Entonces("el sistema muestra el mensaje {string}")
    public void elSistemaMuestraMensaje(String mensaje) {
        assertEquals(mensaje, ctx.mensaje);
    }

    @Entonces("el sistema confirma {string}")
    public void elSistemaConfirma(String mensaje) {
        assertEquals(mensaje, ctx.mensaje);
    }

    @Entonces("el sistema responde {string}")
    public void elSistemaResponde(String mensaje) {
        assertEquals(mensaje, ctx.mensaje);
    }
}
