using System.Security.Claims;
using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Aplicacion.Funcionalidades.Proyectos;
using Aplicacion.Funcionalidades.Tickets;
using Carter;
using Microsoft.AspNetCore.Identity;

namespace Presentacion.Endpoints;

public sealed class ProyectoEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proyectos").WithTags("Proyectos");

        group.MapGet("/", async (IProyectoService service, CancellationToken ct) =>
        {
            var proyectos = await service.ObtenerProyectosAsync(ct);
            return Results.Ok(proyectos);
        });

        group.MapGet("/mis-proyectos", async (
            ICurrentUserService currentUser,
            IProyectoService service,
            CancellationToken ct) =>
        {
            if (!currentUser.UsuarioId.HasValue)
            {
                return Results.Unauthorized();
            }

            var proyectos = await service.ObtenerProyectosDelUsuarioAsync(currentUser.UsuarioId.Value, ct);
            return Results.Ok(proyectos);
        }).RequireAuthorization();

        group.MapGet("/usuario/{usuarioId:guid}", async (
            IProyectoService service,
            Guid usuarioId,
            CancellationToken ct) =>
        {
            var proyectos = await service.ObtenerProyectosDelUsuarioAsync(usuarioId, ct);
            return Results.Ok(proyectos);
        });

        group.MapGet("/{proyectoId:guid}", async (IProyectoService service, Guid proyectoId, CancellationToken ct) =>
        {
            var proyecto = await service.ObtenerProyectoPorIdAsync(proyectoId, ct);
            return proyecto is not null ? Results.Ok(proyecto) : Results.NotFound();
        });

        group.MapPost("/", async (
            IProyectoService service,
            ProyectoCommandDto dto,
            CancellationToken ct) =>
        {
            var result = await service.CrearProyectoAsync(dto, ct);
            return result.ToHttpResult($"/api/proyectos/{result.Value}");
        }).RequireAuthorization();

        group.MapPost("/{proyectoId:guid}/usuarios/{usuarioId:guid}", async (
            IProyectoService service,
            Guid proyectoId,
            Guid usuarioId,
            CancellationToken ct) =>
        {
            var result = await service.AsignarUsuarioAProyectoAsync(proyectoId, usuarioId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        group.MapDelete("/{proyectoId:guid}/usuarios/{usuarioId:guid}", async (
            IProyectoService service,
            Guid proyectoId,
            Guid usuarioId,
            CancellationToken ct) =>
        {
            var result = await service.QuitarUsuarioDeProyectoAsync(proyectoId, usuarioId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        group.MapDelete("/{proyectoId:guid}", async (IProyectoService service, Guid proyectoId, CancellationToken ct) =>
        {
            var result = await service.EliminarProyectoAsync(proyectoId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        // Sub-recursos de tickets para este proyecto
        group.MapGet("/{proyectoId:guid}/tickets", async (
            ITicketService ticketService,
            Guid proyectoId,
            CancellationToken ct) =>
        {
            var tickets = await ticketService.ObtenerTicketsPorProyectoAsync(proyectoId, ct);
            return Results.Ok(tickets);
        });

        group.MapPost("/{proyectoId:guid}/tickets", async (
            ITicketService ticketService,
            Guid proyectoId,
            TicketCommandDto dto,
            CancellationToken ct) =>
        {
            var result = await ticketService.CrearTicketAsync(proyectoId, dto, ct);
            return result.ToHttpResult($"/api/tickets/{result.Value}");
        }).RequireAuthorization();
    }
}
