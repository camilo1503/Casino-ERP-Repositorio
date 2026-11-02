namespace CasinoErp.Dominio.Rellenos;

public enum EstadoRelleno { Pendiente, Aprobado, Rechazado }

/// <summary>Solicitud de fichas que una mesa le hace a la bóveda.</summary>
public class Relleno
{
    public int Id { get; set; }
    public string MesaNumero { get; set; } = "";
    public long Monto { get; set; }
    public EstadoRelleno Estado { get; set; } = EstadoRelleno.Pendiente;
    public DateTime SolicitadoEn { get; set; }
    public string SolicitadoPor { get; set; } = "";
    public string? ResueltoPor { get; set; }
    public string? MotivoRechazo { get; set; }

    public string Codigo => $"RF-{Id:D4}";
}

/// <summary>Bóveda del casino: saldo disponible para rellenar las mesas.</summary>
public class Boveda
{
    public int Id { get; set; } = 1;
    public long Saldo { get; set; }
}

public interface IRellenoRepositorio
{
    Task<Relleno?> ObtenerAsync(int id);
    Task<IReadOnlyList<Relleno>> ListarAsync();
    Task<bool> ExistePendienteAsync(string mesaNumero);
    Task AgregarAsync(Relleno relleno);
    Task<Boveda> ObtenerBovedaAsync();
    Task GuardarCambiosAsync();
}
