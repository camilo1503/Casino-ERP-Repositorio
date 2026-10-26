package co.pitboss.erp.bdd;

import co.pitboss.erp.caja.*;
import co.pitboss.erp.comun.ReglaNegocioException;
import co.pitboss.erp.inventario.*;
import co.pitboss.erp.mesas.*;
import co.pitboss.erp.perfiles.*;
import co.pitboss.erp.rellenos.*;
import co.pitboss.erp.retencion.*;
import java.time.Clock;
import java.time.LocalDate;
import java.time.ZoneId;
import java.util.function.Function;
import java.util.function.Supplier;

/** Estado de un escenario. PicoContainer crea uno nuevo por escenario y lo inyecta en todas las clases de pasos. */
public class Contexto {
    public static final ZoneId BOGOTA = ZoneId.of("America/Bogota");
    public final Clock reloj = Clock.fixed(LocalDate.of(2026, 10, 26).atStartOfDay(BOGOTA).toInstant(), BOGOTA);

    public final InMemoryJugadorRepository repoJugadores = new InMemoryJugadorRepository();
    public final InMemoryMesaRepository repoMesas = new InMemoryMesaRepository();
    public final InMemoryCompraRepository repoCompras = new InMemoryCompraRepository();
    public final InMemoryOfertaRepository repoOfertas = new InMemoryOfertaRepository();
    public final InMemoryRellenoRepository repoRellenos = new InMemoryRellenoRepository();
    public final InMemoryFichaRepository repoFichas = new InMemoryFichaRepository();

    public final JugadorService jugadores = new JugadorService(repoJugadores, reloj);
    public final MesaService mesas = new MesaService(repoMesas);
    public final CompraFichasService caja = new CompraFichasService(repoCompras);
    public final RetencionService retencion = new RetencionService(repoOfertas, reloj);
    public final RellenoService rellenos = new RellenoService(repoRellenos);
    public final InventarioFichasService inventario = new InventarioFichasService(repoFichas);

    public String usuario;
    public String mensaje;

    /** Ejecuta una acción del ERP y guarda el mensaje que vería el usuario. */
    public <T> T intentar(Supplier<T> accion, Function<T, String> mensajeExito) {
        try {
            T resultado = accion.get();
            mensaje = mensajeExito.apply(resultado);
            return resultado;
        } catch (ReglaNegocioException e) {
            mensaje = e.getMessage();
            return null;
        }
    }
}
