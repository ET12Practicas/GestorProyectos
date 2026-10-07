using Microsoft.EntityFrameworkCore;
using Aplicacion.Dominio;

namespace Persistencia;

public sealed class ProyectoDbContext : DbContext
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

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario("User_1") { Id = Guid.Parse("10000000-0000-0000-0000-000000000001") },
            new Usuario("User_2") { Id = Guid.Parse("10000000-0000-0000-0000-000000000002") },
            new Usuario("User_3") { Id = Guid.Parse("10000000-0000-0000-0000-000000000003") }
        );

        modelBuilder.Entity<Proyecto>().HasData(
            new Proyecto("Proyecto_1") { Id = Guid.Parse("30000000-0000-0000-0000-000000000001") },
            new Proyecto("Proyecto_2") { Id = Guid.Parse("30000000-0000-0000-0000-000000000002") },
            new Proyecto("Proyecto_3") { Id = Guid.Parse("30000000-0000-0000-0000-000000000003") }
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
