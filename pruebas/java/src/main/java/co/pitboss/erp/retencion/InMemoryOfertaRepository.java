package co.pitboss.erp.retencion;

import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;

public class InMemoryOfertaRepository implements OfertaRepository {
    private final List<OfertaRetencion> ofertas = new ArrayList<>();
    private final List<long[]> descuentos = new ArrayList<>();

    @Override public long countOffers(String documento, LocalDate fecha) {
        return ofertas.stream().filter(o -> o.documento().equals(documento) && o.fecha().equals(fecha)).count();
    }
    @Override public OfertaRetencion save(OfertaRetencion oferta) { ofertas.add(oferta); return oferta; }
    @Override public void saveDescuento(String documento, long monto, long descuento) { descuentos.add(new long[] {monto, descuento}); }
    @Override public List<OfertaRetencion> findByDocument(String documento) {
        return ofertas.stream().filter(o -> o.documento().equals(documento)).toList();
    }
    public int descuentosGuardados() { return descuentos.size(); }
}
