using Aplicacion.Funcionalidades.Comentarios;
using Carter;

namespace Presentacion.Endpoints;

public sealed class ComentarioEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/comentario", (IComentarioService service) =>
            Results.Ok(service.ObtenerComentarios()))
            .WithTags("Comentarios");
    }
}
