package co.pitboss.erp.mesas;

import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

public class InMemoryMesaRepository implements MesaRepository {
    private final Map<String, Mesa> datos = new HashMap<>();

    @Override public boolean existsByNumber(String numero) { return datos.containsKey(numero); }
    @Override public Optional<Mesa> findByNumber(String numero) { return Optional.ofNullable(datos.get(numero)); }
    @Override public Mesa save(Mesa mesa) { datos.put(mesa.getNumero(), mesa); return mesa; }
}
