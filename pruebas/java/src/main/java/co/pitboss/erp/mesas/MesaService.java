package co.pitboss.erp.mesas;

import co.pitboss.erp.comun.ReglaNegocioException;
import java.util.List;
import java.util.regex.Pattern;

/** E-02 Catálogo de mesas: registrar, consultar, actualizar y retirar (borrado lógico). */
public class MesaService {
    public static final String REGISTRADA = "Mesa registrada exitosamente";
    public static final String ACTUALIZADA = "Mesa actualizada";
    public static final String RETIRADA = "Mesa retirada del catálogo";
    private static final Pattern NUMERO = Pattern.compile("^M-\\d{2}$");
    private static final List<String> JUEGOS = List.of("Blackjack", "Ruleta", "Póker", "Baccarat", "Craps");

    private final MesaRepository repo;

    public MesaService(MesaRepository repo) {
        this.repo = repo;
    }

    private static void validarLimites(long minima, long maxima) {
        if (minima <= 0) throw new ReglaNegocioException("La apuesta mínima debe ser mayor que cero");
        if (minima >= maxima) throw new ReglaNegocioException("La apuesta mínima debe ser menor que la máxima");
    }

    public Mesa registrar(Mesa mesa) {
        if (mesa.getNumero() == null || !NUMERO.matcher(mesa.getNumero()).matches()) {
            throw new ReglaNegocioException("El número de mesa debe tener el formato M-00");
        }
        if (!JUEGOS.contains(mesa.getJuego())) throw new ReglaNegocioException("El juego es obligatorio");
        validarLimites(mesa.getApuestaMinima(), mesa.getApuestaMaxima());
        if (repo.existsByNumber(mesa.getNumero())) throw new ReglaNegocioException("El número de mesa ya está registrado");
        mesa.setEstado("CERRADA");
        return repo.save(mesa);
    }

    public Mesa consultar(String numero) {
        return repo.findByNumber(numero).orElseThrow(() -> new ReglaNegocioException("Mesa no encontrada"));
    }

    public Mesa actualizarLimites(String numero, long minima, long maxima) {
        Mesa mesa = consultar(numero);
        if ("RETIRADA".equals(mesa.getEstado())) throw new ReglaNegocioException("La mesa está retirada");
        validarLimites(minima, maxima);
        mesa.setLimites(minima, maxima);
        return repo.save(mesa);
    }

    public Mesa retirar(String numero, String motivo) {
        Mesa mesa = consultar(numero);
        if ("ABIERTA".equals(mesa.getEstado())) throw new MesaAbiertaException();
        if (motivo == null || motivo.isBlank()) throw new ReglaNegocioException("El motivo es obligatorio");
        mesa.setEstado("RETIRADA");
        mesa.setMotivoRetiro(motivo.trim());
        return repo.save(mesa);
    }
}
