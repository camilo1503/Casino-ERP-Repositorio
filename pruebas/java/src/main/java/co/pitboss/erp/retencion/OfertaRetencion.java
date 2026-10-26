package co.pitboss.erp.retencion;

import java.time.LocalDate;

public record OfertaRetencion(String codigo, String documento, String senal, String tipo, long valor,
                              LocalDate fecha, LocalDate vence, String estado) { }
