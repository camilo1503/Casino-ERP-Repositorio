package co.pitboss.erp.retencion;

import co.pitboss.erp.comun.ReglaNegocioException;
import co.pitboss.erp.perfiles.Jugador;
import java.time.Clock;
import java.time.LocalDate;

/** E-03 Retención: oferta cuando el jugador quiere dejar de jugar (RF-PRO-006, RNF-12). */
public class RetencionService {
    public static final String EMITIDA = "Oferta de retención emitida";
    public static final int PORCENTAJE_DESCUENTO = 20;
    public static final long TOPE_DESCUENTO = 200_000L;
    public static final long FICHAS_BONO = 50_000L;
    public static final long BONO_RECLAMABLE = 100_000L;
    public static final int DIAS_VIGENCIA = 30;
    public static final int MINUTOS_MINIMOS = 30;
    public static final int OFERTAS_POR_DIA = 1;

    private final OfertaRepository repo;
    private final Clock reloj;
    private int secuencia = 1022;

    public RetencionService(OfertaRepository repo, Clock reloj) {
        this.repo = repo;
        this.reloj = reloj;
    }

    public long aplicarDescuento(String documento, long monto) {
        if (monto <= 0) throw new ReglaNegocioException("Monto inválido");
        long descuento = Math.min(Math.round(monto * PORCENTAJE_DESCUENTO / 100.0), TOPE_DESCUENTO);
        repo.saveDescuento(documento, monto, descuento);
        return descuento;
    }

    public static String nivel(int puntos) {
        if (puntos >= 10_000) return "Diamante";
        if (puntos >= 5_000) return "Oro";
        if (puntos >= 2_000) return "Plata";
        return "Bronce";
    }

    public static String ofertaSugerida(int puntos) {
        return switch (nivel(puntos)) {
            case "Oro" -> "FICHAS_BONO";
            case "Diamante" -> "BONO_RECLAMABLE";
            default -> "DESCUENTO_FICHAS";
        };
    }

    public OfertaRetencion evaluarSenal(Jugador jugador, int minutosSesion, String senal) {
        if (jugador == null) throw new ReglaNegocioException("Jugador sin perfil registrado");
        if (!"ACTIVO".equals(jugador.getEstado())) throw new ReglaNegocioException("No se ofrece retención: juego responsable");
        if (minutosSesion < MINUTOS_MINIMOS) throw new ReglaNegocioException("La sesión debe durar al menos " + MINUTOS_MINIMOS + " minutos");
        LocalDate hoy = LocalDate.now(reloj);
        if (repo.countOffers(jugador.getDocumento(), hoy) >= OFERTAS_POR_DIA) throw new ReglaNegocioException("El jugador ya recibió una oferta hoy");
        String tipo = ofertaSugerida(jugador.getPuntos());
        long valor = switch (tipo) {
            case "FICHAS_BONO" -> FICHAS_BONO;
            case "BONO_RECLAMABLE" -> BONO_RECLAMABLE;
            default -> PORCENTAJE_DESCUENTO;
        };
        return repo.save(new OfertaRetencion("RET-" + (secuencia++), jugador.getDocumento(), senal, tipo, valor,
                hoy, hoy.plusDays(DIAS_VIGENCIA), "EMITIDA"));
    }
}
