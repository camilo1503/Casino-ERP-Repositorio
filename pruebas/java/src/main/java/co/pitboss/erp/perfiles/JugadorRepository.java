package co.pitboss.erp.perfiles;

import java.util.Optional;

public interface JugadorRepository {
    boolean existsByDocument(String documento);
    Optional<Jugador> findByDocument(String documento);
    Jugador save(Jugador jugador);
}
