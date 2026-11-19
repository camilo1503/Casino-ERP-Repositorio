using CasinoErp.Dominio.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasinoErp.Infraestructura.Configuraciones;

/// <summary>Tabla de cuentas de acceso (HU-06, módulo Seguridad).</summary>
public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        e.ToTable("Usuarios");
        e.HasIndex(x => x.Correo).IsUnique();
        e.Property(x => x.Correo).HasMaxLength(160).IsRequired();
        e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        e.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
        e.Property(x => x.HashContrasena).HasMaxLength(200).IsRequired();
        e.Property(x => x.SecretoTotp).HasMaxLength(64).IsRequired();
        e.Property(x => x.EmpleadoDocumento).HasMaxLength(10);
    }
}
