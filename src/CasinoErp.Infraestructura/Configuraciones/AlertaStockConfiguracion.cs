using CasinoErp.Dominio.Inventario;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasinoErp.Infraestructura.Configuraciones;

/// <summary>Alertas de stock crítico de la bóveda (HU-05).</summary>
public class AlertaStockConfiguracion : IEntityTypeConfiguration<AlertaStock>
{
    public void Configure(EntityTypeBuilder<AlertaStock> e)
    {
        e.ToTable("AlertasStock");
        e.Property(a => a.CodigoFicha).HasMaxLength(20).IsRequired();
        e.Property(a => a.Estado).HasConversion<string>().HasMaxLength(15);
        e.HasIndex(a => new { a.CodigoFicha, a.Estado });
    }
}
