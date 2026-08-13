using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Proyectos;
using Aplicacion.Funcionalidades.Tickets;
using Aplicacion.Funcionalidades.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace Aplicacion;

public static class DependencyInjection
{
    public static IServiceCollection AddAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IComentarioService, ComentarioService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        return services;
    }
}
