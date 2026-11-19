using CasinoErp.Dominio.Compras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasinoErp.Infraestructura.Configuraciones;

/// <summary>Proveedores de fichas (HU-05, Compras).</summary>
public class ProveedorConfiguracion : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> e)
    {
        e.ToTable("Proveedores");
        e.HasIndex(p => p.Nit).IsUnique();
        e.Property(p => p.Nit).HasMaxLength(12).IsRequired();
        e.Property(p => p.Nombre).HasMaxLength(120).IsRequired();
        e.Property(p => p.Correo).HasMaxLength(160).IsRequired();
        e.Property(p => p.Telefono).HasMaxLength(30).IsRequired();
    }
}

/// <summary>Órdenes de compra de fichas (HU-05, Compras).</summary>
public class OrdenCompraConfiguracion : IEntityTypeConfiguration<OrdenCompra>
{
    public void Configure(EntityTypeBuilder<OrdenCompra> e)
    {
        e.ToTable("OrdenesCompra");
        e.Property(o => o.Estado).HasConversion<string>().HasMaxLength(15);
        e.Property(o => o.Origen).HasMaxLength(30);
        e.Property(o => o.CodigoFicha).HasMaxLength(20).IsRequired();
        e.Property(o => o.ProveedorNit).HasMaxLength(12);
        e.Ignore(o => o.Codigo);
    }
}
