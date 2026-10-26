package co.pitboss.erp.caja;

import java.util.List;

public interface CompraRepository {
    Compra save(Compra compra);
    List<Compra> findAll();
}
