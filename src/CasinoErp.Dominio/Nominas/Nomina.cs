using CasinoErp.Dominio.Personal;

namespace CasinoErp.Dominio.Nominas;

public enum EstadoNomina { Borrador, Aprobada }

/// <summary>Lo que se le liquida a un empleado en una nómina mensual.</summary>
public class LiquidacionEmpleado
{
    public int Id { get; set; }
    public int NominaId { get; set; }
    public string EmpleadoDocumento { get; set; } = "";
    public string EmpleadoNombre { get; set; } = "";
    public Cargo Cargo { get; set; }
    public Turno Turno { get; set; }
    public TipoContrato TipoContrato { get; set; }
    public long SalarioBase { get; set; }
    public long RecargoNocturno { get; set; }
    public long AuxilioTransporte { get; set; }
    public long DeduccionSalud { get; set; }
    public long DeduccionPension { get; set; }
    public long FondoSolidaridad { get; set; }
    public long RetencionFuente { get; set; }
    public long AportesEmpleador { get; set; }
    public long Neto { get; set; }

    public long TotalDeducciones => DeduccionSalud + DeduccionPension + FondoSolidaridad + RetencionFuente;
}

/// <summary>HU-04 Nómina mensual: se calcula en borrador y luego se aprueba.</summary>
public class Nomina
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public EstadoNomina Estado { get; set; } = EstadoNomina.Borrador;
    public DateTime CalculadaEn { get; set; }
    public DateTime? AprobadaEn { get; set; }
    public List<LiquidacionEmpleado> Detalle { get; set; } = [];

    public long TotalNeto => Detalle.Sum(d => d.Neto);
    public long TotalDeducciones => Detalle.Sum(d => d.TotalDeducciones);
    public long TotalAportesEmpleador => Detalle.Sum(d => d.AportesEmpleador);
    public long TotalRecargos => Detalle.Sum(d => d.RecargoNocturno);
}

public interface INominaRepositorio
{
    Task<Nomina?> ObtenerAsync(int id);
    Task<Nomina?> ObtenerPorPeriodoAsync(int anio, int mes);
    Task<IReadOnlyList<Nomina>> ListarAsync();
    Task AgregarAsync(Nomina nomina);
    Task EliminarAsync(Nomina nomina);
    Task GuardarCambiosAsync();
}
