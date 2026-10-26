package co.pitboss.erp.inventario;

import co.pitboss.erp.comun.ReglaNegocioException;
import java.util.regex.Pattern;

/** Inventario de fichas de la bóveda (Taller 4). */
public class InventarioFichasService {
    public static final String REGISTRADA = "Ficha registrada";
    public static final String RETIRO = "Retiro exitoso";
    public static final String ENTRADA = "Entrada registrada";
    private static final Pattern CODIGO = Pattern.compile("^FICHA-[0-9]+[KM]?$");

    private final FichaRepository repo;

    public InventarioFichasService(FichaRepository repo) {
        this.repo = repo;
    }

    public Ficha registrar(Ficha ficha) {
        if (ficha.getCodigo() == null || !CODIGO.matcher(ficha.getCodigo()).matches()) throw new ReglaNegocioException("El código de ficha no es válido");
        if (ficha.getDenominacion() <= 0) throw new ReglaNegocioException("La denominación debe ser mayor que cero");
        if (ficha.getCategoria() == null || ficha.getCategoria().isBlank()) throw new ReglaNegocioException("La categoría es obligatoria");
        if (ficha.getStock() < 0) throw new ReglaNegocioException("El stock inicial no puede ser negativo");
        if (repo.existsByCode(ficha.getCodigo())) throw new ReglaNegocioException("El código de ficha ya está registrado");
        return repo.save(ficha);
    }

    public Ficha consultar(String codigo) {
        return repo.findByCode(codigo).orElseThrow(() -> new ReglaNegocioException("Ficha no encontrada"));
    }

    public Ficha registrarEntrada(String codigo, int cantidad) {
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor que cero");
        Ficha ficha = consultar(codigo);
        ficha.setStock(ficha.getStock() + cantidad);
        repo.save(ficha);
        repo.addMovement(new Movimiento(codigo, "ENTRADA", cantidad, ficha.getStock()));
        return ficha;
    }

    public Ficha retirar(String codigo, int cantidad) {
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor que cero");
        Ficha ficha = consultar(codigo);
        if (cantidad > ficha.getStock()) throw new StockInsuficienteException();
        ficha.setStock(ficha.getStock() - cantidad);
        repo.save(ficha);
        repo.addMovement(new Movimiento(codigo, "SALIDA", cantidad, ficha.getStock()));
        return ficha;
    }
}
