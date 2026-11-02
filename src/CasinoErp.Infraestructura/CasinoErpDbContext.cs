using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Inventario;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Dominio.Retencion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CasinoErp.Infraestructura;

public class CasinoErpDbContext : DbContext
{
    public CasinoErpDbContext(DbContextOptions<CasinoErpDbContext> options) : base(options) { }

    public DbSet<Jugador> Jugadores => Set<Jugador>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<TransaccionCaja> Transacciones => Set<TransaccionCaja>();
    public DbSet<OfertaRetencion> Ofertas => Set<OfertaRetencion>();
    public DbSet<ConfiguracionRetencion> ConfiguracionRetencion => Set<ConfiguracionRetencion>();
    public DbSet<Relleno> Rellenos => Set<Relleno>();
    public DbSet<Boveda> Bovedas => Set<Boveda>();
    public DbSet<Ficha> Fichas => Set<Ficha>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jugador>(e =>
        {
            e.ToTable("Jugadores");
            e.HasIndex(j => j.Documento).IsUnique();
            e.HasIndex(j => j.Correo).IsUnique();
            e.Property(j => j.TipoDocumento).HasMaxLength(10).IsRequired();
            e.Property(j => j.Documento).HasMaxLength(12).IsRequired();
            e.Property(j => j.Telefono).HasMaxLength(30);
            e.Property(j => j.Ciudad).HasMaxLength(60);
            e.Property(j => j.Franja).HasMaxLength(10);
            // Las preferencias se guardan como texto separado por comas: "Blackjack,Ruleta"
            e.Property(j => j.Preferencias)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<List<string>>((a, b) => a!.SequenceEqual(b!), v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())), v => v.ToList()))
                .HasMaxLength(200);
            e.Property(j => j.Nombre).HasMaxLength(120).IsRequired();
            e.Property(j => j.Correo).HasMaxLength(160).IsRequired();
            e.Property(j => j.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(j => j.MotivoEstado).HasMaxLength(250);
            e.Ignore(j => j.Nivel);
        });

        modelBuilder.Entity<Mesa>(e =>
        {
            e.ToTable("Mesas");
            e.HasIndex(m => m.Numero).IsUnique();
            e.Property(m => m.Numero).HasMaxLength(5).IsRequired();
            e.Property(m => m.Juego).HasMaxLength(30).IsRequired();
            e.Property(m => m.Zona).HasMaxLength(20).IsRequired();
            e.Property(m => m.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(m => m.MotivoRetiro).HasMaxLength(250);
        });

        modelBuilder.Entity<TransaccionCaja>(e =>
        {
            e.ToTable("TransaccionesCaja");
            e.Property(t => t.Documento).HasMaxLength(12);
            e.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(10);
            e.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.Caja).HasMaxLength(20);
            e.Property(t => t.Ubicacion).HasMaxLength(60);
            e.Property(t => t.EstadoCumplimiento).HasMaxLength(20);
            e.HasIndex(t => t.Documento);
            e.Ignore(t => t.Codigo);
        });

        modelBuilder.Entity<OfertaRetencion>(e =>
        {
            e.ToTable("OfertasRetencion");
            e.Property(o => o.Documento).HasMaxLength(12).IsRequired();
            e.Property(o => o.Senal).HasMaxLength(120).IsRequired();
            e.Property(o => o.Origen).HasMaxLength(60).IsRequired();
            e.Property(o => o.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(o => o.Resultado).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(o => new { o.Documento, o.Fecha });
            e.Ignore(o => o.Codigo);
        });

        modelBuilder.Entity<ConfiguracionRetencion>(e =>
        {
            e.ToTable("ConfiguracionRetencion");
            e.Property(c => c.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Relleno>(e =>
        {
            e.ToTable("Rellenos");
            e.Property(r => r.MesaNumero).HasMaxLength(5).IsRequired();
            e.Property(r => r.SolicitadoPor).HasMaxLength(120);
            e.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.ResueltoPor).HasMaxLength(120);
            e.Property(r => r.MotivoRechazo).HasMaxLength(250);
            e.Ignore(r => r.Codigo);
        });

        modelBuilder.Entity<Boveda>(e =>
        {
            e.ToTable("Boveda");
            e.Property(b => b.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Ficha>(e =>
        {
            e.ToTable("Fichas");
            e.HasIndex(f => f.Codigo).IsUnique();
            e.Property(f => f.Codigo).HasMaxLength(20).IsRequired();
            e.Property(f => f.Categoria).HasMaxLength(40).IsRequired();
            e.Ignore(f => f.ValorTotal);
        });

        modelBuilder.Entity<MovimientoInventario>(e =>
        {
            e.ToTable("MovimientosInventario");
            e.Property(m => m.CodigoFicha).HasMaxLength(20).IsRequired();
            e.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(m => m.CodigoFicha);
        });
    }
}
