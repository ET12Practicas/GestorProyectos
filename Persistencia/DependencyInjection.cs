using Aplicacion.Abstracciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Persistencia;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistencia(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ProyectoDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 34))));

        services.AddScoped<IProyectoRepository, ProyectoRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IComentarioRepository, ComentarioRepository>();
        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return services;
    }
}
