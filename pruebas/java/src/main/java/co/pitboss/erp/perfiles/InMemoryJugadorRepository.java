package co.pitboss.erp.perfiles;

import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

public class InMemoryJugadorRepository implements JugadorRepository {
    private final Map<String, Jugador> datos = new HashMap<>();

    @Override public boolean existsByDocument(String documento) { return datos.containsKey(documento); }
    @Override public Optional<Jugador> findByDocument(String documento) { return Optional.ofNullable(datos.get(documento)); }
    @Override public Jugador save(Jugador jugador) { datos.put(jugador.getDocumento(), jugador); return jugador; }
}
