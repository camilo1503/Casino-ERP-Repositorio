package co.pitboss.erp.retencion;

import java.time.LocalDate;
import java.util.List;

public interface OfertaRepository {
    long countOffers(String documento, LocalDate fecha);
    OfertaRetencion save(OfertaRetencion oferta);
    void saveDescuento(String documento, long monto, long descuento);
    List<OfertaRetencion> findByDocument(String documento);
}
