package co.pitboss.erp.mesas;

import co.pitboss.erp.comun.ReglaNegocioException;

/** Equivale a SupplierHasActiveOrdersException del taller: no se retira una mesa con juego en curso. */
public class MesaAbiertaException extends ReglaNegocioException {
    public MesaAbiertaException() {
        super("No se puede retirar una mesa abierta");
    }
}
