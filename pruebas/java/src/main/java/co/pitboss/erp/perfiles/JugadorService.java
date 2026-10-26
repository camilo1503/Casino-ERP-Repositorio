package co.pitboss.erp.perfiles;

import co.pitboss.erp.comun.ReglaNegocioException;
import java.time.Clock;
import java.time.LocalDate;
import java.time.Period;
import java.util.regex.Pattern;

/** E-01 Gestión de Perfiles de Jugador: CRUD con borrado lógico. */
public class JugadorService {
    public static final String REGISTRADO = "Jugador registrado exitosamente";
    public static final String DESACTIVADO = "Jugador desactivado";
    private static final Pattern CORREO = Pattern.compile("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$");
    private static final Pattern DOCUMENTO = Pattern.compile("^\\d{6,12}$");

    private final JugadorRepository repo;
    private final Clock reloj;

    public JugadorService(JugadorRepository repo, Clock reloj) {
        this.repo = repo;
        this.reloj = reloj;
    }

    public static void validarCorreo(String correo) {
        if (correo == null || !CORREO.matcher(correo).matches()) {
            throw new ReglaNegocioException("Formato de correo inválido");
        }
    }

    public Jugador registrar(Jugador jugador) {
        if (jugador.getNombre() == null || jugador.getNombre().isBlank()) throw new ReglaNegocioException("El nombre es obligatorio");
        if (jugador.getDocumento() == null || !DOCUMENTO.matcher(jugador.getDocumento()).matches()) throw new ReglaNegocioException("El documento no es válido");
        validarCorreo(jugador.getCorreo());
        if (jugador.getFechaNacimiento() == null) throw new ReglaNegocioException("La fecha de nacimiento es obligatoria");
        if (repo.existsByDocument(jugador.getDocumento())) throw new ReglaNegocioException("El documento ya está registrado");
        if (Period.between(jugador.getFechaNacimiento(), LocalDate.now(reloj)).getYears() < 18) {
            throw new ReglaNegocioException("El jugador debe ser mayor de 18 años");
        }
        jugador.setCorreo(jugador.getCorreo().trim().toLowerCase());
        jugador.setEstado("ACTIVO");
        return repo.save(jugador);
    }

    public Jugador consultar(String documento) {
        return repo.findByDocument(documento).orElseThrow(() -> new ReglaNegocioException("Jugador no encontrado"));
    }

    public Jugador actualizarCorreo(String documento, String correo) {
        validarCorreo(correo);
        Jugador jugador = consultar(documento);
        jugador.setCorreo(correo.toLowerCase());
        return repo.save(jugador);
    }

    public Jugador desactivar(String documento, String motivo) {
        return cambiarEstado(documento, motivo, "INACTIVO");
    }

    public Jugador autoexcluir(String documento, String motivo) {
        return cambiarEstado(documento, motivo, "AUTOEXCLUIDO");
    }

    private Jugador cambiarEstado(String documento, String motivo, String estado) {
        if (motivo == null || motivo.isBlank()) throw new ReglaNegocioException("El motivo es obligatorio");
        Jugador jugador = consultar(documento);
        jugador.setEstado(estado);
        jugador.setMotivoEstado(motivo.trim());
        return repo.save(jugador);
    }
}
