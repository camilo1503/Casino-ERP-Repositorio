package co.pitboss.erp.comun;

/** Regla de negocio incumplida. El mensaje es el que ve el usuario en el ERP. */
public class ReglaNegocioException extends RuntimeException {
    public ReglaNegocioException(String mensaje) {
        super(mensaje);
    }
}
