using CasinoErp.Dominio.Personal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasinoErp.Infraestructura.Configuraciones;

/// <summary>Tabla de empleados (HU-04, módulo Personal).</summary>
public class EmpleadoConfiguracion : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> e)
    {
        e.ToTable("Empleados");
        e.HasIndex(x => x.Documento).IsUnique();
        e.Property(x => x.Documento).HasMaxLength(10).IsRequired();
        e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        e.Property(x => x.Juego).HasMaxLength(30);
        e.Property(x => x.Area).HasMaxLength(30);
        e.Property(x => x.Cargo).HasConversion<string>().HasMaxLength(25);
        e.Property(x => x.Turno).HasConversion<string>().HasMaxLength(25);
        e.Property(x => x.TipoContrato).HasConversion<string>().HasMaxLength(25);
        e.Property(x => x.MotivoRetiro).HasMaxLength(250);
    }
}
