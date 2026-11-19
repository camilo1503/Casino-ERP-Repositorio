namespace CasinoErp.Dominio.Personal;

public enum Cargo { Crupier, Cajero, JefeDePiso }
public enum Turno { Manana, Tarde, Noche }
public enum TipoContrato { Indefinido, TerminoFijo, PrestacionServicios }

/// <summary>HU-04 Empleado del casino (contrato C4 de desarrollo/contratos.md).</summary>
public class Empleado
{
    public int Id { get; set; }
    public string Documento { get; set; } = "";      // solo dígitos, 6 a 10
    public string Nombre { get; set; } = "";
    public Cargo Cargo { get; set; }
    public string Juego { get; set; } = "";          // Blackjack|Ruleta|Póker|Baccarat|Craps para Crupier; "Caja"; "Supervisión"
    public Turno Turno { get; set; } = Turno.Tarde;
    public string Area { get; set; } = "Sin asignar";
    public TipoContrato TipoContrato { get; set; } = TipoContrato.Indefinido;
    public long SalarioBase { get; set; }
    public bool Activo { get; set; } = true;
    public string? MotivoRetiro { get; set; }
    public DateOnly FechaIngreso { get; set; }
}

public interface IEmpleadoRepositorio
{
    Task<bool> ExisteDocumentoAsync(string documento);
    Task<Empleado?> ObtenerPorDocumentoAsync(string documento);
    Task<IReadOnlyList<Empleado>> ListarAsync(bool soloActivos = false);
    Task AgregarAsync(Empleado empleado);
    Task GuardarCambiosAsync();
}
