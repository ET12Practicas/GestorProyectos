using Dominio.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Persistencia;

public static class InicializadorBaseDeDatos
{
    public static async Task InicializarBaseDeDatosAsync(
        this IServiceProvider serviceProvider,
        bool esDesarrollo = false,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ProyectoDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProyectoDbContext>>();

        await contexto.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migraciones de base de datos aplicadas exitosamente.");

        if (esDesarrollo)
        {
            await SembrarDatosDesarrolloAsync(contexto, logger, cancellationToken);
        }

        await AsignarRolesAdministradoresAsync(scope.ServiceProvider, logger);
    }

    private static async Task SembrarDatosDesarrolloAsync(
        ProyectoDbContext contexto,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await contexto.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        var user1Id = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var user2Id = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var user3Id = Guid.Parse("10000000-0000-0000-0000-000000000003");

        var hasher = new PasswordHasher<Usuario>();
        var defaultPasswordHash = hasher.HashPassword(null!, "Password123!");

        var user1 = new Usuario("User_1")
        {
            Id = user1Id,
            UserName = "user1@demo.com",
            NormalizedUserName = "USER1@DEMO.COM",
            Email = "user1@demo.com",
            NormalizedEmail = "USER1@DEMO.COM",
            EmailConfirmed = true,
            PasswordHash = defaultPasswordHash,
            SecurityStamp = "10000000-0000-0000-0000-000000000001",
            ConcurrencyStamp = "10000000-0000-0000-0000-000000000001"
        };

        var user2 = new Usuario("User_2")
        {
            Id = user2Id,
            UserName = "user2@demo.com",
            NormalizedUserName = "USER2@DEMO.COM",
            Email = "user2@demo.com",
            NormalizedEmail = "USER2@DEMO.COM",
            EmailConfirmed = true,
            PasswordHash = defaultPasswordHash,
            SecurityStamp = "10000000-0000-0000-0000-000000000002",
            ConcurrencyStamp = "10000000-0000-0000-0000-000000000002"
        };

        var user3 = new Usuario("User_3")
        {
            Id = user3Id,
            UserName = "user3@demo.com",
            NormalizedUserName = "USER3@DEMO.COM",
            Email = "user3@demo.com",
            NormalizedEmail = "USER3@DEMO.COM",
            EmailConfirmed = true,
            PasswordHash = defaultPasswordHash,
            SecurityStamp = "10000000-0000-0000-0000-000000000003",
            ConcurrencyStamp = "10000000-0000-0000-0000-000000000003"
        };

        contexto.Usuarios.AddRange(user1, user2, user3);

        var p1 = new Proyecto("Proyecto_1") { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), CreadorId = user1Id };
        var p2 = new Proyecto("Proyecto_2") { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), CreadorId = user1Id };
        var p3 = new Proyecto("Proyecto_3") { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), CreadorId = user2Id };

        contexto.Proyectos.AddRange(p1, p2, p3);

        var t1 = new Ticket("Programacion", "Implementación inicial del módulo")
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            FechaTicket = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ProyectoId = p1.Id
        };
        var t2 = new Ticket("Analisis", "Definición y relevamiento de requerimientos")
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            FechaTicket = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ProyectoId = p1.Id
        };
        var t3 = new Ticket("Logica", "Desarrollo de lógica de negocio y validaciones")
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            FechaTicket = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ProyectoId = p2.Id
        };

        contexto.Tickets.AddRange(t1, t2, t3);
        await contexto.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Semillas demo de desarrollo cargadas exitosamente.");
    }

    private static async Task AsignarRolesAdministradoresAsync(
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        var userManager = serviceProvider.GetService<UserManager<Usuario>>();
        var configuration = serviceProvider.GetService<IConfiguration>();

        if (userManager is null || configuration is null) return;

        var adminEmails = configuration.GetSection("Administracion:EmailsAdministradores")
            .GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        foreach (var email in adminEmails)
        {
            if (string.IsNullOrWhiteSpace(email)) continue;

            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario is not null && !await userManager.IsInRoleAsync(usuario, "Administrador"))
            {
                var result = await userManager.AddToRoleAsync(usuario, "Administrador");
                if (result.Succeeded)
                {
                    logger.LogInformation("Rol Administrador asignado exitosamente al usuario {Email}.", email);
                }
                else
                {
                    logger.LogWarning("No se pudo asignar el rol Administrador al usuario {Email}: {Errores}",
                        email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
