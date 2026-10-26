package co.pitboss.erp.mesas;

import java.util.Optional;

public interface MesaRepository {
    boolean existsByNumber(String numero);
    Optional<Mesa> findByNumber(String numero);
    Mesa save(Mesa mesa);
}
