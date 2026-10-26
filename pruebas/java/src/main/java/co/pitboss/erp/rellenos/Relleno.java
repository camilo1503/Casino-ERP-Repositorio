package co.pitboss.erp.rellenos;

public class Relleno {
    private final String id;
    private final String mesa;
    private final long monto;
    private String estado;
    private String aprobadoPor;

    public Relleno(String id, String mesa, long monto, String estado) {
        this.id = id;
        this.mesa = mesa;
        this.monto = monto;
        this.estado = estado;
    }

    public String getId() { return id; }
    public String getMesa() { return mesa; }
    public long getMonto() { return monto; }
    public String getEstado() { return estado; }
    public void setEstado(String estado) { this.estado = estado; }
    public String getAprobadoPor() { return aprobadoPor; }
    public void setAprobadoPor(String aprobadoPor) { this.aprobadoPor = aprobadoPor; }
}
