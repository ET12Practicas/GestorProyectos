using Aplicacion.Funcionalidades.Comentarios;
using Carter;

namespace Presentacion.Endpoints;

public sealed class ComentarioEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comentarios").WithTags("Comentarios");

        group.MapGet("/", async (IComentarioService service, CancellationToken ct) =>
        {
            var comentarios = await service.ObtenerComentariosAsync(ct);
            return Results.Ok(comentarios);
        });

        group.MapDelete("/{comentarioId:guid}", async (
            IComentarioService service,
            Guid comentarioId,
            CancellationToken ct) =>
        {
            var result = await service.EliminarComentarioAsync(comentarioId, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }
}
