package co.pitboss.erp.bdd;

import static org.junit.jupiter.api.Assertions.*;

import co.pitboss.erp.inventario.Ficha;
import co.pitboss.erp.inventario.InventarioFichasService;
import co.pitboss.erp.inventario.Movimiento;
import io.cucumber.java.es.*;
import java.util.List;

/** Taller 4: inventario de fichas. Responsable: Felipe Antonio Cortés Hernández */
public class InventarioSteps {
    private final Contexto ctx;
    private String codigo;

    public InventarioSteps(Contexto ctx) {
        this.ctx = ctx;
    }

    @Dado("que no existe una ficha con código {string}")
    public void noExisteFicha(String cod) {
        assertFalse(ctx.repoFichas.existsByCode(cod));
    }

    @Dado("que la ficha {string} tiene stock {int}")
    public void fichaConStock(String cod, int stock) {
        ctx.repoFichas.save(new Ficha(cod, 25000, "Verde", stock));
        codigo = cod;
    }

    @Cuando("registro la ficha {string} de {long} pesos, categoría {string} y stock inicial {int}")
    public void registroFicha(String cod, long denominacion, String categoria, int stock) {
        codigo = cod;
        ctx.intentar(() -> ctx.inventario.registrar(new Ficha(cod, denominacion, categoria, stock)), f -> InventarioFichasService.REGISTRADA);
    }

    @Cuando("se registra una entrada de {int} fichas")
    public void entrada(int cantidad) {
        ctx.intentar(() -> ctx.inventario.registrarEntrada(codigo, cantidad), f -> InventarioFichasService.ENTRADA);
    }

    @Cuando("se retiran {int} fichas")
    public void retiro(int cantidad) {
        ctx.intentar(() -> ctx.inventario.retirar(codigo, cantidad), f -> InventarioFichasService.RETIRO);
    }

    @Entonces("el stock final es {int}")
    public void stockFinal(int esperado) {
        assertEquals(esperado, ctx.repoFichas.findByCode(codigo).orElseThrow().getStock());
    }

    @Y("el movimiento queda registrado en el historial")
    public void movimientoRegistrado() {
        List<Movimiento> movimientos = ctx.repoFichas.movements();
        assertFalse(movimientos.isEmpty());
        assertEquals(codigo, movimientos.get(movimientos.size() - 1).codigo());
    }
}
