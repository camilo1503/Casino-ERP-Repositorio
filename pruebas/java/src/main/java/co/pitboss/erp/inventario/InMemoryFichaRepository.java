package co.pitboss.erp.inventario;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;

public class InMemoryFichaRepository implements FichaRepository {
    private final Map<String, Ficha> datos = new HashMap<>();
    private final List<Movimiento> movimientos = new ArrayList<>();

    @Override public boolean existsByCode(String codigo) { return datos.containsKey(codigo); }
    @Override public Optional<Ficha> findByCode(String codigo) { return Optional.ofNullable(datos.get(codigo)); }
    @Override public Ficha save(Ficha ficha) { datos.put(ficha.getCodigo(), ficha); return ficha; }
    @Override public void addMovement(Movimiento movimiento) { movimientos.add(movimiento); }
    @Override public List<Movimiento> movements() { return List.copyOf(movimientos); }
}
