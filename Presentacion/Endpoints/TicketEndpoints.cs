using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Tickets;
using Carter;

namespace Presentacion.Endpoints;

public sealed class TicketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets").WithTags("Tickets");

        group.MapGet("/", async (ITicketService service, CancellationToken ct) =>
        {
            var tickets = await service.ObtenerTicketsAsync(ct);
            return Results.Ok(tickets);
        });

        group.MapGet("/{ticketId:guid}", async (ITicketService service, Guid ticketId, CancellationToken ct) =>
        {
            var ticket = await service.ObtenerTicketPorIdAsync(ticketId, ct);
            return ticket is not null ? Results.Ok(ticket) : Results.NotFound();
        });

        group.MapPut("/{ticketId:guid}/estado", async (
            ITicketService service,
            Guid ticketId,
            EstadoTicketDto dto,
            CancellationToken ct) =>
        {
            var result = await service.CambiarEstadoTicketAsync(ticketId, dto, ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{ticketId:guid}/usuario/{usuarioId:guid}", async (
            ITicketService service,
            Guid ticketId,
            Guid usuarioId,
            CancellationToken ct) =>
        {
            var result = await service.AsignarUsuarioATicketAsync(ticketId, usuarioId, ct);
            return result.ToHttpResult();
        });

        group.MapDelete("/{ticketId:guid}/usuario", async (
            ITicketService service,
            Guid ticketId,
            CancellationToken ct) =>
        {
            var result = await service.DesasignarUsuarioDeTicketAsync(ticketId, ct);
            return result.ToHttpResult();
        });

        group.MapDelete("/{ticketId:guid}", async (
            ITicketService service,
            Guid ticketId,
            CancellationToken ct) =>
        {
            var result = await service.EliminarTicketAsync(ticketId, ct);
            return result.ToHttpResult();
        });

        // Sub-recursos de comentarios del ticket
        group.MapGet("/{ticketId:guid}/comentarios", async (
            IComentarioService comentarioService,
            Guid ticketId,
            CancellationToken ct) =>
        {
            var comentarios = await comentarioService.ObtenerComentariosPorTicketAsync(ticketId, ct);
            return Results.Ok(comentarios);
        });

        group.MapPost("/{ticketId:guid}/usuarios/{usuarioId:guid}/comentarios", async (
            IComentarioService comentarioService,
            Guid ticketId,
            Guid usuarioId,
            ComentarioCommandDto dto,
            CancellationToken ct) =>
        {
            var result = await comentarioService.CrearComentarioAsync(ticketId, usuarioId, dto, ct);
            return result.ToHttpResult($"/api/comentarios/{result.Value}");
        });
    }
}
