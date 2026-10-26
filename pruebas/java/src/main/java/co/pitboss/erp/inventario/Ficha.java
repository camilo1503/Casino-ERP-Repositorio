package co.pitboss.erp.inventario;

public class Ficha {
    private final String codigo;
    private final long denominacion;
    private final String categoria;
    private int stock;

    public Ficha(String codigo, long denominacion, String categoria, int stock) {
        this.codigo = codigo;
        this.denominacion = denominacion;
        this.categoria = categoria;
        this.stock = stock;
    }

    public String getCodigo() { return codigo; }
    public long getDenominacion() { return denominacion; }
    public String getCategoria() { return categoria; }
    public int getStock() { return stock; }
    public void setStock(int stock) { this.stock = stock; }
}
