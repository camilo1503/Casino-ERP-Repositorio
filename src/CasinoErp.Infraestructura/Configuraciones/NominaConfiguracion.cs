using CasinoErp.Dominio.Nominas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasinoErp.Infraestructura.Configuraciones;

/// <summary>Nómina mensual: una por periodo, con su detalle por empleado (HU-04).</summary>
public class NominaConfiguracion : IEntityTypeConfiguration<Nomina>
{
    public void Configure(EntityTypeBuilder<Nomina> e)
    {
        e.ToTable("Nominas");
        e.HasIndex(n => new { n.Anio, n.Mes }).IsUnique();
        e.Property(n => n.Estado).HasConversion<string>().HasMaxLength(20);
        e.HasMany(n => n.Detalle).WithOne().HasForeignKey(d => d.NominaId).OnDelete(DeleteBehavior.Cascade);
        e.Ignore(n => n.TotalNeto);
        e.Ignore(n => n.TotalDeducciones);
        e.Ignore(n => n.TotalAportesEmpleador);
        e.Ignore(n => n.TotalRecargos);
    }
}

/// <summary>Lo liquidado a cada empleado dentro de una nómina.</summary>
public class LiquidacionEmpleadoConfiguracion : IEntityTypeConfiguration<LiquidacionEmpleado>
{
    public void Configure(EntityTypeBuilder<LiquidacionEmpleado> e)
    {
        e.ToTable("LiquidacionesNomina");
        e.Property(d => d.EmpleadoDocumento).HasMaxLength(10).IsRequired();
        e.Property(d => d.EmpleadoNombre).HasMaxLength(120).IsRequired();
        e.Property(d => d.Cargo).HasConversion<string>().HasMaxLength(25);
        e.Property(d => d.Turno).HasConversion<string>().HasMaxLength(25);
        e.Property(d => d.TipoContrato).HasConversion<string>().HasMaxLength(25);
        e.Ignore(d => d.TotalDeducciones);
    }
}

/// <summary>Parámetros legales por año (D-003): salario mínimo, auxilio y tasas.</summary>
public class ParametrosNominaConfiguracion : IEntityTypeConfiguration<ParametrosNomina>
{
    public void Configure(EntityTypeBuilder<ParametrosNomina> e)
    {
        e.ToTable("ParametrosNomina");
        e.HasIndex(p => p.Anio).IsUnique();
        e.Property(p => p.SaludEmpleado).HasPrecision(7, 5);
        e.Property(p => p.PensionEmpleado).HasPrecision(7, 5);
        e.Property(p => p.FondoSolidaridad).HasPrecision(7, 5);
        e.Property(p => p.RecargoNocturno).HasPrecision(7, 5);
        e.Property(p => p.RetencionServicios).HasPrecision(7, 5);
        e.Property(p => p.SaludEmpleador).HasPrecision(7, 5);
        e.Property(p => p.PensionEmpleador).HasPrecision(7, 5);
        e.Property(p => p.Arl).HasPrecision(7, 5);
        e.Property(p => p.CajaCompensacion).HasPrecision(7, 5);
        e.Property(p => p.Icbf).HasPrecision(7, 5);
        e.Property(p => p.Sena).HasPrecision(7, 5);
    }
}
