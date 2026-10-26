package co.pitboss.erp.caja;

import co.pitboss.erp.comun.ReglaNegocioException;
import java.util.Set;

/** Caja / POS: total por denominación (Ejercicio 2) y registro de compras de fichas (Ejercicio 4). */
public class CompraFichasService {
    public static final long UMBRAL_CUMPLIMIENTO = 5_000_000L;
    public static final String APROBADA = "Compra registrada";
    public static final String EN_REVISION = "Compra enviada a revisión de cumplimiento";
    private static final Set<Long> DENOMINACIONES = Set.of(5_000L, 10_000L, 25_000L, 50_000L, 100_000L, 500_000L);

    private final CompraRepository repo;
    private int secuencia = 10483;

    public CompraFichasService(CompraRepository repo) {
        this.repo = repo;
    }

    public long calcularTotal(long denominacion, int cantidad) {
        if (!DENOMINACIONES.contains(denominacion)) throw new ReglaNegocioException("Denominación no válida");
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor que cero");
        return denominacion * cantidad;
    }

    public Compra registrarCompra(String documento, long monto) {
        if (monto <= 0) throw new ReglaNegocioException("El monto debe ser mayor que cero");
        String estado = monto >= UMBRAL_CUMPLIMIENTO ? "EN_REVISION" : "APROBADA";
        return repo.save(new Compra("TX-" + (secuencia++), documento, monto, estado));
    }

    public static String mensajePara(Compra compra) {
        return "EN_REVISION".equals(compra.estado()) ? EN_REVISION : APROBADA;
    }
}
