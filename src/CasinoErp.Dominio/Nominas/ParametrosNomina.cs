namespace CasinoErp.Dominio.Nominas;

/// <summary>Valores legales de la nómina de un año: salario mínimo, auxilio de transporte y tasas (D-003).</summary>
public class ParametrosNomina
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public long SalarioMinimo { get; set; }
    public long AuxilioTransporte { get; set; }
    public decimal SaludEmpleado { get; set; }
    public decimal PensionEmpleado { get; set; }
    public decimal FondoSolidaridad { get; set; }
    public decimal RecargoNocturno { get; set; }
    public decimal RetencionServicios { get; set; }
    public decimal SaludEmpleador { get; set; }
    public decimal PensionEmpleador { get; set; }
    public decimal Arl { get; set; }
    public decimal CajaCompensacion { get; set; }
    public decimal Icbf { get; set; }
    public decimal Sena { get; set; }

    /// <summary>Decretos 1469 y 1470 de 2025 (ver desarrollo/decisiones/D-003).</summary>
    public static ParametrosNomina Decreto2026() => new()
    {
        Anio = 2026,
        SalarioMinimo = 1_750_905,
        AuxilioTransporte = 249_095,
        SaludEmpleado = 0.04m,
        PensionEmpleado = 0.04m,
        FondoSolidaridad = 0.01m,
        RecargoNocturno = 0.35m,
        RetencionServicios = 0.10m,
        SaludEmpleador = 0.085m,
        PensionEmpleador = 0.12m,
        Arl = 0.00522m,
        CajaCompensacion = 0.04m,
        Icbf = 0.03m,
        Sena = 0.02m,
    };
}

public interface IParametrosNominaRepositorio
{
    Task<ParametrosNomina?> ObtenerAsync(int anio);
    Task GuardarAsync(ParametrosNomina p);
}
