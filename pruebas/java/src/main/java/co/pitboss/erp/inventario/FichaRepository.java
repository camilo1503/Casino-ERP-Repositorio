package co.pitboss.erp.inventario;

import java.util.List;
import java.util.Optional;

public interface FichaRepository {
    boolean existsByCode(String codigo);
    Optional<Ficha> findByCode(String codigo);
    Ficha save(Ficha ficha);
    void addMovement(Movimiento movimiento);
    List<Movimiento> movements();
}
