package co.pitboss.erp.inventario;

import co.pitboss.erp.comun.ReglaNegocioException;

public class StockInsuficienteException extends ReglaNegocioException {
    public StockInsuficienteException() {
        super("Stock insuficiente");
    }
}
