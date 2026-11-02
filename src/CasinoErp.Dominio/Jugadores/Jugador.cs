namespace CasinoErp.Dominio.Jugadores;

public enum EstadoJugador { Activo, Inactivo, Autoexcluido }

public enum NivelLealtad { Bronce, Plata, Oro, Diamante }

/// <summary>E-01 Perfil de jugador del casino.</summary>
public class Jugador
{
    public int Id { get; set; }
    public string TipoDocumento { get; set; } = "CC";
    public string Documento { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public DateOnly FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string Ciudad { get; set; } = "Bogotá";
    public List<string> Preferencias { get; set; } = [];
    /// <summary>Horario preferido: Mañana, Tarde o Noche.</summary>
    public string Franja { get; set; } = "Noche";
    public long ApuestaPromedio { get; set; } = 50_000;
    public int Puntos { get; set; }
    public EstadoJugador Estado { get; set; } = EstadoJugador.Activo;
    public string? MotivoEstado { get; set; }
    public DateTime FechaRegistro { get; set; }

    public NivelLealtad Nivel => Puntos switch
    {
        >= 10000 => NivelLealtad.Diamante,
        >= 5000 => NivelLealtad.Oro,
        >= 2000 => NivelLealtad.Plata,
        _ => NivelLealtad.Bronce,
    };
}
