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
        }).RequireAuthorization("SoloAdministradores");

        group.MapGet("/{ticketId:guid}", async (ITicketService service, Guid ticketId, CancellationToken ct) =>
        {
            var ticket = await service.ObtenerTicketPorIdAsync(ticketId, ct);
            return ticket is not null ? Results.Ok(ticket) : Results.NotFound();
        }).RequireAuthorization();

        group.MapPut("/{ticketId:guid}/estado", async (
            ITicketService service,
            Guid ticketId,
            EstadoTicketDto dto,
            CancellationToken ct) =>
        {
            var result = await service.CambiarEstadoTicketAsync(ticketId, dto, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        group.MapPut("/{ticketId:guid}/usuario/{usuarioId:guid}", async (
            ITicketService service,
            Guid ticketId,
            Guid usuarioId,
            CancellationToken ct) =>
        {
            var result = await service.AsignarUsuarioATicketAsync(ticketId, usuarioId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        group.MapDelete("/{ticketId:guid}/usuario", async (
            ITicketService service,
            Guid ticketId,
            CancellationToken ct) =>
        {
            var result = await service.DesasignarUsuarioDeTicketAsync(ticketId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        group.MapDelete("/{ticketId:guid}", async (
            ITicketService service,
            Guid ticketId,
            CancellationToken ct) =>
        {
            var result = await service.EliminarTicketAsync(ticketId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }
}
