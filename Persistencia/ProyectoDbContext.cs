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
    public DbSet<Comentario> Comentarios => Set<Comentario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProyectoDbContext).Assembly);

        var user1Id = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var user2Id = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var user3Id = Guid.Parse("10000000-0000-0000-0000-000000000003");

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario("User_1")
            {
                Id = user1Id,
                UserName = "user1@demo.com",
                NormalizedUserName = "USER1@DEMO.COM",
                Email = "user1@demo.com",
                NormalizedEmail = "USER1@DEMO.COM",
                EmailConfirmed = true,
                SecurityStamp = "10000000-0000-0000-0000-000000000001",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000001"
            },
            new Usuario("User_2")
            {
                Id = user2Id,
                UserName = "user2@demo.com",
                NormalizedUserName = "USER2@DEMO.COM",
                Email = "user2@demo.com",
                NormalizedEmail = "USER2@DEMO.COM",
                EmailConfirmed = true,
                SecurityStamp = "10000000-0000-0000-0000-000000000002",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000002"
            },
            new Usuario("User_3")
            {
                Id = user3Id,
                UserName = "user3@demo.com",
                NormalizedUserName = "USER3@DEMO.COM",
                Email = "user3@demo.com",
                NormalizedEmail = "USER3@DEMO.COM",
                EmailConfirmed = true,
                SecurityStamp = "10000000-0000-0000-0000-000000000003",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000003"
            }
        );

        modelBuilder.Entity<Proyecto>().HasData(
            new Proyecto("Proyecto_1") { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), CreadorId = user1Id },
            new Proyecto("Proyecto_2") { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), CreadorId = user1Id },
            new Proyecto("Proyecto_3") { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), CreadorId = user2Id }
        );

        modelBuilder.Entity<Ticket>().HasData(
            CrearTicketSemilla("20000000-0000-0000-0000-000000000001", "Programacion", "Implementación inicial del módulo"),
            CrearTicketSemilla("20000000-0000-0000-0000-000000000002", "Analisis", "Definición y relevamiento de requerimientos"),
            CrearTicketSemilla("20000000-0000-0000-0000-000000000003", "Logica", "Desarrollo de lógica de negocio y validaciones")
        );
    }

    private static Ticket CrearTicketSemilla(string id, string nombre, string descripcion)
    {
        return new Ticket(nombre, descripcion)
        {
            Id = Guid.Parse(id),
            FechaTicket = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
