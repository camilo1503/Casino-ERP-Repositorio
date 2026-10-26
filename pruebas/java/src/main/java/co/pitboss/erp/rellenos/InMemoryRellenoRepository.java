package co.pitboss.erp.rellenos;

import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

public class InMemoryRellenoRepository implements RellenoRepository {
    private final Map<String, Relleno> datos = new HashMap<>();
    private long saldoBoveda;

    @Override public Optional<Relleno> findById(String id) { return Optional.ofNullable(datos.get(id)); }
    @Override public Relleno save(Relleno relleno) { datos.put(relleno.getId(), relleno); return relleno; }
    @Override public long getVaultBalance() { return saldoBoveda; }
    @Override public void updateVaultBalance(long saldo) { this.saldoBoveda = saldo; }
}
