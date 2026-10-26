package co.pitboss.erp.rellenos;

import java.util.Optional;

public interface RellenoRepository {
    Optional<Relleno> findById(String id);
    Relleno save(Relleno relleno);
    long getVaultBalance();
    void updateVaultBalance(long saldo);
}
