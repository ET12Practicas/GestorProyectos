using Aplicacion.Funcionalidades.Tickets;
using Carter;

namespace Presentacion.Endpoints;

public sealed class TicketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/ticket", (ITicketService service) =>
            Results.Ok(service.ObtenerTickets()))
            .WithTags("Tickets");
    }
}
