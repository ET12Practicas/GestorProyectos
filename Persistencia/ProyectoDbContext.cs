using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;

namespace Persistencia;

public sealed class ProyectoDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
{
    public ProyectoDbContext(DbContextOptions<ProyectoDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProyectoDbContext).Assembly);

        modelBuilder.Entity<IdentityRole<Guid>>().HasData(new IdentityRole<Guid>
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            Name = "Administrador",
            NormalizedName = "ADMINISTRADOR",
            ConcurrencyStamp = "40000000-0000-0000-0000-000000000001"
        });
    }
}
