package co.pitboss.erp.perfiles;

import java.time.LocalDate;

public class Jugador {
    private final String documento;
    private String nombre;
    private String correo;
    private final LocalDate fechaNacimiento;
    private int puntos;
    private String estado = "ACTIVO";
    private String motivoEstado;

    public Jugador(String documento, String nombre, String correo, LocalDate fechaNacimiento) {
        this.documento = documento;
        this.nombre = nombre;
        this.correo = correo;
        this.fechaNacimiento = fechaNacimiento;
    }

    public String getDocumento() { return documento; }
    public String getNombre() { return nombre; }
    public void setNombre(String nombre) { this.nombre = nombre; }
    public String getCorreo() { return correo; }
    public void setCorreo(String correo) { this.correo = correo; }
    public LocalDate getFechaNacimiento() { return fechaNacimiento; }
    public int getPuntos() { return puntos; }
    public void setPuntos(int puntos) { this.puntos = puntos; }
    public String getEstado() { return estado; }
    public void setEstado(String estado) { this.estado = estado; }
    public String getMotivoEstado() { return motivoEstado; }
    public void setMotivoEstado(String motivo) { this.motivoEstado = motivo; }
}
