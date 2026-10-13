using Aplicacion.Funcionalidades.Auth;
using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Proyectos;
using Aplicacion.Funcionalidades.Tickets;
using Aplicacion.Funcionalidades.Usuarios;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Aplicacion;

public static class DependencyInjection
{
    public static IServiceCollection AddAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ProyectoCommandDto>, ProyectoCommandDtoValidator>();
        services.AddScoped<IValidator<TicketCommandDto>, TicketCommandDtoValidator>();
        services.AddScoped<IValidator<ComentarioCommandDto>, ComentarioCommandDtoValidator>();
        services.AddScoped<IValidator<UsuarioCommandDto>, UsuarioCommandDtoValidator>();
        services.AddScoped<IValidator<RegistroUsuarioDto>, RegistroUsuarioDtoValidator>();
        services.AddScoped<IValidator<LoginUsuarioDto>, LoginUsuarioDtoValidator>();

        services.AddScoped<IComentarioService, ComentarioService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        return services;
    }
}
