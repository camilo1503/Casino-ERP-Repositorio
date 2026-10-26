package co.pitboss.erp.rellenos;

import co.pitboss.erp.comun.ReglaNegocioException;

/** Operación de mesas: aprobación de rellenos de fichas contra el saldo de la bóveda (Taller 3). */
public class RellenoService {
    public static final String APROBADO = "Relleno aprobado";

    private final RellenoRepository repo;

    public RellenoService(RellenoRepository repo) {
        this.repo = repo;
    }

    public Relleno aprobar(String id, String aprobadoPor) {
        Relleno relleno = repo.findById(id).orElseThrow(() -> new ReglaNegocioException("Relleno no encontrado"));
        if ("APROBADO".equals(relleno.getEstado())) throw new ReglaNegocioException("El relleno ya está aprobado");
        if (!"PENDIENTE".equals(relleno.getEstado())) throw new ReglaNegocioException("Solo se aprueban rellenos pendientes");
        long saldo = repo.getVaultBalance();
        if (relleno.getMonto() > saldo) throw new ReglaNegocioException("Saldo insuficiente en la bóveda");
        repo.updateVaultBalance(saldo - relleno.getMonto());
        relleno.setEstado("APROBADO");
        relleno.setAprobadoPor(aprobadoPor);
        return repo.save(relleno);
    }
}
