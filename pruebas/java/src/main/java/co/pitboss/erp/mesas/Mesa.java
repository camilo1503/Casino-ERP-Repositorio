package co.pitboss.erp.mesas;

public class Mesa {
    private final String numero;
    private final String juego;
    private long apuestaMinima;
    private long apuestaMaxima;
    private String estado = "CERRADA";
    private String motivoRetiro;

    public Mesa(String numero, String juego, long apuestaMinima, long apuestaMaxima) {
        this.numero = numero;
        this.juego = juego;
        this.apuestaMinima = apuestaMinima;
        this.apuestaMaxima = apuestaMaxima;
    }

    public String getNumero() { return numero; }
    public String getJuego() { return juego; }
    public long getApuestaMinima() { return apuestaMinima; }
    public long getApuestaMaxima() { return apuestaMaxima; }
    public void setLimites(long minima, long maxima) { this.apuestaMinima = minima; this.apuestaMaxima = maxima; }
    public String getEstado() { return estado; }
    public void setEstado(String estado) { this.estado = estado; }
    public String getMotivoRetiro() { return motivoRetiro; }
    public void setMotivoRetiro(String motivo) { this.motivoRetiro = motivo; }
}
