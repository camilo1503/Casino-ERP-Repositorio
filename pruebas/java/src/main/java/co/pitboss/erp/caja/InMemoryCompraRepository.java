package co.pitboss.erp.caja;

import java.util.ArrayList;
import java.util.List;

public class InMemoryCompraRepository implements CompraRepository {
    private final List<Compra> datos = new ArrayList<>();

    @Override public Compra save(Compra compra) { datos.add(compra); return compra; }
    @Override public List<Compra> findAll() { return List.copyOf(datos); }
}
